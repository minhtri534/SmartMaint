using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Notifications;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// F4 background clocks (pages 1–2, Leader 02/11), once each per case, 24/7 (no duty roster):
/// nobody acknowledged after 10 minutes → the backup staff of the building; after 30 minutes → its managers;
/// a technician has not accepted 10 minutes after being assigned → remind them and the staff in charge.
/// The deadline itself (near / overdue) is <see cref="SlaReminder"/>. Run by <see cref="EmergencyWatchJob"/>.
/// </summary>
public interface IEmergencyWatch
{
    /// <summary>Sends the alerts that are due now. Returns how many alerts went out.</summary>
    Task<int> RunOnceAsync(CancellationToken cancellationToken);
}

internal sealed class EmergencyWatch(AppDbContext db, INotificationService notifications, TimeProvider clock) : IEmergencyWatch
{
    public static readonly TimeSpan BackupAfter = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan ManagerAfter = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan AcceptReminderAfter = TimeSpan.FromMinutes(10);

    private static readonly CaseStatus[] Open =
    [
        CaseStatus.Approved, CaseStatus.Assigned, CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold,
        CaseStatus.Reopened,
    ];

    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var cases = await db.Cases
            .Include(c => c.Room).ThenInclude(r => r.Floor)
            .Include(c => c.Technician)
            .Where(c => c.EmergencyAt != null && c.Priority.Code == CasePriority.Emergency && Open.Contains(c.Status)
                        && ((c.AcknowledgedAt == null && (c.BackupAlertedAt == null || c.ManagerAlertedAt == null))
                            || ((c.Status == CaseStatus.Assigned || c.Status == CaseStatus.Reopened) && c.AcceptRemindedAt == null)))
            .ToListAsync(cancellationToken);

        var sent = 0;
        foreach (var c in cases)
        {
            var code = Case.CodeOf(c.Id);
            var where = $"{c.Room.Floor.DisplayName}, {c.Room.RoomNumber}";
            var waited = now - c.EmergencyAt!.Value;
            var buildingId = c.Room.Floor.BuildingId;

            if (c.AcknowledgedAt is null && c.BackupAlertedAt is null && waited >= BackupAfter)
            {
                var primary = await CaseService.PrimaryStaffAsync(db, buildingId, primary: true, cancellationToken);
                var backup = await CaseService.PrimaryStaffAsync(db, buildingId, primary: false, cancellationToken);
                notifications.Send([.. primary, .. backup],
                    $"EMERGENCY {code} \"{c.Title}\" in {where} has not been acknowledged for {(int)waited.TotalMinutes} min.");
                c.BackupAlertedAt = now;
                sent++;
            }

            if (c.AcknowledgedAt is null && c.ManagerAlertedAt is null && waited >= ManagerAfter)
            {
                var managers = await db.ManagerBuildingAssignments
                    .Where(a => a.BuildingId == buildingId && a.Manager.Status == UserStatus.Active)
                    .Select(a => a.ManagerId).ToListAsync(cancellationToken);
                notifications.Send(managers,
                    $"EMERGENCY {code} \"{c.Title}\" in {where}: nobody has acknowledged it for {(int)waited.TotalMinutes} min.");
                c.ManagerAlertedAt = now;
                c.BackupAlertedAt ??= now;
                sent++;
            }

            if (c.Status is CaseStatus.Assigned or CaseStatus.Reopened && c.AcceptRemindedAt is null && c.AssignedTechnicianId is { } technicianId)
            {
                var assignedAt = await db.CaseStatusHistory
                    .Where(h => h.CaseId == c.Id && h.NewStatus == c.Status)
                    .OrderByDescending(h => h.ChangedAt).Select(h => (DateTimeOffset?)h.ChangedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                if (assignedAt is { } since && now - since >= AcceptReminderAfter)
                {
                    long[] staff = c.StaffId is { } s ? [s] : [];
                    notifications.Send([technicianId], $"EMERGENCY {code} \"{c.Title}\" in {where} still waits for you to accept it.");
                    notifications.Send(staff, $"{c.Technician?.FullName} has not accepted EMERGENCY {code} yet. Call them or reassign.");
                    c.AcceptRemindedAt = now;
                    sent++;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return sent;
    }
}

/// <summary>Runs <see cref="IEmergencyWatch"/> every Jobs:EmergencyWatchMinutes inside the API.</summary>
internal sealed class EmergencyWatchJob(IServiceScopeFactory scopes, IOptions<JobOptions> options, ILogger<EmergencyWatchJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = options.Value.EmergencyWatchMinutes;
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
                var count = await scope.ServiceProvider.GetRequiredService<IEmergencyWatch>().RunOnceAsync(stoppingToken);
                if (count > 0)
                {
                    logger.LogInformation("Emergency alerts sent: {Count}", count);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "Emergency watch run failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
