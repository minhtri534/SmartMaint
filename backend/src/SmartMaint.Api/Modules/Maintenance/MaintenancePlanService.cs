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
/// F2 plans (PM-01..03), shaped like UpKeep since the meeting note of 18/11: a plan is one work-order template (name,
/// checklist, urgency, hours) with one or more triggers, each with its own schedule, end day and assets. The staff of a
/// building makes a plan, a manager of that building approves or rejects it (E10), then it runs until paused or archived.
/// Two ways to stop: a trigger's end day (the plan shows "Expired"; moving the end day later extends it without a new
/// approval), or Archive (for good: no edit, no new work orders). Any other edit sends it back for approval. The building
/// of a plan is the building of its assets (the ERD has no building column on MaintenanceTriggers).
/// </summary>
public interface IMaintenancePlanService
{
    Task<IReadOnlyList<PlanSummaryResponse>> ListAsync(CancellationToken cancellationToken);

    Task<PlanDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>Every work order the plan made, newest due day first (the "Work orders" tab).</summary>
    Task<IReadOnlyList<PlanWorkOrderResponse>> WorkOrdersAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff. The plan waits for a manager's approval.</summary>
    Task<PlanDetailResponse> CreateAsync(PlanRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Staff. Back to "pending approval", unless only end days changed on a running or paused plan: then it keeps going
    /// (extending an expired plan).
    /// </summary>
    Task<PlanDetailResponse> UpdateAsync(long id, PlanRequest request, CancellationToken cancellationToken);

    Task<PlanDetailResponse> ApproveAsync(long id, CancellationToken cancellationToken);
    Task<PlanDetailResponse> RejectAsync(long id, RejectPlanRequest request, CancellationToken cancellationToken);
    Task<PlanDetailResponse> PauseAsync(long id, CancellationToken cancellationToken);

    /// <summary>Due days missed while paused are skipped; the next one counts from today.</summary>
    Task<PlanDetailResponse> ResumeAsync(long id, CancellationToken cancellationToken);

    /// <summary>For good: no edit, no new work orders; the ones already made go on.</summary>
    Task<PlanDetailResponse> ArchiveAsync(long id, CancellationToken cancellationToken);
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
        return plans.OrderBy(p => p.Status).ThenBy(p => p.Id).Select(p => (PlanSummaryResponse)ToDetail(p, 0)).ToList();
    }

    public async Task<PlanDetailResponse> GetAsync(long id, CancellationToken cancellationToken)
    {
        var plan = (await VisibleAsync(asTracking: false, cancellationToken)).SingleOrDefault(p => p.Id == id)
            ?? throw MaintenanceErrors.PlanNotFound();
        var triggerIds = plan.Details.Select(d => d.Id).ToList();
        var count = await db.Cases.CountAsync(
            c => c.TriggerWorkOrderId != null && triggerIds.Contains(c.TriggerWorkOrderId.Value), cancellationToken);
        return ToDetail(plan, count);
    }

    public async Task<IReadOnlyList<PlanWorkOrderResponse>> WorkOrdersAsync(long id, CancellationToken cancellationToken)
    {
        var plan = (await VisibleAsync(asTracking: false, cancellationToken)).SingleOrDefault(p => p.Id == id)
            ?? throw MaintenanceErrors.PlanNotFound();
        var triggerIds = plan.Details.Select(d => d.Id).ToList();
        var rows = await db.Cases.AsNoTracking()
            .Where(c => c.TriggerWorkOrderId != null && triggerIds.Contains(c.TriggerWorkOrderId.Value))
            .OrderByDescending(c => c.DueAt).ThenByDescending(c => c.Id)
            .Select(c => new
            {
                c.Id,
                TriggerId = c.TriggerWorkOrderId!.Value,
                AssetName = c.Asset!.Name,
                TechnicianName = c.Technician == null ? null : c.Technician.FullName,
                c.Status,
                c.DueAt,
                c.ClosedAt,
            })
            .ToListAsync(cancellationToken);
        return rows
            .Select(c => new PlanWorkOrderResponse(
                c.Id, Case.CodeOf(c.Id), c.TriggerId, c.AssetName, c.TechnicianName, c.Status, c.DueAt, c.ClosedAt))
            .ToList();
    }

    // ----- writes -----------------------------------------------------------------------------------------------

