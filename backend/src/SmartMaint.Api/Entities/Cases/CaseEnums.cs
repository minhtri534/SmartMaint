namespace SmartMaint.Api.Entities.Cases;

/// <summary>
/// The 11 case statuses (F3 page 5). ERD: CaseStatuses(ID, Name) — the value is the row ID, the name is the snake_case
/// text the API sends ("in_progress", "on_hold"...).
/// </summary>
public enum CaseStatus
{
    Pending = 1,
    Approved = 2,
    Assigned = 3,
    Accepted = 4,
    InProgress = 5,
    OnHold = 6,
    Completed = 7,
    Closed = 8,
    Reopened = 9,
    Rejected = 10,
    Cancelled = 11,
}

/// <summary>Urgency level. The lookup row (priority_levels) holds the SLA hours of each level.</summary>
public enum CasePriority
{
    Emergency,
    High,
    Normal,
    Low,
}

/// <summary>Who pays for the repair (N23). NOT IN ERD v5.</summary>
public enum CostBearer
{
    Building,
    Customer,
}

/// <summary>Why a technician put the work on hold. A fixed list, not free text (N13).</summary>
public enum HoldReason
{
    WaitingParts,
    WaitingCustomer,
    WaitingStaffDecision,
    WaitingThirdParty,
}

public enum CaseImageKind
{
    Report,
    Before,
    After,
}

/// <summary>F4 step ⑫: what the post-incident report proposes so it does not happen again.</summary>
public enum IncidentFollowUp
{
    None,

    /// <summary>Make a preventive maintenance plan (F2).</summary>
    PreventivePlan,

    /// <summary>Consider replacing the asset (F5).</summary>
    ReplacementReview,
}
