namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// How some status-keeping history rows start (CaseStatusHistory.Reason). The ERD has no table for deadline moves,
/// downgrades or holds, so F6 counts them from these rows: CaseService writes them, AnalyticsService reads them.
/// </summary>
public static class CaseHistoryTags
{
    /// <summary>O09: the staff moved the deadline.</summary>
    public const string DeadlineMoved = "Deadline moved";

    /// <summary>K04: an emergency went back to F3.</summary>
    public const string NotAnEmergency = "Not an emergency";

    /// <summary>F5 step ②: the technician asked for the staff's decision (the hold starts here).</summary>
    public const string NeedsDecision = "Needs the staff's decision";

    public const string WaitingParts = "Waiting for parts";
    public const string WaitingCustomer = "Waiting for the customer";
}
