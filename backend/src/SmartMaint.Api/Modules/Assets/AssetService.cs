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

internal static class AssetSort
{
    /// <summary>The list's order; assets without a warranty date come last.</summary>
    public static IQueryable<Asset> SortedBy(this IQueryable<Asset> rows, string? sort) => sort switch
    {
        "name_asc" => rows.OrderBy(a => a.Name).ThenBy(a => a.Id),
        "name_desc" => rows.OrderByDescending(a => a.Name).ThenBy(a => a.Id),
        "warranty_asc" => rows.OrderBy(a => a.WarrantyEndDate == null).ThenBy(a => a.WarrantyEndDate).ThenBy(a => a.Name),
        "warranty_desc" => rows.OrderBy(a => a.WarrantyEndDate == null).ThenByDescending(a => a.WarrantyEndDate).ThenBy(a => a.Name),
        _ => rows.OrderBy(a => a.Room.Floor.Building.Name).ThenBy(a => a.Room.Floor.FloorNumber).ThenBy(a => a.Room.RoomNumber)
            .ThenBy(a => a.Name),
    };
}

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
        if (!query.IncludeRetired && query.Status != AssetListStatus.Retired) rows = rows.Where(a => a.RetiredAt == null);
        rows = query.Status switch
        {
            AssetListStatus.Retired => rows.Where(a => a.RetiredAt != null),
            AssetListStatus.OpenCase => rows.Where(a => a.RetiredAt == null
                && db.Cases.Any(c => c.AssetId == a.Id && !FinishedStatuses.Contains(c.Status))),
            AssetListStatus.InService => rows.Where(a => a.RetiredAt == null
                && !db.Cases.Any(c => c.AssetId == a.Id && !FinishedStatuses.Contains(c.Status))),
            _ => rows,
        };
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
            .SortedBy(query.Sort)
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
        var actions = AllowedActions().ToList();
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

        // Meeting note 18/11: a customer may report any asset in service, wherever it is.
        var canReport = currentUser.Role == UserRole.Customer && !asset.IsRetired;

        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken);
        var canOpen = currentUser.Role is UserRole.Admin or UserRole.Staff or UserRole.Manager
                      && (buildingIds is null || buildingIds.Contains(asset.Room.Floor.BuildingId));

        return new QrTargetResponse(asset.Id, asset.Name, asset.TypeId, ToLocation(asset.Room), asset.IsRetired, canReport, canOpen);
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
        if (!await db.Rooms.AnyAsync(r => r.Id == request.RoomId && r.DeletedAt == null, cancellationToken)) throw AssetErrors.UnknownRoom();
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

    /// <summary>
    /// Meeting note 18/11: nobody changes an asset directly any more. The staff of its building put it in a change list,
    /// a manager approves it (the change list service then calls Update / Retire / Delete above).
    /// </summary>
    private IReadOnlyList<AssetAction> AllowedActions() =>
        currentUser.Role == UserRole.Staff ? [AssetAction.ProposeChange] : [];

    private AssetSummaryResponse ToSummary(Asset a, int openCases)
    {
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        return new AssetSummaryResponse(
            a.Id, a.Name, a.TypeId, a.Type.Name, a.Type.Category.CategoryName, a.Manufacturer?.Name, ToLocation(a.Room),
            a.WarrantyEndDate, a.WarrantyEndDate >= today, a.HasCopyright, a.RetiredAt, openCases);
    }

    private static AssetLocationResponse ToLocation(Room r) =>
        new(r.Floor.BuildingId, r.Floor.Building.Name, r.Floor.DisplayName, r.Id, r.RoomNumber);

    private static object Snapshot(Asset a) =>
        new { a.Id, a.Name, a.RoomId, a.TypeId, a.ManufacturerId, a.HasCopyright, a.WarrantyEndDate, a.RetiredAt };
}
