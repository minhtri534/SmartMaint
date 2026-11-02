using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Maintenance.Dtos;
using SmartMaint.Api.Modules.Notifications;

namespace SmartMaint.Api.Modules.Maintenance;

/// <summary>
/// F2 plans (PM-01..03). The staff of a building makes a plan, a manager of that building approves or rejects it (E10),
/// then it runs until paused or ended. Any edit sends it back for approval. The building of a plan is the building of its
/// assets (the ERD has no building column on MaintenanceTriggers).
/// </summary>
public interface IMaintenancePlanService
{
    Task<IReadOnlyList<PlanSummaryResponse>> ListAsync(CancellationToken cancellationToken);

    Task<PlanDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff. The plan waits for a manager's approval.</summary>
    Task<PlanDetailResponse> CreateAsync(PlanRequest request, CancellationToken cancellationToken);

    /// <summary>Staff. Back to "pending approval".</summary>
    Task<PlanDetailResponse> UpdateAsync(long id, PlanRequest request, CancellationToken cancellationToken);

    Task<PlanDetailResponse> ApproveAsync(long id, CancellationToken cancellationToken);
    Task<PlanDetailResponse> RejectAsync(long id, RejectPlanRequest request, CancellationToken cancellationToken);
    Task<PlanDetailResponse> PauseAsync(long id, CancellationToken cancellationToken);

    /// <summary>Due days missed while paused are skipped; the next one counts from today.</summary>
    Task<PlanDetailResponse> ResumeAsync(long id, CancellationToken cancellationToken);

    /// <summary>No new work orders; the ones already made go on.</summary>
    Task<PlanDetailResponse> EndAsync(long id, CancellationToken cancellationToken);
}

