namespace SmartMaint.Api.Entities.Cases;

/// <summary>
/// The 10 case statuses (F3 page 5). ERD: CaseStatuses(ID, Name) — the value is the row ID, the name is the snake_case
/// text the API sends ("in_progress", "on_hold"...). Meeting 07/11: "Assigned" (id 3) is gone, the staff chooses the
/// technician on approval; the ids of the other rows stay.
/// </summary>
public enum CaseStatus
{
    Pending = 1,
    Approved = 2,
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

/// <summary>
/// Who pays (N23). Meeting 07/11: the system no longer records who pays for a case; only the old F5 quote rows
/// (case_third_parties.quote_bearer) still use it.
/// </summary>
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

    /// <summary>F5: the technician's assessment photos.</summary>
    Assessment,
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