    public async Task<PlanDetailResponse> CreateAsync(PlanRequest request, CancellationToken cancellationToken)
    {
        var buildingId = await CheckRequestAsync(request, cancellationToken);
        var plan = new MaintenanceTrigger { StaffId = currentUser.Id, Status = TriggerStatus.PendingApproval };
        Apply(plan, request);
        db.MaintenanceTriggers.Add(plan);
        await NotifyManagersAsync(buildingId, $"Maintenance plan \"{plan.Name}\" waits for your approval.", cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(plan.Id, cancellationToken);
    }

    public async Task<PlanDetailResponse> UpdateAsync(long id, PlanRequest request, CancellationToken cancellationToken)
    {
        var plan = await FindAsync(id, PlanAction.Edit, cancellationToken);
        var buildingId = await CheckRequestAsync(request, cancellationToken);

        if (plan.Status is TriggerStatus.Active or TriggerStatus.Paused && Shape(plan) == Shape(request))
        {
            // Only end days changed: the approved plan keeps running (decision 18/11, "extend without approval").
            ExtendOnly(plan, request);
            await NotifyManagersAsync(buildingId, $"Maintenance plan \"{NameOf(plan)}\": the end days were changed, it keeps running.", cancellationToken);
        }
        else
        {
            await CheckRemovedTriggersAsync(plan, request, cancellationToken);
            Apply(plan, request);
            plan.Status = TriggerStatus.PendingApproval;
            plan.ApprovingManagerId = null;
            plan.RejectReason = null;
            await NotifyManagersAsync(buildingId, $"Maintenance plan \"{plan.Name}\" was changed and waits for your approval again.", cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public Task<PlanDetailResponse> ApproveAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, PlanAction.Approve, plan =>
        {
            plan.Status = TriggerStatus.Active;
            plan.ApprovingManagerId = currentUser.Id;
            plan.RejectReason = null;
            notifications.Send([plan.StaffId], $"Maintenance plan \"{NameOf(plan)}\" was approved and is running.");
        }, cancellationToken);

    public Task<PlanDetailResponse> RejectAsync(long id, RejectPlanRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, PlanAction.Reject, plan =>
        {
            plan.Status = TriggerStatus.Rejected;
            plan.ApprovingManagerId = currentUser.Id;
            plan.RejectReason = request.Reason.Trim();
            notifications.Send([plan.StaffId], $"Maintenance plan \"{NameOf(plan)}\" was rejected: {plan.RejectReason}");
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
            SkipPastDueDays(plan);
        }, cancellationToken);

    public Task<PlanDetailResponse> ArchiveAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, PlanAction.Archive, plan =>
        {
            plan.Status = TriggerStatus.Archived;
            plan.ArchivedAt = clock.GetUtcNow();
        }, cancellationToken);

    private async Task<PlanDetailResponse> ActAsync(
        long id, PlanAction action, Action<MaintenanceTrigger> apply, CancellationToken cancellationToken)
    {
        var plan = await FindAsync(id, action, cancellationToken);
        apply(plan);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    // ----- rules ------------------------------------------------------------------------------------------------

    /// <summary>Who may do what: staff of the building edit / pause / resume / archive; managers of the building approve.</summary>
    private IReadOnlyList<PlanAction> AllowedActions(MaintenanceTrigger plan) => currentUser.Role switch
    {
        UserRole.Staff => plan.Status switch
        {
            TriggerStatus.PendingApproval or TriggerStatus.Rejected => [PlanAction.Edit, PlanAction.Archive],
            TriggerStatus.Active => [PlanAction.Edit, PlanAction.Pause, PlanAction.Archive],
            TriggerStatus.Paused => [PlanAction.Edit, PlanAction.Resume, PlanAction.Archive],
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
            .Include(p => p.Details).ThenInclude(d => d.Checklist).ThenInclude(c => c.Tasks)
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

    /// <summary>The name shown: plans made before 19/11 have none, they read "checklist · building".</summary>
    private static string NameOf(MaintenanceTrigger plan) =>
        plan.Name ?? $"{plan.Details.First().Checklist.Name} · {BuildingOf(plan)?.Name}";

    /// <summary>Checks the request and returns the building of its assets (one building the caller looks after).</summary>
    private async Task<long> CheckRequestAsync(PlanRequest request, CancellationToken cancellationToken)
    {
        if (!await db.CaseChecklists.AnyAsync(c => c.Id == request.ChecklistId, cancellationToken))
        {
            throw MaintenanceErrors.ChecklistNotFound();
        }

        var entries = request.Triggers.SelectMany(t => t.Assets).ToList();
        var assetIds = entries.Select(a => a.AssetId).Distinct().ToList();
        var assets = await db.Assets.AsNoTracking()
            .Where(a => assetIds.Contains(a.Id) && a.RetiredAt == null)
            .Select(a => new { a.Id, a.Room.Floor.BuildingId })
            .ToListAsync(cancellationToken);
        if (assets.Count != assetIds.Count)
        {
            throw MaintenanceErrors.UnknownAsset();
        }

        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken) ?? [];
        var building = assets.Select(a => a.BuildingId).Distinct().ToList();
        if (building.Count != 1 || !buildingIds.Contains(building[0]))
        {
            throw MaintenanceErrors.AssetsInSeveralBuildings();
        }

        var technicianIds = entries.Where(a => a.TechnicianId is not null).Select(a => a.TechnicianId!.Value).Distinct().ToList();
        if (await db.Technicians.CountAsync(t => technicianIds.Contains(t.Id) && t.Status == UserStatus.Active, cancellationToken)
            != technicianIds.Count)
        {
            throw MaintenanceErrors.UnknownTechnician();
        }

        return building[0];
    }

    /// <summary>A trigger that already made work orders is kept (the work orders point at it): end it instead.</summary>
    private async Task CheckRemovedTriggersAsync(MaintenanceTrigger plan, PlanRequest request, CancellationToken cancellationToken)
    {
        var kept = request.Triggers.Where(t => t.Id is not null).Select(t => t.Id!.Value).ToList();
        if (kept.Any(id => plan.Details.All(d => d.Id != id)))
        {
            throw MaintenanceErrors.UnknownTrigger();
        }

        var removed = plan.Details.Where(d => !kept.Contains(d.Id)).Select(d => d.Id).ToList();
        if (removed.Count > 0 && await db.Cases.AnyAsync(
                c => c.TriggerWorkOrderId != null && removed.Contains(c.TriggerWorkOrderId.Value), cancellationToken))
        {
            throw MaintenanceErrors.TriggerHasWorkOrders();
        }
    }

    /// <summary>Everything that needs a manager's approval when it changes: all but the end days.</summary>
    private static string Shape(MaintenanceTrigger plan)
    {
        var first = plan.Details.First();
        return Shape(plan.Name, plan.Description, first.ChecklistId, first.EstimatedHours, first.PriorityLevel.Code,
            plan.Details.Select(d => (d.Id, d.Frequency, d.FrequencyType, d.LeadDays, d.StartDate,
                d.Assets.Select(a => (a.AssetId, a.TechnicianId)))));
    }

    private static string Shape(PlanRequest r) =>
        Shape(r.Name.Trim(), Clean(r.Description), r.ChecklistId, r.EstimatedHours, r.Priority,
            r.Triggers.Select(t => (t.Id ?? 0, t.Frequency, t.FrequencyType, t.LeadDays, t.StartDate,
                t.Assets.Select(a => (a.AssetId, a.TechnicianId)))));

    private static string Shape(
        string? name, string? description, long checklistId, decimal? hours, CasePriority priority,
        IEnumerable<(long Id, int Frequency, FrequencyUnit Unit, int LeadDays, DateOnly Start, IEnumerable<(long AssetId, long? TechnicianId)> Assets)> triggers) =>
        string.Join('|', name, description, checklistId, hours?.ToString("0.##"), priority,
            string.Join(';', triggers.OrderBy(t => t.Id).Select(t =>
                $"{t.Id},{t.Frequency},{t.Unit},{t.LeadDays},{t.Start:yyyy-MM-dd}:" +
                string.Join(',', t.Assets.OrderBy(a => a.AssetId).Select(a => $"{a.AssetId}/{a.TechnicianId}")))));

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>Moves only the end days. Due days that passed while expired are skipped, like after a pause.</summary>
    private void ExtendOnly(MaintenanceTrigger plan, PlanRequest request)
    {
        foreach (var r in request.Triggers)
        {
            var trigger = plan.Details.Single(d => d.Id == r.Id);
            trigger.EndDate = r.EndDate;
            foreach (var asset in trigger.Assets)
            {
                asset.EndTime = r.EndDate;
            }
        }

        SkipPastDueDays(plan);
    }

    private void SkipPastDueDays(MaintenanceTrigger plan)
    {
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        foreach (var trigger in plan.Details)
        {
            foreach (var asset in trigger.Assets)
            {
                // Mark the due days before today as done, so the next work order is a future one.
                while (MaintenanceSchedule.NextDue(asset, trigger) is { } due && due < today)
                {
                    asset.LastDueDate = due;
                }
            }
        }
    }

    private void Apply(MaintenanceTrigger plan, PlanRequest request)
    {
        plan.Name = request.Name.Trim();
        plan.Description = Clean(request.Description);
        // The plan's own schedule columns (ERD) keep the first trigger's values for the code deployed before 19/11.
        plan.Frequency = request.Triggers[0].Frequency;
        plan.FrequencyType = request.Triggers[0].FrequencyType;
        plan.LeadDays = request.Triggers[0].LeadDays;
        var priorityId = db.PriorityLevels.Single(p => p.Code == request.Priority).Id;

        var kept = request.Triggers.Where(t => t.Id is not null).Select(t => t.Id!.Value).ToHashSet();
        db.MaintenanceWorkOrderDetails.RemoveRange(plan.Details.Where(d => d.Id != 0 && !kept.Contains(d.Id)).ToList());

        // A new first due day cannot be in the past (one day of slack for the time zone); an unchanged one of a running
        // plan can, so the plan stays editable.
        var earliest = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime).AddDays(-1);
        foreach (var r in request.Triggers)
        {
            var trigger = r.Id is { } id
                ? plan.Details.SingleOrDefault(d => d.Id == id) ?? throw MaintenanceErrors.UnknownTrigger()
                : null;
            var startMoved = trigger is null || trigger.StartDate != r.StartDate;
            if (startMoved && r.StartDate < earliest)
            {
                throw MaintenanceErrors.StartInPast();
            }

            if (trigger is null)
            {
                trigger = new MaintenanceWorkOrderDetail();
                plan.Details.Add(trigger);
            }

            trigger.ChecklistId = request.ChecklistId;
            trigger.EstimatedHours = request.EstimatedHours;
            trigger.PriorityLevelId = priorityId;
            trigger.Frequency = r.Frequency;
            trigger.FrequencyType = r.FrequencyType;
            trigger.LeadDays = r.LeadDays;
            trigger.StartDate = r.StartDate;
            trigger.EndDate = r.EndDate;
            ApplyAssets(trigger, r, startMoved);
        }
    }

    /// <summary>
    /// Changes only what differs: an asset that stays keeps its schedule position (LastDueDate), so an edit never makes
    /// a work order twice; removing and re-adding the same (trigger, asset) key in one save is not allowed by EF.
    /// </summary>
    private void ApplyAssets(MaintenanceWorkOrderDetail trigger, TriggerRequest r, bool startMoved)
    {
        db.MaintenanceWorkOrderAssets.RemoveRange(
            trigger.Assets.Where(a => r.Assets.All(x => x.AssetId != a.AssetId)).ToList());
        foreach (var x in r.Assets)
        {
            var row = trigger.Assets.SingleOrDefault(a => a.AssetId == x.AssetId);
            if (row is null)
            {
                row = new MaintenanceWorkOrderAsset { AssetId = x.AssetId, StartTime = r.StartDate };
                trigger.Assets.Add(row);
            }
            else if (startMoved)
            {
                row.StartTime = r.StartDate;
                row.LastDueDate = null;
            }

            row.TechnicianId = x.TechnicianId;
            row.EndTime = r.EndDate;
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

    private PlanDetailResponse ToDetail(MaintenanceTrigger p, int workOrderCount)
    {
        var first = p.Details.OrderBy(d => d.Id).First();
        var building = BuildingOf(p)!;
        var triggers = p.Details
            .OrderBy(d => d.Id)
            .Select(d =>
            {
                var assets = d.Assets
                    .OrderBy(a => a.Asset.Name)
                    .Select(a => new PlanAssetResponse(
                        a.AssetId, a.Asset.Name, a.Asset.Room.RoomNumber,
                        a.Technician is null ? null : new PersonRef(a.Technician.Id, a.Technician.FullName),
                        a.StartTime, a.LastDueDate, MaintenanceSchedule.NextDue(a, d), a.Asset.IsRetired))
                    .ToList();
                return new PlanTriggerResponse(
                    d.Id, d.Frequency, d.FrequencyType, d.LeadDays, d.StartDate, d.EndDate,
                    assets.Where(a => !a.IsRetired).Min(a => a.NextDueDate), MaintenanceSchedule.Expired(d), assets);
            })
            .ToList();
        var stopped = p.Status is TriggerStatus.Archived or TriggerStatus.Rejected;
        var expired = p.Status is TriggerStatus.Active or TriggerStatus.Paused && triggers.All(t => t.Expired);

        return new PlanDetailResponse(
            p.Id, NameOf(p), p.Status, expired,
            triggers.Select(t => new PlanEvery(t.Frequency, t.FrequencyType)).Distinct().ToList(),
            building.Id, building.Name, triggers.Sum(t => t.Assets.Count),
            stopped ? null : triggers.Min(t => t.NextDueDate),
            new PersonRef(p.Staff.Id, p.Staff.FullName),
            p.ApprovingManager is null ? null : new PersonRef(p.ApprovingManager.Id, p.ApprovingManager.FullName),
            p.Description, first.ChecklistId, first.Checklist.Name, first.Checklist.Tasks.Count, first.EstimatedHours,
            first.PriorityLevel.Code, p.RejectReason, p.CreatedAt, p.LastPaused, p.LastTriggered, p.ArchivedAt,
            triggers, workOrderCount, AllowedActions(p));
    }
}
