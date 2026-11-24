using SmartMaint.Api.Modules.Maintenance.Dtos;

namespace SmartMaint.Api.Modules.Analytics.Dtos;

// F6 (docs/flows/F6_Giam_Sat, Leader 03/11). Names match frontend/src/features/analytics/types.ts.

/// <summary>
/// ?from=yyyy-MM-dd&amp;to=yyyy-MM-dd&amp;buildingId= — local dates (UTC+7), both included. Default (meeting note 18/11):
/// from the same day a month ago to today. The building must be one the caller looks after.
/// </summary>
public record AnalyticsQuery
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public long? BuildingId { get; init; }
}

public sealed record PeriodResponse(DateOnly From, DateOnly To);

/// <summary>
/// ANA-01 KPIs of one period (incidents F3 / F4; preventive work orders are in <see cref="PreventiveStats"/>).
/// Counted: reported in the period, except duplicates and surveys (N5). Rates use the cases completed in the period;
/// the on-time rate also counts, as missed, the cases not done yet whose deadline passed in the period.
/// AvgFirstResponseMinutes: report → the staff's first decision (an emergency: → acknowledged).
/// AvgResolutionHours: start of work (an emergency: when it became one) → reported done; not from the report, since a
/// low urgency case may wait on purpose (meeting note 18/11). Ratings = how many ratings AvgRating averages.
/// </summary>
public sealed record KpiSet(
    int Reported,
    int Rejected,
    int Cancelled,
    int Completed,
    double? AvgFirstResponseMinutes,
    double? AvgResolutionHours,
    double? OnTimeRate,
    double? OnTimeOriginalRate,
    int ExtendedCases,
    int DeadlineMoves,
    double? ReopenRate,
    double? AvgRating,
    int Ratings,
    int TechnicianDeclines);

/// <summary>F4 milestones, averaged from when the case became an emergency (24/7).</summary>
public sealed record EmergencyStats(
    int Count, double? AvgAcknowledgeMinutes, double? AvgFixHours, int Downgraded, int Merged);

/// <summary>S03: a preventive work order is on time when reported done by the end of its due day.</summary>
public sealed record PreventiveStats(int Due, int OnTime, double? OnTimeRate, int Inspected, double? PassedRate);

/// <summary>On Hold periods that started in the period, grouped by the reason they started with (N13).</summary>
public sealed record HoldTimeRow(string Reason, int Times, double TotalHours, double AvgHours);

public sealed record WeekPoint(DateOnly WeekStart, int Reported, int Completed);

/// <summary>
/// One building. OpenNow = all work not finished (pending → waiting for verification, preventive included, merged
/// reports aside); OverdueNow = of those, not reported done and past the deadline. OnTimeRate as in <see cref="KpiSet"/>.
/// </summary>
public sealed record BuildingRow(
    long BuildingId, string Name, int Reported, int Completed, double? OnTimeRate, int OpenNow, int OverdueNow);

/// <summary>ANA-01 (MGR-01 tiles use the same numbers): totals and averages of the period. AvgRating: managers only.</summary>
public sealed record OverviewResponse(
    PeriodResponse Period,
    KpiSet Current,
    EmergencyStats Emergency,
    PreventiveStats Preventive,
    IReadOnlyList<HoldTimeRow> HoldTime,
    double? AvgOutsideHours,
    IReadOnlyList<WeekPoint> Weekly,
    IReadOnlyList<BuildingRow> ByBuilding);

/// <summary>ANA-02 one technician. FewCases: under 5 completed, the rates are not reliable yet.</summary>
public sealed record TechnicianRow(
    PersonRef Technician,
    int Completed,
    double? AvgCompletionHours,
    double? OnTimeRate,
    double? ReopenRate,
    double? AvgRating,
    decimal LaborHours,
    int OpenNow,
    int Declines,
    IReadOnlyList<string> DeclineReasons,
    bool FewCases);

public sealed record ServiceTypeRow(long Id, string Name, int Cases, double? AvgResolutionHours);

public sealed record GroupRow(string Group, int Cases);

public sealed record ProblemAssetRow(
    long Id,
    string Name,
    string Location,
    string TypeName,
    int Repairs,
    int RepairsLast30Days,
    decimal AgeYears,
    int? ExpectedLifeYears,
    bool UnderWarranty);

