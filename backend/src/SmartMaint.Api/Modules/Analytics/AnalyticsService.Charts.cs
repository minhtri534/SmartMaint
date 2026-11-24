using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Analytics.Dtos;

namespace SmartMaint.Api.Modules.Analytics;

/// <summary>
/// Meeting note 18/11, the Charts tab: one metric per day, week or month of the period, and the same metric over the
/// whole period split by building, technician, staff member or fault category. Same definitions as the overview.
/// </summary>
internal sealed partial class AnalyticsService
{
    /// <summary>The split shows this many names; the rest of a count or a sum is "Other".</summary>
    private const int BreakdownSize = 8;

    public async Task<ChartResponse> ChartAsync(ChartQuery query, CancellationToken cancellationToken)
    {
        var (period, from, to) = Resolve(query);
        var cases = await LoadAsync(query.BuildingId, cancellationToken);
        var now = clock.GetUtcNow();
        var buckets = Buckets(period, query.Step);
        var incidents = cases.Where(IsIncident).ToList();
        var work = cases.Where(c => !c.IsSurvey && c.MergedIntoCaseId == null).ToList();

        static string Building(Case c) => c.Room.Floor.Building.Name;

        var (unit, by, points, parts) = query.Metric switch
        {
            ChartMetric.CasesReported => Count("cases", "Building", incidents, c => c.SubmittedAt, Building),
            ChartMetric.CasesCompleted => Count("cases", "Building", incidents, c => c.CompletedAt, Building),
            ChartMetric.CasesLate => Count("cases", "Building",
                incidents.Where(c => c.DueAt is { } due && due < now && (c.CompletedAt is { } done ? done > due : !Final.Contains(c.Status))).ToList(),
                c => c.DueAt, Building),
            ChartMetric.TechnicianCompleted => Count("cases", "Technician",
                work.Where(c => c.Technician != null).ToList(), c => c.CompletedAt, c => c.Technician!.FullName),
            ChartMetric.TechnicianOnTime => OnTimeChart(work.Where(c => c.Technician != null).ToList(), now),
            ChartMetric.TechnicianLabourHours => Sum("hours", "Technician",
                work.Where(c => c.Technician != null && c.ActualCompletionHours != null).ToList(), c => c.CompletedAt,
                c => c.Technician!.FullName, c => (double)c.ActualCompletionHours!.Value),
            ChartMetric.StaffDecisions => Decisions(incidents, averageMinutes: false),
            ChartMetric.StaffFirstResponse => Decisions(incidents, averageMinutes: true),
            ChartMetric.CostTotal => Sum("VND", "Building", work.Where(c => c.Costs.Count > 0).ToList(), c => c.CompletedAt,
                Building, c => (double)c.Costs.Sum(x => x.Amount)),
            ChartMetric.CostByFault => Sum("VND", "Fault category", work.Where(c => c.Costs.Count > 0).ToList(), c => c.CompletedAt,
                c => c.Category.Name, c => (double)c.Costs.Sum(x => x.Amount)),
            _ => throw new ArgumentOutOfRangeException(nameof(query)),
        };
        return new ChartResponse(period, query.Metric, query.Step, unit, points, by, parts);

        // ---- one shape per kind of metric ----

        (string, string, List<ChartPoint>, List<ChartPart>) Count(
            string u, string splitBy, List<Case> rows, Func<Case, DateTimeOffset?> at, Func<Case, string> name) =>
            Sum(u, splitBy, rows, at, name, _ => 1);

        (string, string, List<ChartPoint>, List<ChartPart>) Sum(
            string u, string splitBy, List<Case> rows, Func<Case, DateTimeOffset?> at, Func<Case, string> name, Func<Case, double> value)
        {
            var line = buckets.Select(b => new ChartPoint(b.Start, Math.Round(rows.Where(c => In(at(c), b.From, b.To)).Sum(value), 1))).ToList();
            var split = rows.Where(c => In(at(c), from, to)).GroupBy(name)
                .Select(g => new ChartPart(g.Key, Math.Round(g.Sum(value), 1)));
            return (u, splitBy, line, Top(split, withOther: true));
        }

        (string, string, List<ChartPoint>, List<ChartPart>) OnTimeChart(List<Case> rows, DateTimeOffset current)
        {
            var line = buckets.Select(b => new ChartPoint(b.Start, Percent(OnTime(rows, b.From, b.To, current, c => c.DueAt)))).ToList();
            var split = rows.GroupBy(c => c.Technician!.FullName)
                .Select(g => (g.Key, Rate: Percent(OnTime(g, from, to, current, c => c.DueAt))))
                .Where(x => x.Rate != null)
                .Select(x => new ChartPart(x.Key, x.Rate!.Value));
            return ("%", "Technician", line, Top(split, withOther: false));
        }

        (string, string, List<ChartPoint>, List<ChartPart>) Decisions(List<Case> rows, bool averageMinutes)
        {
            // The staff's first decision on each request: who, when, and how long after the report.
            var decisions = rows.Select(FirstDecision).Where(d => d != null).Select(d => d!.Value).ToList();
            double? Value(IEnumerable<(string Staff, DateTimeOffset At, double Minutes)> set)
            {
                var list = set.ToList();
                return averageMinutes ? Average(list.Select(d => d.Minutes)) : list.Count;
            }

            var line = buckets.Select(b => new ChartPoint(b.Start, Value(decisions.Where(d => In(d.At, b.From, b.To))))).ToList();
            var split = decisions.Where(d => In(d.At, from, to)).GroupBy(d => d.Staff)
                .Select(g => new ChartPart(g.Key, Value(g) ?? 0));
            return (averageMinutes ? "minutes" : "cases", "Staff member", line, Top(split, withOther: !averageMinutes));
        }
    }

