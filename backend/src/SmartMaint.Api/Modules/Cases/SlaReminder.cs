using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Notifications;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// SRS 5.4.2 "Case close to its SLA target, or overdue": once per case, tell the staff of the building when 80% of the
/// time is used, and the staff + managers of the building when the deadline passes. An emergency (F4) counts 24/7 from
/// EmergencyAt, already at Approved; an F3 case from the start of work.
/// Run by <see cref="SlaReminderJob"/> every few minutes; tests call <see cref="RunOnceAsync"/> directly.
/// </summary>
public interface ISlaReminder
{
    /// <summary>Sends the reminders that are due now. Returns how many cases got one.</summary>
    Task<int> RunOnceAsync(CancellationToken cancellationToken);
}

internal sealed class SlaReminder(AppDbContext db, INotificationService notifications, TimeProvider clock) : ISlaReminder
{
    /// <summary>"Close to the deadline" = less than this share of the SLA time is left.</summary>
    public const double WarnWhenLeft = 0.2;

    private static readonly CaseStatus[] Running =
    [
        CaseStatus.Assigned, CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold, CaseStatus.Reopened,
    ];

    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var cases = await db.Cases
            .Include(c => c.Room).ThenInclude(r => r.Floor)
            .Include(c => c.Priority)
            .Where(c => c.DueAt != null && (Running.Contains(c.Status) || (c.Status == CaseStatus.Approved && c.EmergencyAt != null))
                        && (c.SlaWarnedAt == null || c.SlaOverdueNotifiedAt == null))
            .ToListAsync(cancellationToken);

        var reminded = 0;
        foreach (var c in cases)
        {
            var due = c.DueAt!.Value;
            var code = Case.CodeOf(c.Id);
            var staff = await db.StaffBuildingAssignments
                .Where(a => a.BuildingId == c.Room.Floor.BuildingId && a.Staff.Status == UserStatus.Active)
                .Select(a => a.StaffId).ToListAsync(cancellationToken);

            if (now >= due && c.SlaOverdueNotifiedAt is null)
            {
                var managers = await db.ManagerBuildingAssignments
                    .Where(a => a.BuildingId == c.Room.Floor.BuildingId && a.Manager.Status == UserStatus.Active)
                    .Select(a => a.ManagerId).ToListAsync(cancellationToken);
                notifications.Send([.. staff, .. managers], $"{code} \"{c.Title}\" is past its deadline ({due:yyyy-MM-dd HH:mm} UTC).");
                c.SlaOverdueNotifiedAt = now;
                c.SlaWarnedAt ??= now;
                reminded++;
            }
            // "Close to the deadline" needs a start (F3); a preventive work order (due day set, not started) only gets "overdue".
            else if (now < due && c.SlaWarnedAt is null && (c.EmergencyAt ?? c.StartedAt) is { } started && due - now <= (due - started) * WarnWhenLeft)
            {
                notifications.Send(staff, $"{code} \"{c.Title}\" is close to its deadline ({due:yyyy-MM-dd HH:mm} UTC).");
                c.SlaWarnedAt = now;
                reminded++;
            }
        }

        reminded += await RemindQuotesAsync(now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return reminded;
    }

    /// <summary>F5 page 1: the customer's contract representative has not answered a quote for 3 days → tell the staff once.</summary>
    public static readonly TimeSpan QuoteAnswerTime = TimeSpan.FromDays(3);

    private async Task<int> RemindQuotesAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var limit = now - QuoteAnswerTime;
        var waiting = await db.CaseThirdParties
            .Include(t => t.Case).ThenInclude(c => c.Room).ThenInclude(r => r.Floor)
            .Where(t => t.Status == Entities.Cases.OutsideWorkStatus.AwaitingDecision && t.QuoteBearer == CostBearer.Customer
                        && t.QuoteSentAt <= limit && t.QuoteRemindedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var t in waiting)
        {
            long[] staff = t.Case.StaffId is { } s
                ? [s]
                : [.. await db.StaffBuildingAssignments.Where(a => a.BuildingId == t.Case.Room.Floor.BuildingId)
                    .Select(a => a.StaffId).ToListAsync(cancellationToken)];
            notifications.Send(staff,
                $"{Case.CodeOf(t.CaseId)}: {t.QuoteSentTo} has not answered the quote of {t.CompanyName} for 3 days. Call them.");
            t.QuoteRemindedAt = now;
        }

        return waiting.Count;
    }
}

/// <summary>Section "Jobs" of the settings. 0 minutes = the job does not run.</summary>
public sealed class JobOptions
{
    public const string SectionName = "Jobs";

    public int SlaReminderMinutes { get; set; } = 5;

    /// <summary>F2 work-order generation; safe to run often (each due day is made once).</summary>
    public int MaintenanceGeneratorMinutes { get; set; } = 60;

    /// <summary>F4 escalation clocks (10 / 30 minutes): checked every minute.</summary>
    public int EmergencyWatchMinutes { get; set; } = 1;
}

/// <summary>Runs <see cref="ISlaReminder"/> every Jobs:SlaReminderMinutes inside the API (no extra service).</summary>
internal sealed class SlaReminderJob(IServiceScopeFactory scopes, IOptions<JobOptions> options, ILogger<SlaReminderJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = options.Value.SlaReminderMinutes;
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
                var count = await scope.ServiceProvider.GetRequiredService<ISlaReminder>().RunOnceAsync(stoppingToken);
                if (count > 0)
                {
                    logger.LogInformation("SLA reminders sent for {Count} case(s)", count);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                // A failed run (e.g. the database restarting) must not stop the job; the next tick tries again.
                logger.LogError(e, "SLA reminder run failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
