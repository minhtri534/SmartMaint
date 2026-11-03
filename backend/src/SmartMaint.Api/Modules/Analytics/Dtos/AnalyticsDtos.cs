using SmartMaint.Api.Modules.Maintenance.Dtos;

namespace SmartMaint.Api.Modules.Analytics.Dtos;

// F6 (docs/flows/F6_Giam_Sat, Leader 03/11). Names match frontend/src/features/analytics/types.ts.

/// <summary>
/// ?from=yyyy-MM-dd&amp;to=yyyy-MM-dd&amp;buildingId= — local dates (UTC+7), both included. Default: the last 30 days.
/// The building must be one the caller looks after.
/// </summary>
public sealed record AnalyticsQuery
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public long? BuildingId { get; init; }
}

public sealed record PeriodResponse(DateOnly From, DateOnly To);

/// <summary>
/// ANA-01 KPIs of one period (incidents F3 / F4; preventive work orders are in <see cref="PreventiveStats"/>).
/// Counted: reported in the period, except merged reports and surveys (N5). Rates use the cases completed in the
/// period. OnTimeRate = by the deadline in force (after moves); OnTimeOriginalRate = by the first SLA deadline (S02).
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
    int TechnicianDeclines);

/// <summary>F4 milestones, averaged from when the case became an emergency (24/7).</summary>
public sealed record EmergencyStats(
    int Count, double? AvgAcknowledgeMinutes, double? AvgMadeSafeMinutes, double? AvgFixHours, int Downgraded, int Merged);

/// <summary>S03: a preventive work order is on time when reported done by the end of its due day.</summary>
public sealed record PreventiveStats(int Due, int OnTime, double? OnTimeRate, int Inspected, double? PassedRate);

/// <summary>On Hold periods that started in the period, grouped by the reason they started with (N13).</summary>
public sealed record HoldTimeRow(string Reason, int Times, double TotalHours, double AvgHours);

public sealed record WeekPoint(DateOnly WeekStart, int Reported, int Completed);

public sealed record BuildingRow(
    long BuildingId, string Name, int Reported, int Completed, double? OnTimeRate, int OpenNow, int OverdueNow);

/// <summary>ANA-01 (MGR-01 tiles use the same numbers). AvgRating is for managers only.</summary>
public sealed record OverviewResponse(
    PeriodResponse Period,
    PeriodResponse PreviousPeriod,
    KpiSet Current,
    KpiSet Previous,
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
    IReadOnlyList<ProblemAssetRow> ProblemAssets);

public enum AttentionKind
{
    /// <summary>Past its deadline by more than 24 hours.</summary>
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

/// <summary>ANA-04 exports (CSV, Excel opens it).</summary>
public enum ExportReport
{
    Cases,
    Assets,
    Preventive,
    Technicians,
}
