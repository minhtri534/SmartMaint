using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Assets.Dtos;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Notifications;

namespace SmartMaint.Api.Modules.Assets;

/// <summary>
/// A06 / A07 (F5 page 2 → F1 page 4): closing a case that replaced an asset makes a request; the Administrator
/// registers the new asset ("replaces" the old one) and retires the old one in one step, or dismisses the request.
/// Staff and managers read the requests of their buildings.
/// </summary>
public interface IReplacementService
{
    Task<IReadOnlyList<ReplacementRequestResponse>> ListAsync(CancellationToken cancellationToken);

    Task<ReplacementRequestResponse> CompleteAsync(long id, CompleteReplacementRequest request, CancellationToken cancellationToken);

    Task<ReplacementRequestResponse> DismissAsync(long id, DismissReplacementRequest request, CancellationToken cancellationToken);
}

internal sealed class ReplacementService(
    AppDbContext db,
    ICurrentUser currentUser,
    IBuildingScope scope,
    IAuditService audit,
    INotificationService notifications,
    TimeProvider clock) : IReplacementService
{
    private static readonly CaseStatus[] FinishedStatuses = [CaseStatus.Closed, CaseStatus.Rejected, CaseStatus.Cancelled];

    public async Task<IReadOnlyList<ReplacementRequestResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken);
        var rows = await Query().AsNoTracking()
            .Where(r => buildingIds == null || buildingIds.Contains(r.OldAsset.Room.Floor.BuildingId))
            .OrderBy(r => r.Status).ThenByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
        return rows.Select(ToResponse).ToList();
    }

    public async Task<ReplacementRequestResponse> CompleteAsync(
        long id, CompleteReplacementRequest request, CancellationToken cancellationToken)
    {
        var r = await OpenAsync(id, cancellationToken);
        if (!await db.AssetTypes.AnyAsync(t => t.Id == request.TypeId, cancellationToken))
        {
            throw AssetErrors.UnknownType();
        }

        if (request.ManufacturerId is { } makerId && !await db.Manufacturers.AnyAsync(m => m.Id == makerId, cancellationToken))
        {
            throw AssetErrors.UnknownManufacturer();
        }

        var old = r.OldAsset;
        var name = request.Name.Trim();
        if (await db.Assets.AnyAsync(a => a.RoomId == old.RoomId && a.Name.ToLower() == name.ToLower(), cancellationToken))
        {
            throw AssetErrors.NameTaken();
        }

        var open = await db.Cases.CountAsync(c => c.AssetId == old.Id && !FinishedStatuses.Contains(c.Status), cancellationToken);
        if (open > 0)
        {
            throw AssetErrors.OpenCases(open);
        }

        var now = clock.GetUtcNow();
        var created = new Asset
        {
            Name = name,
            RoomId = old.RoomId,
            TypeId = request.TypeId,
            ManufacturerId = request.ManufacturerId,
            HasCopyright = request.HasCopyright,
            WarrantyEndDate = request.WarrantyEndDate,
            ReplacesAssetId = old.Id,
        };
        db.Assets.Add(created);
        var oldBefore = new { old.Id, old.Name, old.RetiredAt };
        old.RetiredAt ??= now;
        r.Status = ReplacementStatus.Done;
        r.NewAsset = created;
        r.FinishedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        audit.Record(AuditTarget.Asset, AuditOperation.Create, null,
            new { created.Id, created.Name, created.RoomId, created.TypeId, created.ReplacesAssetId, ReplacementRequestId = r.Id });
        audit.Record(AuditTarget.Asset, AuditOperation.Retire, oldBefore, new { old.Id, old.Name, old.RetiredAt, ReplacedBy = created.Id });
        notifications.Send([r.RequestedById], $"\"{created.Name}\" is registered in place of \"{old.Name}\", which is retired.");
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(await Query().AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken));
    }

    public async Task<ReplacementRequestResponse> DismissAsync(
        long id, DismissReplacementRequest request, CancellationToken cancellationToken)
    {
        var r = await OpenAsync(id, cancellationToken);
        r.Status = ReplacementStatus.Dismissed;
        r.DismissReason = request.Reason.Trim();
        r.FinishedAt = clock.GetUtcNow();
        notifications.Send([r.RequestedById], $"The replacement of \"{r.OldAsset.Name}\" was not registered: {r.DismissReason}");
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(await Query().AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken));
    }

    private async Task<AssetReplacementRequest> OpenAsync(long id, CancellationToken cancellationToken)
    {
        var r = await Query().SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw AssetErrors.ReplacementNotFound();
        return r.Status == ReplacementStatus.Open ? r : throw AssetErrors.ActionNotAllowed();
    }

    private IQueryable<AssetReplacementRequest> Query() => db.AssetReplacementRequests
        .Include(r => r.OldAsset).ThenInclude(a => a.Type)
        .Include(r => r.OldAsset).ThenInclude(a => a.Room).ThenInclude(rm => rm.Floor).ThenInclude(f => f.Building)
        .Include(r => r.NewAsset)
        .Include(r => r.RequestedBy);

    private ReplacementRequestResponse ToResponse(AssetReplacementRequest r) => new(
        r.Id,
        Case.CodeOf(r.CaseId),
        currentUser.Role == UserRole.Admin ? null : r.CaseId,
        new AssetRefResponse(r.OldAsset.Id, r.OldAsset.Name),
        r.OldAsset.TypeId,
        r.OldAsset.Type.Name,
        r.OldAsset.ManufacturerId,
        new AssetLocationResponse(
            r.OldAsset.Room.Floor.BuildingId, r.OldAsset.Room.Floor.Building.Name, r.OldAsset.Room.Floor.DisplayName,
            r.OldAsset.RoomId, r.OldAsset.Room.RoomNumber),
        r.NewModel,
        r.NewSerial,
        r.RequestedBy.FullName,
        r.CreatedAt,
        r.Status,
        r.NewAsset is null ? null : new AssetRefResponse(r.NewAsset.Id, r.NewAsset.Name),
        r.FinishedAt,
        r.DismissReason);
}
