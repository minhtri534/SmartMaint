using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Notifications;

namespace SmartMaint.Api.Modules.Maintenance;

/// <summary>
/// F2 steps ⑦–⑧: for every running plan, make the work orders whose due day is within the lead time — one case per asset,
/// no customer, the checklist copied as CaseTasks, Assigned to the default technician or Approved for the staff to
/// assign (BR-06: no Pending). G3: when the asset has an open case or was repaired in the last 10% of the period, the
/// staff are told so they can move or cancel the work order. Retired assets are skipped.
/// </summary>
public interface IMaintenanceGenerator
{
    /// <summary>Returns how many work orders were made.</summary>
    Task<int> RunOnceAsync(CancellationToken cancellationToken);
}

internal sealed class MaintenanceGenerator(AppDbContext db, INotificationService notifications, TimeProvider clock)
    : IMaintenanceGenerator
{
    private static readonly CaseStatus[] Finished = [CaseStatus.Closed, CaseStatus.Rejected, CaseStatus.Cancelled];

    /// <summary>Due days are in Vietnam time: a work order is due at the end of its day (23:59 UTC+7).</summary>
    private static readonly TimeSpan Vietnam = TimeSpan.FromHours(7);

    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(now.ToOffset(Vietnam).DateTime);
        var plans = await db.MaintenanceTriggers
            .Where(p => p.Status == TriggerStatus.Active)
            .Include(p => p.Staff)
            .Include(p => p.Details).ThenInclude(d => d.Checklist).ThenInclude(c => c.Tasks)
            .Include(p => p.Details).ThenInclude(d => d.Assets).ThenInclude(a => a.Asset).ThenInclude(a => a.Room).ThenInclude(r => r.Floor)
            .Include(p => p.Details).ThenInclude(d => d.Assets).ThenInclude(a => a.Technician)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var made = 0;
        foreach (var plan in plans)
        {
            foreach (var detail in plan.Details)
            {
                foreach (var entry in detail.Assets)
                {
                    if (MaintenanceSchedule.DueToGenerate(entry, plan, today) is not { } due)
                    {
                        continue;
                    }

                    entry.LastDueDate = due;
                    if (entry.Asset.IsRetired)
                    {
                        continue;
                    }

                    var workOrder = NewWorkOrder(plan, detail, entry, due, now);
                    db.Cases.Add(workOrder);
                    await db.SaveChangesAsync(cancellationToken);
                    made++;

                    var code = Case.CodeOf(workOrder.Id);
                    if (workOrder.AssignedTechnicianId is { } technicianId)
                    {
                        notifications.Send([technicianId], $"{code} preventive maintenance of {entry.Asset.Name} is due {due:yyyy-MM-dd}.");
                    }

                    var staff = await StaffOfBuildingAsync(entry.Asset.Room.Floor.BuildingId, cancellationToken);
                    var warning = await RecentRepairAsync(entry.Asset.Id, plan, now, cancellationToken);
                    notifications.Send(staff, warning is null
                        ? $"{code} preventive maintenance of {entry.Asset.Name} was made (due {due:yyyy-MM-dd})."
                        : $"{code} preventive maintenance of {entry.Asset.Name} is due {due:yyyy-MM-dd}, but {warning}. Keep it, move the deadline or cancel it.");
                }
            }

            plan.LastTriggered = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        return made;
    }

    private static Case NewWorkOrder(
        MaintenanceTrigger plan, MaintenanceWorkOrderDetail detail, MaintenanceWorkOrderAsset entry, DateOnly due, DateTimeOffset now)
    {
        var dueAt = new DateTimeOffset(due.ToDateTime(new TimeOnly(23, 59)), Vietnam).ToUniversalTime();
        var c = new Case
        {
            TriggerWorkOrderId = detail.Id,
            RoomId = entry.Asset.RoomId,
            AssetId = entry.AssetId,
            CategoryId = detail.Checklist.CategoryId,
            Title = $"Preventive: {detail.Checklist.Name}",
            Description = $"Scheduled maintenance of {entry.Asset.Name}, due {due:yyyy-MM-dd} (plan #{plan.Id}). Go through every checklist item.",
            Status = entry.TechnicianId is null ? CaseStatus.Approved : CaseStatus.Assigned,
            PriorityLevelId = detail.PriorityLevelId,
            CostBearer = CostBearer.Building,
            StaffId = plan.StaffId,
            AssignedTechnicianId = entry.TechnicianId,
            SubmittedAt = now,
            DueAt = dueAt,
            EstimatedCompletionHours = detail.EstimatedHours,
            Tasks = detail.Checklist.Tasks
                .OrderBy(t => t.Id)
                .Select(t => new CaseTask { Name = t.Name, Description = t.Description, Status = CaseTaskStatus.Pending })
                .ToList(),
        };

        // The job has no account: the history rows are written in the name of the plan's staff member.
        c.History.Add(new CaseStatusHistory
        {
            NewStatus = CaseStatus.Approved,
            ChangedById = plan.StaffId,
            ChangedAt = now,
            Reason = $"Made by maintenance plan #{plan.Id} (due {due:yyyy-MM-dd}).",
        });
        if (entry.Technician is { } technician)
        {
            c.History.Add(new CaseStatusHistory
            {
                NewStatus = CaseStatus.Assigned,
                ChangedById = plan.StaffId,
                ChangedAt = now,
                Reason = $"Assigned to {technician.FullName} (default technician of the plan).",
            });
        }

        return c;
    }

    /// <summary>G3: an open case on the asset, or one closed within 10% of the period (P04). Null when none.</summary>
    private async Task<string?> RecentRepairAsync(long assetId, MaintenanceTrigger plan, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var since = now.AddDays(-Math.Max(1, MaintenanceSchedule.PeriodDays(plan) / 10));
        var last = await db.Cases.AsNoTracking()
            .Where(c => c.AssetId == assetId && c.TriggerWorkOrderId == null
                        && (!Finished.Contains(c.Status) || (c.ClosedAt != null && c.ClosedAt >= since)))
            .OrderByDescending(c => c.SubmittedAt)
            .Select(c => new { c.Id, c.Status })
            .FirstOrDefaultAsync(cancellationToken);
        return last is null
            ? null
            : last.Status == CaseStatus.Closed
                ? $"the asset was repaired recently ({Case.CodeOf(last.Id)})"
                : $"the asset has an open case ({Case.CodeOf(last.Id)})";
    }

    private async Task<List<long>> StaffOfBuildingAsync(long buildingId, CancellationToken cancellationToken) =>
        await db.StaffBuildingAssignments
            .Where(a => a.BuildingId == buildingId && a.Staff.Status == UserStatus.Active)
            .Select(a => a.StaffId)
            .ToListAsync(cancellationToken);
}

/// <summary>Runs <see cref="IMaintenanceGenerator"/> every Jobs:MaintenanceGeneratorMinutes (default hourly, safe to repeat).</summary>
internal sealed class MaintenanceGeneratorJob(
    IServiceScopeFactory scopes, IOptions<JobOptions> options, ILogger<MaintenanceGeneratorJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = options.Value.MaintenanceGeneratorMinutes;
        if (minutes <= 0)
        {
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var count = await scope.ServiceProvider.GetRequiredService<IMaintenanceGenerator>().RunOnceAsync(stoppingToken);
                if (count > 0)
                {
                    logger.LogInformation("Preventive maintenance: {Count} work order(s) made", count);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "Preventive maintenance run failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
