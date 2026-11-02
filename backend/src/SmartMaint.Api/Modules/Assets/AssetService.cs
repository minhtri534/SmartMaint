using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Assets.Dtos;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Facilities;

namespace SmartMaint.Api.Modules.Assets;

internal sealed class AssetService(
    AppDbContext db,
    ICurrentUser currentUser,
    IBuildingScope scope,
    IAuditService audit,
    TimeProvider clock) : IAssetService
{
    /// <summary>A case that is not finished yet (anything but closed, rejected, cancelled).</summary>
    private static readonly CaseStatus[] FinishedStatuses = [CaseStatus.Closed, CaseStatus.Rejected, CaseStatus.Cancelled];

    // ----- reads ------------------------------------------------------------------------------------------------

    public async Task<PagedResult<AssetSummaryResponse>> ListAsync(AssetListQuery query, CancellationToken cancellationToken)
    {
        var rows = await VisibleAsync(cancellationToken);
        if (!query.IncludeRetired) rows = rows.Where(a => a.RetiredAt == null);
        if (query.BuildingId is { } buildingId) rows = rows.Where(a => a.Room.Floor.BuildingId == buildingId);
        if (query.RoomId is { } roomId) rows = rows.Where(a => a.RoomId == roomId);
        if (query.CategoryId is { } categoryId) rows = rows.Where(a => a.Type.CategoryId == categoryId);
        if (query.TypeId is { } typeId) rows = rows.Where(a => a.TypeId == typeId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            rows = rows.Where(a => a.Name.ToLower().Contains(term));
        }

        var total = await rows.CountAsync(cancellationToken);
        var page = await rows
            .OrderBy(a => a.Room.Floor.Building.Name).ThenBy(a => a.Room.Floor.FloorNumber).ThenBy(a => a.Room.RoomNumber)
            .ThenBy(a => a.Name)
            .Skip(query.Skip).Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var openCases = await OpenCaseCountsAsync(page.Select(a => a.Id).ToList(), cancellationToken);
        var items = page.Select(a => ToSummary(a, openCases.GetValueOrDefault(a.Id))).ToList();
        return new PagedResult<AssetSummaryResponse>(items, query.Page, query.PageSize, total);
    }

    public async Task<AssetDetailResponse> GetAsync(long id, CancellationToken cancellationToken)
    {
        var asset = await (await VisibleAsync(cancellationToken)).SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw AssetErrors.NotFound();

        var cases = db.Cases.AsNoTracking().Where(c => c.AssetId == id);
        var caseCount = await cases.CountAsync(cancellationToken);
        var openCases = await cases.CountAsync(c => !FinishedStatuses.Contains(c.Status), cancellationToken);

        // BR-11: the Administrator never sees case content, only how many there are.
        IReadOnlyList<AssetCaseResponse>? history = null;
        if (currentUser.Role != UserRole.Admin)
        {
            history = (await cases
                    .OrderByDescending(c => c.SubmittedAt)
                    .Select(c => new { c.Id, c.Title, c.Status, c.SubmittedAt, c.ClosedAt, c.RootCause })
                    .ToListAsync(cancellationToken))
                .Select(c => new AssetCaseResponse(c.Id, Case.CodeOf(c.Id), c.Title, c.Status, c.SubmittedAt, c.ClosedAt, c.RootCause))
                .ToList();
        }

        // F1 page 3 / F5 way 2: how often it broke, and the replacement links (A07).
        var repairs = await AssetRepairStats.RepairsSinceAsync(db, id, clock.GetUtcNow() - AssetRepairStats.Window, cancellationToken);
        var frequent = repairs >= AssetRepairStats.FrequentFailureThreshold;
        var replaces = asset.ReplacesAssetId is { } oldId
            ? await db.Assets.AsNoTracking().Where(a => a.Id == oldId).Select(a => new AssetRefResponse(a.Id, a.Name))
                .SingleOrDefaultAsync(cancellationToken)
            : null;
        var replacedBy = await db.Assets.AsNoTracking().Where(a => a.ReplacesAssetId == id)
            .Select(a => new AssetRefResponse(a.Id, a.Name)).FirstOrDefaultAsync(cancellationToken);
        var openSurvey = await cases.AnyAsync(
            c => c.IsSurvey && c.Status != CaseStatus.Closed && c.Status != CaseStatus.Cancelled, cancellationToken);

        var summary = ToSummary(asset, openCases);
        var actions = AllowedActions(asset, caseCount, openCases).ToList();
        if (currentUser.Role == UserRole.Staff && frequent && !asset.IsRetired && !openSurvey)
        {
            actions.Add(AssetAction.OpenSurvey);
        }

        return new AssetDetailResponse(
            summary.Id, summary.Name, summary.TypeId, summary.TypeName, summary.CategoryName, summary.ManufacturerName,
            summary.Location, summary.WarrantyEndDate, summary.UnderWarranty, summary.HasCopyright, summary.RetiredAt,
            openCases, asset.ManufacturerId, asset.CreatedAt, caseCount, history,
            repairs, frequent, asset.Type.ExpectedLifeYears, replaces, replacedBy, actions);
    }

    public async Task<QrTargetResponse> GetQrTargetAsync(long id, CancellationToken cancellationToken)
    {
        var asset = await WithRefs(db.Assets.AsNoTracking()).SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw AssetErrors.NotFound();

        var canReport = false;
        if (currentUser.Role == UserRole.Customer && !asset.IsRetired)
        {
            // Same rule as reporting: own room, or a common area of the own building.
            var home = await db.Customers.AsNoTracking()
                .Where(c => c.Id == currentUser.Id)
                .Select(c => new { c.RoomId, c.Room.Floor.BuildingId })
                .SingleAsync(cancellationToken);
            canReport = asset.RoomId == home.RoomId
                        || (asset.Room.IsCommonArea && asset.Room.Floor.BuildingId == home.BuildingId);
        }

        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken);
        var canOpen = currentUser.Role is UserRole.Admin or UserRole.Staff or UserRole.Manager
                      && (buildingIds is null || buildingIds.Contains(asset.Room.Floor.BuildingId));

        return new QrTargetResponse(asset.Id, asset.Name, ToLocation(asset.Room), asset.IsRetired, canReport, canOpen);
    }

    // ----- writes (Administrator) -------------------------------------------------------------------------------

    public async Task<AssetDetailResponse> CreateAsync(AssetRequest request, CancellationToken cancellationToken)
    {
        await EnsureReferencesAsync(request, null, cancellationToken);
        var asset = new Asset();
        Apply(asset, request);
        db.Assets.Add(asset);
        await db.SaveChangesAsync(cancellationToken);

        audit.Record(AuditTarget.Asset, AuditOperation.Create, null, Snapshot(asset));
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(asset.Id, cancellationToken);
    }

    public async Task<AssetDetailResponse> UpdateAsync(long id, AssetRequest request, CancellationToken cancellationToken)
    {
        var asset = await db.Assets.SingleOrDefaultAsync(a => a.Id == id, cancellationToken) ?? throw AssetErrors.NotFound();
        await EnsureReferencesAsync(request, id, cancellationToken);

        var before = Snapshot(asset);
        Apply(asset, request);
        audit.Record(AuditTarget.Asset, AuditOperation.Update, before, Snapshot(asset));
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public async Task<AssetDetailResponse> RetireAsync(long id, CancellationToken cancellationToken)
    {
        var asset = await db.Assets.SingleOrDefaultAsync(a => a.Id == id, cancellationToken) ?? throw AssetErrors.NotFound();
        if (asset.IsRetired)
        {
            throw AssetErrors.ActionNotAllowed();
        }

        var open = await db.Cases.CountAsync(c => c.AssetId == id && !FinishedStatuses.Contains(c.Status), cancellationToken);
        if (open > 0)
        {
            throw AssetErrors.OpenCases(open);
        }

        var before = Snapshot(asset);
        asset.RetiredAt = clock.GetUtcNow();
        audit.Record(AuditTarget.Asset, AuditOperation.Retire, before, Snapshot(asset));
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var asset = await db.Assets.SingleOrDefaultAsync(a => a.Id == id, cancellationToken) ?? throw AssetErrors.NotFound();
        if (await db.Cases.AnyAsync(c => c.AssetId == id, cancellationToken))
        {
            throw AssetErrors.HasHistory();
        }

        db.Assets.Remove(asset);
        audit.Record(AuditTarget.Asset, AuditOperation.Delete, Snapshot(asset), null);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ImportAssetsResponse> ImportAsync(ImportAssetsRequest request, CancellationToken cancellationToken)
    {
        List<AssetCsv.Row> rows;
        try
        {
            rows = AssetCsv.Parse(request.Csv);
        }
        catch (FormatException e)
        {
            return new ImportAssetsResponse(0, 1, false, [new ImportRowResult(1, string.Empty, [e.Message])]);
        }

        if (rows.Count > AssetCsv.MaxRows)
        {
            return new ImportAssetsResponse(rows.Count, 1, false,
                [new ImportRowResult(1, string.Empty, [$"At most {AssetCsv.MaxRows} rows per file."])]);
        }

        // Look-ups loaded once: room by "building|floor|room", type by code, manufacturer by name (case-insensitive).
        var rooms = (await db.Rooms.AsNoTracking()
                .Select(r => new { r.Id, Building = r.Floor.Building.Name, r.Floor.FloorNumber, r.RoomNumber })
                .ToListAsync(cancellationToken))
            .ToDictionary(r => Key(r.Building, r.FloorNumber, r.RoomNumber), r => r.Id);
        var types = await db.AssetTypes.AsNoTracking().ToDictionaryAsync(t => t.Code.ToLower(), t => t.Id, cancellationToken);
        var makers = await db.Manufacturers.AsNoTracking().ToDictionaryAsync(m => m.Name.ToLower(), m => m.Id, cancellationToken);
        var existing = (await db.Assets.AsNoTracking().Select(a => new { a.RoomId, a.Name }).ToListAsync(cancellationToken))
            .Select(a => (a.RoomId, a.Name.ToLower())).ToHashSet();

        var results = new List<ImportRowResult>();
        var assets = new List<Asset>();
        foreach (var row in rows)
        {
            var errors = new List<string>();
            var name = row["name"];
            if (name.Length is < 2 or > 150) errors.Add("Name must have 2 to 150 characters.");
            if (!rooms.TryGetValue(Key(row["building"], row["floor"], row["room"]), out var roomId))
                errors.Add($"No room \"{row["room"]}\" on floor \"{row["floor"]}\" of \"{row["building"]}\".");
            if (!types.TryGetValue(row["type_code"].ToLowerInvariant(), out var typeId))
                errors.Add($"Unknown asset type code \"{row["type_code"]}\".");

            long? makerId = null;
            if (row["manufacturer"].Length > 0)
            {
                if (makers.TryGetValue(row["manufacturer"].ToLowerInvariant(), out var m)) makerId = m;
                else errors.Add($"Unknown manufacturer \"{row["manufacturer"]}\".");
            }

            var copyright = row["has_copyright"].ToLowerInvariant() switch
            {
                "" or "no" or "false" or "0" => false,
                "yes" or "true" or "1" => true,
                _ => (bool?)null,
            };
            if (copyright is null) errors.Add("has_copyright must be yes or no.");

            DateOnly? warranty = null;
            if (row["warranty_end_date"].Length > 0)
            {
                if (DateOnly.TryParseExact(row["warranty_end_date"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                    warranty = d;
                else errors.Add("warranty_end_date must look like 2027-06-30.");
            }

            // Same room + same name = duplicate, against the database and against earlier rows of the file.
            if (errors.Count == 0 && !existing.Add((roomId, name.ToLowerInvariant())))
                errors.Add("This room already has an asset with this name.");

            results.Add(new ImportRowResult(row.Line, name, errors));
            if (errors.Count == 0)
            {
                assets.Add(new Asset
                {
                    Name = name, RoomId = roomId, TypeId = typeId, ManufacturerId = makerId, HasCopyright = copyright!.Value,
                    WarrantyEndDate = warranty,
                });
            }
        }

        var errorCount = results.Count(r => r.Errors.Count > 0);
        var import = !request.DryRun && errorCount == 0 && assets.Count > 0;
        if (import)
        {
            db.Assets.AddRange(assets);
            audit.Record(AuditTarget.Asset, AuditOperation.Import, null, new { Count = assets.Count, Names = assets.Select(a => a.Name) });
            await db.SaveChangesAsync(cancellationToken);
        }

        return new ImportAssetsResponse(rows.Count, errorCount, import, results);
    }

    // ----- helpers ----------------------------------------------------------------------------------------------

    private async Task<IQueryable<Asset>> VisibleAsync(CancellationToken cancellationToken)
    {
        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken);
        return WithRefs(db.Assets.AsNoTracking())
            .Where(a => buildingIds == null || buildingIds.Contains(a.Room.Floor.BuildingId));
    }

    private static IQueryable<Asset> WithRefs(IQueryable<Asset> query) => query
        .Include(a => a.Room).ThenInclude(r => r.Floor).ThenInclude(f => f.Building)
        .Include(a => a.Type).ThenInclude(t => t.Category)
        .Include(a => a.Manufacturer);

    private async Task<Dictionary<long, int>> OpenCaseCountsAsync(List<long> assetIds, CancellationToken cancellationToken) =>
        await db.Cases.AsNoTracking()
            .Where(c => c.AssetId != null && assetIds.Contains(c.AssetId.Value) && !FinishedStatuses.Contains(c.Status))
            .GroupBy(c => c.AssetId!.Value)
            .Select(g => new { AssetId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AssetId, x => x.Count, cancellationToken);

    private async Task EnsureReferencesAsync(AssetRequest request, long? exceptId, CancellationToken cancellationToken)
    {
        if (!await db.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken)) throw AssetErrors.UnknownRoom();
        if (!await db.AssetTypes.AnyAsync(t => t.Id == request.TypeId, cancellationToken)) throw AssetErrors.UnknownType();
        if (request.ManufacturerId is { } makerId && !await db.Manufacturers.AnyAsync(m => m.Id == makerId, cancellationToken))
            throw AssetErrors.UnknownManufacturer();

        var name = request.Name.Trim().ToLower();
        if (await db.Assets.AnyAsync(a => a.RoomId == request.RoomId && a.Name.ToLower() == name && a.Id != exceptId, cancellationToken))
            throw AssetErrors.NameTaken();
    }

    private static void Apply(Asset asset, AssetRequest request)
    {
        asset.Name = request.Name.Trim();
        asset.RoomId = request.RoomId;
        asset.TypeId = request.TypeId;
        asset.ManufacturerId = request.ManufacturerId;
        asset.HasCopyright = request.HasCopyright;
        asset.WarrantyEndDate = request.WarrantyEndDate;
    }

    private IReadOnlyList<AssetAction> AllowedActions(Asset asset, int caseCount, int openCases)
    {
        if (currentUser.Role != UserRole.Admin) return [];
        var actions = new List<AssetAction> { AssetAction.Edit };
        if (!asset.IsRetired && openCases == 0) actions.Add(AssetAction.Retire);
        if (caseCount == 0) actions.Add(AssetAction.Delete);
        return actions;
    }

    private AssetSummaryResponse ToSummary(Asset a, int openCases)
    {
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        return new AssetSummaryResponse(
            a.Id, a.Name, a.TypeId, a.Type.Name, a.Type.Category.CategoryName, a.Manufacturer?.Name, ToLocation(a.Room),
            a.WarrantyEndDate, a.WarrantyEndDate >= today, a.HasCopyright, a.RetiredAt, openCases);
    }

    private static AssetLocationResponse ToLocation(Room r) =>
        new(r.Floor.BuildingId, r.Floor.Building.Name, r.Floor.DisplayName, r.Id, r.RoomNumber);

    private static string Key(string building, string floor, string room) =>
        $"{building.Trim().ToLowerInvariant()}|{floor.Trim().ToLowerInvariant()}|{room.Trim().ToLowerInvariant()}";

    private static object Snapshot(Asset a) =>
        new { a.Id, a.Name, a.RoomId, a.TypeId, a.ManufacturerId, a.HasCopyright, a.WarrantyEndDate, a.RetiredAt };
}