/// <summary>ANA-03 failure trends and problem assets.</summary>
public sealed record FailuresResponse(
    PeriodResponse Period,
    IReadOnlyList<ServiceTypeRow> ByServiceType,
    IReadOnlyList<GroupRow> ByGroup,
    IReadOnlyList<GroupRow> ByAssetType,
    IReadOnlyList<ProblemAssetRow> ProblemAssets);

public enum AttentionKind
{
    /// <summary>Past its deadline and not reported done (meeting note 18/11: every one, not only after 24 hours).</summary>
    OverdueCase,

    /// <summary>On Hold for more than 3 days.</summary>
    LongHold,

    /// <summary>A preventive work order past its due day.</summary>
    PreventiveOverdue,

    /// <summary>An asset repaired 3 times or more in 30 days.</summary>
    FrequentFailure,

    /// <summary>An open case whose deadline moved twice or more.</summary>
    ExtendedTwice,

    /// <summary>A technician declined 3 jobs or more in 7 days (N10).</summary>
    TechnicianDeclines,

    /// <summary>Approved, but nobody has it: the technician declined and no one was chosen again.</summary>
    NoTechnician,

    /// <summary>The customer says a closed case is still not fixed and the staff have not answered.</summary>
    NotFixedReport,

    /// <summary>An emergency nobody has acknowledged yet.</summary>
    EmergencyNotAcknowledged,
}

/// <summary>One line of "Needs attention" (F6 step ④). Only shown; F2 / F3 / F4 already sent their notifications.</summary>
public sealed record AttentionItem(
    AttentionKind Kind,
    string Text,
    long? CaseId,
    string? CaseCode,
    long? AssetId,
    long? PlanId,
    long? TechnicianId,
    DateTimeOffset? Since);

// ----- charts (meeting note 18/11) ---------------------------------------------------------------------------------

/// <summary>What a chart shows. Each topic of the Charts tab has its own metrics.</summary>
public enum ChartMetric
{
    CasesReported,
    CasesCompleted,
    CasesLate,
    TechnicianCompleted,
    TechnicianOnTime,
    TechnicianLabourHours,
    StaffDecisions,
    StaffFirstResponse,
    CostTotal,
    CostByFault,
}

/// <summary>One point of the chart per day, week (Monday) or month.</summary>
public enum ChartStep
{
    Day,
    Week,
    Month,
}

public sealed record ChartQuery : AnalyticsQuery
{
    public ChartMetric Metric { get; init; }
    public ChartStep Step { get; init; } = ChartStep.Week;
}

/// <summary>A point of the line: the first day of its day / week / month, and its value (empty when nothing to average).</summary>
public sealed record ChartPoint(DateOnly Start, double? Value);

public sealed record ChartPart(string Name, double Value);

/// <summary>
/// The metric over time, and the same metric over the whole period split by <see cref="BreakdownBy"/> (a building,
/// a technician, a staff member or a fault category), largest first. Unit: "cases", "%", "hours", "minutes" or "VND".
/// </summary>
public sealed record ChartResponse(
    PeriodResponse Period,
    ChartMetric Metric,
    ChartStep Step,
    string Unit,
    IReadOnlyList<ChartPoint> Points,
    string BreakdownBy,
    IReadOnlyList<ChartPart> Breakdown);

// ----- Excel export (meeting note 18/11) ---------------------------------------------------------------------------

/// <summary>The tab of the analytics page the export copies.</summary>
public enum ExportTab
{
    Overview,
    Charts,
    Technicians,
    Failures,
    Attention,
}

/// <summary>
/// The page as it is on screen: its tab, period and building, the chart's metric and step, the technician table's
/// sort. The file also has a sheet of every case reported in the period.
/// </summary>
public sealed record ExportQuery : AnalyticsQuery
{
    public ExportTab Tab { get; init; }
    public ChartMetric Metric { get; init; }
    public ChartStep Step { get; init; } = ChartStep.Week;

    /// <summary>The technician column the table is sorted by (its web name, e.g. "onTime"); empty = most completed first.</summary>
    public string? Sort { get; init; }
    public bool Descending { get; init; }
}