    /// <summary>The first approve / reject of a reported request: who decided, when, and the minutes since the report.</summary>
    private static (string Staff, DateTimeOffset At, double Minutes)? FirstDecision(Case c)
    {
        var history = c.History.OrderBy(h => h.ChangedAt).ThenBy(h => h.Id).ToList();
        if (history.FirstOrDefault()?.NewStatus != CaseStatus.Pending)
        {
            return null;
        }

        var decision = history.Skip(1).FirstOrDefault(h => h.NewStatus is CaseStatus.Approved or CaseStatus.Rejected);
        return decision?.ChangedBy is null
            ? null
            : (decision.ChangedBy.FullName, decision.ChangedAt, (decision.ChangedAt - c.SubmittedAt).TotalMinutes);
    }

    private static double? Percent(double? rate) => rate is { } r ? Math.Round(r * 100, 1) : null;

    /// <summary>Largest first; past <see cref="BreakdownSize"/> a count or a sum folds the rest into "Other".</summary>
    private static List<ChartPart> Top(IEnumerable<ChartPart> parts, bool withOther)
    {
        var sorted = parts.OrderByDescending(p => p.Value).ThenBy(p => p.Name).ToList();
        if (sorted.Count <= BreakdownSize)
        {
            return sorted;
        }

        var top = sorted.Take(withOther ? BreakdownSize - 1 : BreakdownSize).ToList();
        if (withOther)
        {
            top.Add(new ChartPart("Other", Math.Round(sorted.Skip(BreakdownSize - 1).Sum(p => p.Value), 1)));
        }

        return top;
    }

    /// <summary>The days, weeks (from Monday) or months of the period; the first and the last are cut to the period.</summary>
    private static List<(DateOnly Start, DateTimeOffset From, DateTimeOffset To)> Buckets(PeriodResponse period, ChartStep step)
    {
        var list = new List<(DateOnly, DateTimeOffset, DateTimeOffset)>();
        var start = step switch
        {
            ChartStep.Week => period.From.AddDays(-(((int)period.From.DayOfWeek + 6) % 7)),
            ChartStep.Month => new DateOnly(period.From.Year, period.From.Month, 1),
            _ => period.From,
        };
        while (start <= period.To)
        {
            var next = step switch
            {
                ChartStep.Week => start.AddDays(7),
                ChartStep.Month => start.AddMonths(1),
                _ => start.AddDays(1),
            };
            var first = start < period.From ? period.From : start;
            var last = next.AddDays(-1) > period.To ? period.To : next.AddDays(-1);
            list.Add((start, Start(first), Start(last.AddDays(1))));
            start = next;
        }

        return list;
    }
}
