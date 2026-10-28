namespace SmartMaint.Api.Entities.Cases;

/// <summary>The 11 case statuses (F3 page 5). Stored as snake_case text: "in_progress", "on_hold"...</summary>
public enum CaseStatus
{
    Pending,
    Approved,
    Assigned,
    Accepted,
    InProgress,
    OnHold,
    Completed,
    Closed,
    Reopened,
    Rejected,
    Cancelled,
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

public enum AssignmentAnswer
{
    Waiting,
    Accepted,
    Declined,
}
