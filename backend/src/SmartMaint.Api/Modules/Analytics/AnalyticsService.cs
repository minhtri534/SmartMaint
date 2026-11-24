using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Analytics.Dtos;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Maintenance.Dtos;

namespace SmartMaint.Api.Modules.Analytics;

/// <summary>
/// F6 numbers (ANA-01..04, MGR-01, OFF-01), one definition for every screen. Computed on request from the cases of
/// the caller's buildings: the data of a building team is small. Move to SQL views if it grows.
/// Not counted in the KPIs (N5): duplicates (merged into another case), surveys (F5 way 2); rejected and cancelled cases
/// are only counted. Preventive work orders (F2) have their own block.
/// </summary>
internal sealed partial class AnalyticsService(AppDbContext db, ICurrentUser currentUser, IBuildingScope scope, TimeProvider clock)
    : IAnalyticsService
{
    private static readonly TimeSpan Vietnam = TimeSpan.FromHours(7);
    private const int FewCases = 5;

    private static readonly CaseStatus[] Open =
    [
        CaseStatus.Approved, CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold,
        CaseStatus.Reopened,
    ];

    private static readonly CaseStatus[] Final = [CaseStatus.Closed, CaseStatus.Rejected, CaseStatus.Cancelled];

    // ----- ANA-01 ---------------------------------------------------------------------------------------------

    public async Task<OverviewResponse> OverviewAsync(AnalyticsQuery query, CancellationToken cancellationToken)
    {
        var (period, from, to) = Resolve(query);
        var cases = await LoadAsync(query.BuildingId, cancellationToken);
        var now = clock.GetUtcNow();
        var incidents = cases.Where(IsIncident).ToList();

        var emergencies = cases.Where(c => c.EmergencyAt >= from && c.EmergencyAt < to && c.TriggerWorkOrderId == null).ToList();
        var counted = emergencies.Where(c => c.MergedIntoCaseId == null && !c.IsSurvey).ToList();
        var emergency = new EmergencyStats(
            counted.Count,
            Average(counted.Where(c => c.AcknowledgedAt != null).Select(c => (c.AcknowledgedAt!.Value - c.EmergencyAt!.Value).TotalMinutes)),
            Average(counted.Where(c => c.CompletedAt != null).Select(c => (c.CompletedAt!.Value - c.EmergencyAt!.Value).TotalHours)),
            cases.Sum(c => c.History.Count(h => In(h.ChangedAt, from, to) && Starts(h.Reason, CaseHistoryTags.NotAnEmergency))),
            emergencies.Count(c => c.MergedIntoCaseId != null));

        var due = cases.Where(c => c.TriggerWorkOrderId != null && c.DueAt >= from && c.DueAt < to && c.DueAt < now
                                   && c.Status != CaseStatus.Cancelled).ToList();
        var inspected = due.Where(c => c.CompletedAt != null).ToList();
        var preventive = new PreventiveStats(
            due.Count,
            due.Count(c => c.CompletedAt <= c.DueAt),
            Rate(due.Count(c => c.CompletedAt <= c.DueAt), due.Count),
            inspected.Count,
            Rate(inspected.Count(c => c.Tasks.All(t => t.Status != CaseTaskStatus.Fail)), inspected.Count));

        var outside = cases.SelectMany(c => c.ThirdParties)
            .Where(t => t.FinishedAt != null && In(t.FinishedAt.Value, from, to) && t.Status == OutsideWorkStatus.Done)
            .Select(t => (t.FinishedAt!.Value - t.CreatedAt).TotalHours);

        var buildings = cases
            .GroupBy(c => (c.Room.Floor.BuildingId, c.Room.Floor.Building.Name))
            .Select(g =>
            {
                var list = g.Where(IsIncident).ToList();
                // Open now = every piece of work not finished yet (pending → waiting for verification), merged
                // reports aside; overdue = still to be done (not reported done) and past its deadline.
                var openNow = g.Where(c => c.MergedIntoCaseId == null && !Final.Contains(c.Status)).ToList();
                return new BuildingRow(
                    g.Key.BuildingId, g.Key.Name,
                    list.Count(c => In(c.SubmittedAt, from, to)),
                    list.Count(c => In(c.CompletedAt, from, to)),
                    OnTime(list, from, to, now, c => c.DueAt),
                    openNow.Count,
                    openNow.Count(c => c.Status != CaseStatus.Completed && c.DueAt < now));
            })
            .OrderBy(b => b.Name)
            .ToList();

        return new OverviewResponse(
            period,
            Kpis(incidents, from, to, now),
            emergency,
            preventive,
            HoldTime(cases, from, to, now),
            Average(outside),
            Weekly(incidents, period),
            buildings);
    }

    private KpiSet Kpis(List<Case> incidents, DateTimeOffset from, DateTimeOffset to, DateTimeOffset now)
    {
        var reported = incidents.Where(c => In(c.SubmittedAt, from, to)).ToList();
        var done = incidents.Where(c => In(c.CompletedAt, from, to)).ToList();
        var moves = incidents.SelectMany(c => c.History.Where(h => In(h.ChangedAt, from, to) && Starts(h.Reason, CaseHistoryTags.DeadlineMoved))
            .Select(_ => c.Id)).ToList();
        var rated = incidents.Where(c => c.RatedAt != null && In(c.RatedAt.Value, from, to)).ToList();

        return new KpiSet(
            reported.Count,
            reported.Count(c => c.Status == CaseStatus.Rejected),
            reported.Count(c => c.Status == CaseStatus.Cancelled),
            done.Count,
            Average(reported.Select(FirstResponseMinutes).Where(m => m != null).Select(m => m!.Value)),
            Average(done.Where(c => WorkStart(c) != null).Select(c => (c.CompletedAt!.Value - WorkStart(c)!.Value).TotalHours)),
            OnTime(incidents, from, to, now, c => c.DueAt),
            OnTime(incidents, from, to, now, OriginalDue),
            moves.Distinct().Count(),
            moves.Count,
            Rate(done.Count(c => c.History.Any(h => h.NewStatus == CaseStatus.Reopened)), done.Count),
            currentUser.Role == UserRole.Manager ? Average(rated.Select(c => (double)c.RatingStars!.Value)) : null,
            currentUser.Role == UserRole.Manager ? rated.Count : 0,
            incidents.Sum(c => c.Rejections.Count(r => In(r.RejectedAt, from, to))));
    }

    /// <summary>Where the resolution time starts: the start of work, or when the case became an emergency (the SLA clock).</summary>
    private static DateTimeOffset? WorkStart(Case c) => c.EmergencyAt ?? c.StartedAt;

    /// <summary>F3: report → the staff's first decision (approve / reject). F4: the emergency clock → acknowledged.</summary>
    private static double? FirstResponseMinutes(Case c)
    {
        if (c.EmergencyAt is { } since)
        {
            return c.AcknowledgedAt is { } ack ? (ack - since).TotalMinutes : null;
        }

        var history = c.History.OrderBy(h => h.ChangedAt).ThenBy(h => h.Id).ToList();
        if (history.FirstOrDefault()?.NewStatus != CaseStatus.Pending)
        {
            return null;
        }

        var decision = history.Skip(1).FirstOrDefault(h => h.NewStatus is CaseStatus.Approved or CaseStatus.Rejected);
        return decision is null ? null : (decision.ChangedAt - c.SubmittedAt).TotalMinutes;
    }

    /// <summary>
    /// Share of deadlines met (17/11, Leader: "100% on time" next to 3 cases past the deadline was wrong). Counted:
    /// the cases reported done in the period (met when done by the deadline) and the cases still not done whose
    /// deadline already passed inside the period (missed, even though the work goes on). Null when none counts.
    /// </summary>
    private static double? OnTime(
        IEnumerable<Case> cases, DateTimeOffset from, DateTimeOffset to, DateTimeOffset now, Func<Case, DateTimeOffset?> dueOf)
    {
        int met = 0, counted = 0;
        foreach (var c in cases)
        {
            if (dueOf(c) is not { } due)
            {
                continue;
            }

            if (In(c.CompletedAt, from, to))
            {
                counted++;
                met += c.CompletedAt <= due ? 1 : 0;
            }
            else if (c.CompletedAt == null && !Final.Contains(c.Status) && due < now && In(due, from, to))
            {
                counted++;
            }
        }

        return Rate(met, counted);
    }

    /// <summary>The first SLA deadline, before any move: start of the clock + the SLA hours of the urgency.</summary>
    private static DateTimeOffset? OriginalDue(Case c) =>
        (c.EmergencyAt ?? c.StartedAt) is { } start ? start.AddHours(c.Priority.SlaHours) : null;

    private static List<HoldTimeRow> HoldTime(List<Case> cases, DateTimeOffset from, DateTimeOffset to, DateTimeOffset now)
    {
        var holds = new List<(string Reason, double Hours)>();
        foreach (var c in cases)
        {
            var history = c.History.OrderBy(h => h.ChangedAt).ThenBy(h => h.Id).ToList();
            for (var i = 0; i < history.Count; i++)
            {
                var h = history[i];
                var entering = h.NewStatus == CaseStatus.OnHold && (i == 0 || history[i - 1].NewStatus != CaseStatus.OnHold);
                if (!entering || !In(h.ChangedAt, from, to))
                {
                    continue;
                }

                var end = history.Skip(i + 1).FirstOrDefault(x => x.NewStatus != CaseStatus.OnHold)?.ChangedAt ?? now;
                holds.Add((HoldGroup(h.Reason), (end - h.ChangedAt).TotalHours));
            }
        }

        return holds.GroupBy(x => x.Reason)
            .Select(g => new HoldTimeRow(g.Key, g.Count(), Math.Round(g.Sum(x => x.Hours), 1), Math.Round(g.Average(x => x.Hours), 1)))
            .OrderByDescending(r => r.TotalHours)
            .ToList();
    }

    private static string HoldGroup(string? reason) =>
        Starts(reason, CaseHistoryTags.WaitingParts) ? "Waiting for parts"
        : Starts(reason, CaseHistoryTags.WaitingCustomer) ? "Waiting for the customer"
        : Starts(reason, CaseHistoryTags.NeedsDecision) ? "Staff decision / outside party"
        : "Other";

    private static List<WeekPoint> Weekly(List<Case> incidents, PeriodResponse period)
    {
        var weeks = new List<WeekPoint>();
        // Weeks start on Monday (Vietnamese calendar habit).
        var start = period.From.AddDays(-(((int)period.From.DayOfWeek + 6) % 7));
        for (var week = start; week <= period.To; week = week.AddDays(7))
        {
            var from = Start(week);
            var to = Start(week.AddDays(7));
            weeks.Add(new WeekPoint(
                week,
                incidents.Count(c => In(c.SubmittedAt, from, to)),
                incidents.Count(c => In(c.CompletedAt, from, to))));
        }

        return weeks;
    }

    // ----- ANA-02 ---------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<TechnicianRow>> TechniciansAsync(AnalyticsQuery query, CancellationToken cancellationToken)
    {
        var (_, from, to) = Resolve(query);
        var cases = await LoadAsync(query.BuildingId, cancellationToken);
        var now = clock.GetUtcNow();
        var work = cases.Where(c => !c.IsSurvey && c.MergedIntoCaseId == null).ToList();
        var technicians = await db.Technicians.AsNoTracking()
            .Where(t => t.Status == UserStatus.Active)
            .Select(t => new { t.Id, t.FullName })
            .ToListAsync(cancellationToken);

        return technicians
            .Select(t =>
            {
                var done = work.Where(c => c.AssignedTechnicianId == t.Id && In(c.CompletedAt, from, to)).ToList();
                var declines = work.SelectMany(c => c.Rejections)
                    .Where(r => r.TechnicianId == t.Id && In(r.RejectedAt, from, to))
                    .OrderByDescending(r => r.RejectedAt).ToList();
                var rated = done.Where(c => c.RatingStars != null).ToList();
                return new TechnicianRow(
                    new PersonRef(t.Id, t.FullName),
                    done.Count,
                    Average(done.Where(c => c.ActualCompletionHours != null).Select(c => (double)c.ActualCompletionHours!.Value)),
                    OnTime(work.Where(c => c.AssignedTechnicianId == t.Id), from, to, now, c => c.DueAt),
                    Rate(done.Count(c => c.History.Any(h => h.NewStatus == CaseStatus.Reopened)), done.Count),
                    Average(rated.Select(c => (double)c.RatingStars!.Value)),
                    done.Sum(c => c.ActualCompletionHours ?? 0),
                    work.Count(c => c.AssignedTechnicianId == t.Id && Open.Contains(c.Status) && c.Status != CaseStatus.Approved),
                    declines.Count,
                    declines.Take(3).Select(r => r.Reason).ToList(),
                    done.Count < FewCases);
            })
            .Where(r => r.Completed > 0 || r.OpenNow > 0 || r.Declines > 0)
            .OrderByDescending(r => r.Completed)
            .ToList();
    }

    // ----- ANA-03 ---------------------------------------------------------------------------------------------

    public async Task<FailuresResponse> FailuresAsync(AnalyticsQuery query, CancellationToken cancellationToken)
    {
        var (period, from, to) = Resolve(query);
        var cases = await LoadAsync(query.BuildingId, cancellationToken);
        var reported = cases.Where(c => IsIncident(c) && In(c.SubmittedAt, from, to)
                                        && c.Status is not (CaseStatus.Rejected or CaseStatus.Cancelled)).ToList();
        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(now.ToOffset(Vietnam).DateTime);

        // AsNoTracking gives each case its own copy of the category / asset: group by id, not by object.
        var byType = reported.GroupBy(c => c.CategoryId)
            .Select(g => new ServiceTypeRow(
                g.Key, CaseMapper.ServiceTypeName(g.First().Category), g.Count(),
                Average(g.Where(c => c.CompletedAt != null && WorkStart(c) != null)
                    .Select(c => (c.CompletedAt!.Value - WorkStart(c)!.Value).TotalHours))))
            .OrderByDescending(r => r.Cases).ToList();
        var byGroup = reported.GroupBy(c => c.Category.GroupName)
            .Select(g => new GroupRow(g.Key, g.Count())).OrderByDescending(r => r.Cases).ToList();
        var byAssetType = reported.GroupBy(c => c.Asset?.Type.Name ?? "No asset")
            .Select(g => new GroupRow(g.Key, g.Count())).OrderByDescending(r => r.Cases).ToList();

        var last30 = now.AddDays(-30);
        var assets = reported.Where(c => c.Asset != null).GroupBy(c => c.AssetId!.Value)
            .Select(g =>
            {
                var a = g.First().Asset!;
                return new ProblemAssetRow(
                    a.Id, a.Name, $"{g.First().Room.Floor.Building.Name} · {g.First().Room.RoomNumber}", a.Type.Name,
                    g.Count(),
                    cases.Count(c => IsIncident(c) && c.AssetId == a.Id && c.SubmittedAt >= last30
                                     && c.Status is not (CaseStatus.Rejected or CaseStatus.Cancelled)),
                    Math.Round((decimal)(now - a.CreatedAt).TotalDays / 365.25m, 1),
                    a.Type.ExpectedLifeYears,
                    a.WarrantyEndDate >= today);
            })
            .OrderByDescending(r => r.Repairs).ThenByDescending(r => r.RepairsLast30Days)
            .Take(10)
            .ToList();

        return new FailuresResponse(period, byType, byGroup, byAssetType, assets);
    }

    // ----- "Needs attention" (F6 step ④) -----------------------------------------------------------------------

    public async Task<IReadOnlyList<AttentionItem>> AttentionAsync(CancellationToken cancellationToken)
    {
        var cases = await LoadAsync(null, cancellationToken);
        var now = clock.GetUtcNow();
        var items = new List<AttentionItem>();

        foreach (var c in cases.Where(c => Open.Contains(c.Status) && !c.IsSurvey && c.MergedIntoCaseId == null))
        {
            var code = Case.CodeOf(c.Id);
            if (c.TriggerWorkOrderId is not null && c.DueAt < now)
            {
                items.Add(new AttentionItem(AttentionKind.PreventiveOverdue,
                    $"Preventive work order {code} \"{c.Title}\" is past its due day.",
                    c.Id, code, c.AssetId, c.TriggerWorkOrder?.TriggerId, null, c.DueAt));
            }
            else if (c.DueAt is { } due && due < now)
            {
                var late = now - due;
                items.Add(new AttentionItem(AttentionKind.OverdueCase,
                    $"{code} \"{c.Title}\" is overdue by {(late.TotalHours < 1 ? $"{(int)late.TotalMinutes} min" : $"{(int)late.TotalHours} h")}.",
                    c.Id, code, c.AssetId, null, null, due));
            }

            if (c.Status == CaseStatus.Approved && c.AssignedTechnicianId == null && c.Rejections.Count > 0)
            {
                var declined = c.Rejections.Max(r => r.RejectedAt);
                items.Add(new AttentionItem(AttentionKind.NoTechnician,
                    $"{code} \"{c.Title}\" has no technician since it was declined.", c.Id, code, c.AssetId, null, null, declined));
            }

            if (c.EmergencyAt is { } emergency && c.AcknowledgedAt == null)
            {
                items.Add(new AttentionItem(AttentionKind.EmergencyNotAcknowledged,
                    $"Emergency {code} \"{c.Title}\" is not acknowledged yet.", c.Id, code, c.AssetId, null, null, emergency));
            }

            if (c.Status == CaseStatus.OnHold && HoldSince(c) is { } since && since < now.AddDays(-3))
            {
                items.Add(new AttentionItem(AttentionKind.LongHold,
                    $"{code} \"{c.Title}\" has been on hold for {(int)(now - since).TotalDays} days.", c.Id, code, c.AssetId, null, null, since));
            }

            var moves = c.History.Count(h => Starts(h.Reason, CaseHistoryTags.DeadlineMoved));
            if (moves >= 2)
            {
                items.Add(new AttentionItem(AttentionKind.ExtendedTwice,
                    $"The deadline of {code} \"{c.Title}\" moved {moves} times.", c.Id, code, c.AssetId, null, null,
                    c.History.Where(h => Starts(h.Reason, CaseHistoryTags.DeadlineMoved)).Max(h => h.ChangedAt)));
            }
        }

        foreach (var c in cases.Where(c => c.Status == CaseStatus.Closed && c.NotFixedAt != null))
        {
            var code = Case.CodeOf(c.Id);
            items.Add(new AttentionItem(AttentionKind.NotFixedReport,
                $"The customer says {code} \"{c.Title}\" is still not fixed.", c.Id, code, c.AssetId, null, null, c.NotFixedAt));
        }

        var month = now.AddDays(-30);
        foreach (var g in cases.Where(c => IsIncident(c) && c.Asset != null && c.SubmittedAt >= month
                                           && c.Status is not (CaseStatus.Rejected or CaseStatus.Cancelled))
                     .GroupBy(c => c.AssetId!.Value).Where(g => g.Count() >= AssetRepairStats.FrequentFailureThreshold))
        {
            items.Add(new AttentionItem(AttentionKind.FrequentFailure,
                $"\"{g.First().Asset!.Name}\" was repaired {g.Count()} times in 30 days.", null, null, g.Key, null, null,
                g.Max(c => c.SubmittedAt)));
        }

        var week = now.AddDays(-7);
        foreach (var g in cases.SelectMany(c => c.Rejections).Where(r => r.RejectedAt >= week)
                     .GroupBy(r => r.TechnicianId).Where(g => g.Count() >= 3))
        {
            items.Add(new AttentionItem(AttentionKind.TechnicianDeclines,
                $"{g.First().Technician.FullName} declined {g.Count()} jobs in 7 days.", null, null, null, null, g.Key,
                g.Max(r => r.RejectedAt)));
        }

        // Meeting note 18/11: the newest problem first, so the home's first lines are the latest.
        return items.OrderByDescending(i => i.Since ?? DateTimeOffset.MinValue).ThenBy(i => i.Kind).ToList();
    }

    /// <summary>When the current hold started: the first On Hold row after the last row with another status.</summary>
    private static DateTimeOffset? HoldSince(Case c)
    {
        var history = c.History.OrderBy(h => h.ChangedAt).ThenBy(h => h.Id).ToList();
        var lastOther = history.FindLastIndex(h => h.NewStatus != CaseStatus.OnHold);
        return history.Skip(lastOther + 1).FirstOrDefault(h => h.NewStatus == CaseStatus.OnHold)?.ChangedAt;
    }

    // ----- helpers ----------------------------------------------------------------------------------------------

    /// <summary>The period in local dates and its UTC bounds [from, to).</summary>
    private (PeriodResponse Period, DateTimeOffset From, DateTimeOffset To) Resolve(AnalyticsQuery query)
    {
        var today = DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(Vietnam).DateTime);
        var to = query.To ?? today;
        var from = query.From ?? to.AddMonths(-1);
        return (new PeriodResponse(from, to), Start(from), Start(to.AddDays(1)));
    }

    private static DateTimeOffset Start(DateOnly day) =>
        new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), Vietnam).ToUniversalTime();

    /// <summary>The cases of the caller's buildings (one building when asked and allowed), with what F6 reads.</summary>
    private async Task<List<Case>> LoadAsync(long? buildingId, CancellationToken cancellationToken)
    {
        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken) ?? [];
        if (buildingId is { } only)
        {
            buildingIds = buildingIds.Contains(only) ? [only] : throw AnalyticsErrors.BuildingNotAllowed();
        }

        return await db.Cases.AsNoTracking()
            .Where(c => buildingIds.Contains(c.Room.Floor.BuildingId))
            .Include(c => c.Room).ThenInclude(r => r.Floor).ThenInclude(f => f.Building)
            .Include(c => c.Priority)
            .Include(c => c.Category)
            .Include(c => c.Technician)
            .Include(c => c.Asset).ThenInclude(a => a!.Type)
            .Include(c => c.TriggerWorkOrder)
            .Include(c => c.History).ThenInclude(h => h.ChangedBy)
            .Include(c => c.Costs)
            .Include(c => c.Rejections).ThenInclude(r => r.Technician)
            .Include(c => c.ThirdParties)
            .Include(c => c.Tasks)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    private static bool IsIncident(Case c) => c.TriggerWorkOrderId == null && !c.IsSurvey && c.MergedIntoCaseId == null;

    private static bool In(DateTimeOffset? at, DateTimeOffset from, DateTimeOffset to) => at >= from && at < to;

    private static bool Starts(string? reason, string tag) => reason?.StartsWith(tag, StringComparison.Ordinal) == true;

    private static double? Average(IEnumerable<double> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? null : Math.Round(list.Average(), 1);
    }

    private static double? Rate(int part, int whole) => whole == 0 ? null : Math.Round((double)part / whole, 3);

    private static DateTime? Local(DateTimeOffset? at) => at?.ToOffset(Vietnam).DateTime;
}