internal sealed class MaintenancePlanService(
    AppDbContext db,
    ICurrentUser currentUser,
    IBuildingScope scope,
    INotificationService notifications,
    TimeProvider clock) : IMaintenancePlanService
{
    // ----- reads ------------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<PlanSummaryResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var plans = await VisibleAsync(asTracking: false, cancellationToken);
        return plans.OrderBy(p => p.Status).ThenBy(p => p.Id).Select(p => (PlanSummaryResponse)ToDetail(p, [])).ToList();
    }

    public async Task<PlanDetailResponse> GetAsync(long id, CancellationToken cancellationToken)
    {
        var plan = (await VisibleAsync(asTracking: false, cancellationToken)).SingleOrDefault(p => p.Id == id)
            ?? throw MaintenanceErrors.PlanNotFound();

        var detailIds = plan.Details.Select(d => d.Id).ToList();
        var workOrders = (await db.Cases.AsNoTracking()
                .Where(c => c.TriggerWorkOrderId != null && detailIds.Contains(c.TriggerWorkOrderId.Value))
                .OrderByDescending(c => c.SubmittedAt).Take(20)
                .Select(c => new { c.Id, AssetName = c.Asset!.Name, c.Status, c.DueAt, c.ClosedAt })
                .ToListAsync(cancellationToken))
            .Select(c => new PlanWorkOrderResponse(c.Id, Case.CodeOf(c.Id), c.AssetName, c.Status, c.DueAt, c.ClosedAt))
            .ToList();

        return ToDetail(plan, workOrders);
    }

    // ----- writes -----------------------------------------------------------------------------------------------

    public async Task<PlanDetailResponse> CreateAsync(PlanRequest request, CancellationToken cancellationToken)
    {
        var buildingId = await CheckRequestAsync(request, cancellationToken);
        var plan = new MaintenanceTrigger { StaffId = currentUser.Id, Status = TriggerStatus.PendingApproval };
        Apply(plan, new MaintenanceWorkOrderDetail(), request);
        db.MaintenanceTriggers.Add(plan);
        await NotifyManagersAsync(buildingId, "A new maintenance plan waits for your approval.", cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(plan.Id, cancellationToken);
    }

    public async Task<PlanDetailResponse> UpdateAsync(long id, PlanRequest request, CancellationToken cancellationToken)
    {
        var plan = await FindAsync(id, PlanAction.Edit, cancellationToken);
        var buildingId = await CheckRequestAsync(request, cancellationToken);

        // One plan = one work-order detail (the screen offers one checklist per plan); the ERD allows more.
        Apply(plan, plan.Details.Single(), request);
        plan.Status = TriggerStatus.PendingApproval;
        plan.ApprovingManagerId = null;
        plan.RejectReason = null;
        await NotifyManagersAsync(buildingId, $"Maintenance plan #{id} was changed and waits for your approval again.", cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public Task<PlanDetailResponse> ApproveAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, PlanAction.Approve, plan =>
        {
            plan.Status = TriggerStatus.Active;
            plan.ApprovingManagerId = currentUser.Id;
            plan.RejectReason = null;
            notifications.Send([plan.StaffId], $"Maintenance plan #{id} was approved and is running.");
        }, cancellationToken);

    public Task<PlanDetailResponse> RejectAsync(long id, RejectPlanRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, PlanAction.Reject, plan =>
        {
            plan.Status = TriggerStatus.Rejected;
            plan.ApprovingManagerId = currentUser.Id;
            plan.RejectReason = request.Reason.Trim();
            notifications.Send([plan.StaffId], $"Maintenance plan #{id} was rejected: {plan.RejectReason}");
        }, cancellationToken);

    public Task<PlanDetailResponse> PauseAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, PlanAction.Pause, plan =>
        {
            plan.Status = TriggerStatus.Paused;
            plan.LastPaused = clock.GetUtcNow();
        }, cancellationToken);

    public Task<PlanDetailResponse> ResumeAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, PlanAction.Resume, plan =>
        {
            plan.Status = TriggerStatus.Active;
            var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
            foreach (var asset in plan.Details.SelectMany(d => d.Assets))
            {
                // Skip the due days that passed while paused: mark the last one before today as done.
                while (MaintenanceSchedule.NextDue(asset, plan) is { } due && due < today)
                {
                    asset.LastDueDate = due;
                }
            }
        }, cancellationToken);

    public Task<PlanDetailResponse> EndAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, PlanAction.End, plan => plan.Status = TriggerStatus.Ended, cancellationToken);

    private async Task<PlanDetailResponse> ActAsync(
        long id, PlanAction action, Action<MaintenanceTrigger> apply, CancellationToken cancellationToken)
    {
        var plan = await FindAsync(id, action, cancellationToken);
        apply(plan);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    // ----- rules ------------------------------------------------------------------------------------------------

    /// <summary>Who may do what: staff of the building edit / pause / resume / end; managers of the building approve.</summary>
    private IReadOnlyList<PlanAction> AllowedActions(MaintenanceTrigger plan) => currentUser.Role switch
    {
        UserRole.Staff => plan.Status switch
        {
            TriggerStatus.PendingApproval or TriggerStatus.Rejected => [PlanAction.Edit, PlanAction.End],
            TriggerStatus.Active => [PlanAction.Edit, PlanAction.Pause, PlanAction.End],
            TriggerStatus.Paused => [PlanAction.Edit, PlanAction.Resume, PlanAction.End],
            _ => [],
        },
        UserRole.Manager when plan.Status == TriggerStatus.PendingApproval => [PlanAction.Approve, PlanAction.Reject],
        _ => [],
    };

    private async Task<MaintenanceTrigger> FindAsync(long id, PlanAction action, CancellationToken cancellationToken)
    {
        var plan = (await VisibleAsync(asTracking: true, cancellationToken)).SingleOrDefault(p => p.Id == id)
            ?? throw MaintenanceErrors.PlanNotFound();
        return AllowedActions(plan).Contains(action) ? plan : throw MaintenanceErrors.ActionNotAllowed();
    }

    /// <summary>The plans of the caller's buildings (B3). Small numbers: filtered in memory after one query.</summary>
    private async Task<List<MaintenanceTrigger>> VisibleAsync(bool asTracking, CancellationToken cancellationToken)
    {
        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken);
        var query = db.MaintenanceTriggers
            .Include(p => p.Staff)
            .Include(p => p.ApprovingManager)
            .Include(p => p.Details).ThenInclude(d => d.Checklist)
            .Include(p => p.Details).ThenInclude(d => d.PriorityLevel)
            .Include(p => p.Details).ThenInclude(d => d.Assets).ThenInclude(a => a.Technician)
            .Include(p => p.Details).ThenInclude(d => d.Assets).ThenInclude(a => a.Asset)
            .ThenInclude(a => a.Room).ThenInclude(r => r.Floor).ThenInclude(f => f.Building)
            .AsSplitQuery();
        var plans = asTracking ? await query.ToListAsync(cancellationToken) : await query.AsNoTracking().ToListAsync(cancellationToken);
        return plans.Where(p => buildingIds is not null && BuildingOf(p) is { } b && buildingIds.Contains(b.Id)).ToList();
    }

    private static Entities.Buildings.Building? BuildingOf(MaintenanceTrigger plan) =>
        plan.Details.SelectMany(d => d.Assets).Select(a => a.Asset.Room.Floor.Building).FirstOrDefault();

    /// <summary>Checks the request and returns the building of its assets (one building the caller looks after).</summary>
    private async Task<long> CheckRequestAsync(PlanRequest request, CancellationToken cancellationToken)
    {
        if (!await db.CaseChecklists.AnyAsync(c => c.Id == request.ChecklistId, cancellationToken))
        {
            throw MaintenanceErrors.ChecklistNotFound();
        }

        var assetIds = request.Assets.Select(a => a.AssetId).ToList();
        var assets = await db.Assets.AsNoTracking()
            .Where(a => assetIds.Contains(a.Id) && a.RetiredAt == null)
            .Select(a => new { a.Id, a.Room.Floor.BuildingId })
            .ToListAsync(cancellationToken);
        if (assets.Count != assetIds.Distinct().Count())
        {
            throw MaintenanceErrors.UnknownAsset();
        }

        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken) ?? [];
        var building = assets.Select(a => a.BuildingId).Distinct().ToList();
        if (building.Count != 1 || !buildingIds.Contains(building[0]))
        {
            throw MaintenanceErrors.AssetsInSeveralBuildings();
        }

        var technicianIds = request.Assets.Where(a => a.TechnicianId is not null).Select(a => a.TechnicianId!.Value).Distinct().ToList();
        if (await db.Technicians.CountAsync(t => technicianIds.Contains(t.Id) && t.Status == UserStatus.Active, cancellationToken)
            != technicianIds.Count)
        {
            throw MaintenanceErrors.UnknownTechnician();
        }

        return building[0];
    }

    private void Apply(MaintenanceTrigger plan, MaintenanceWorkOrderDetail detail, PlanRequest request)
    {
        plan.Frequency = request.Frequency;
        plan.FrequencyType = request.FrequencyType;
        plan.LeadDays = request.LeadDays;
        detail.ChecklistId = request.ChecklistId;
        detail.EstimatedHours = request.EstimatedHours;
        detail.PriorityLevelId = db.PriorityLevels.Single(p => p.Code == request.Priority).Id;

        // Change only what differs: an asset that stays keeps its schedule position (LastDueDate), so an edit never
        // makes a work order twice; removing and re-adding the same (detail, asset) key in one save is not allowed by EF.
        db.MaintenanceWorkOrderAssets.RemoveRange(
            detail.Assets.Where(a => request.Assets.All(r => r.AssetId != a.AssetId)).ToList());
        // A new first due day cannot be in the past (one day of slack for the time zone); an unchanged one of a running
        // plan can, so the plan stays editable.
        var earliest = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime).AddDays(-1);
        foreach (var r in request.Assets)
        {
            var row = detail.Assets.SingleOrDefault(a => a.AssetId == r.AssetId);
            if (r.StartDate < earliest && row?.StartTime != r.StartDate)
            {
                throw MaintenanceErrors.StartInPast();
            }

            if (row is null)
            {
                row = new MaintenanceWorkOrderAsset { AssetId = r.AssetId };
                detail.Assets.Add(row);
            }

            row.TechnicianId = r.TechnicianId;
            row.StartTime = r.StartDate;
            row.EndTime = r.EndDate;
        }

        if (detail.Id == 0)
        {
            plan.Details.Add(detail);
        }
    }

    private async Task NotifyManagersAsync(long buildingId, string details, CancellationToken cancellationToken)
    {
        var managers = await db.ManagerBuildingAssignments
            .Where(a => a.BuildingId == buildingId && a.Manager.Status == UserStatus.Active)
            .Select(a => a.ManagerId).ToListAsync(cancellationToken);
        notifications.Send(managers, details);
    }

    // ----- mapping ----------------------------------------------------------------------------------------------

    private PlanDetailResponse ToDetail(MaintenanceTrigger p, IReadOnlyList<PlanWorkOrderResponse> workOrders)
    {
        var detail = p.Details.First();
        var building = BuildingOf(p)!;
        var assets = detail.Assets
            .OrderBy(a => a.Asset.Name)
            .Select(a => new PlanAssetResponse(
                a.AssetId, a.Asset.Name, a.Asset.Room.RoomNumber,
                a.Technician is null ? null : new PersonRef(a.Technician.Id, a.Technician.FullName),
                a.StartTime, a.EndTime, a.LastDueDate, MaintenanceSchedule.NextDue(a, p), a.Asset.IsRetired))
            .ToList();
        var next = assets.Where(a => !a.IsRetired).Min(a => a.NextDueDate);

        return new PlanDetailResponse(
            p.Id, $"{detail.Checklist.Name} · {building.Name}", p.Status, p.Frequency, p.FrequencyType, building.Id,
            building.Name, assets.Count, p.Status is TriggerStatus.Ended ? null : next,
            new PersonRef(p.Staff.Id, p.Staff.FullName),
            p.ApprovingManager is null ? null : new PersonRef(p.ApprovingManager.Id, p.ApprovingManager.FullName),
            p.LeadDays, detail.ChecklistId, detail.Checklist.Name, detail.EstimatedHours, detail.PriorityLevel.Code,
            p.RejectReason, p.LastPaused, p.LastTriggered, assets, workOrders, AllowedActions(p));
    }
}
