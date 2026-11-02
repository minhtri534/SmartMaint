using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Cases.Dtos;

/// <summary>Why an officer rejects a case. "Duplicate" replaces the old merge: the customer is told a case already exists (28/10).</summary>
public enum RejectReason
{
    MissingInfo,
    OutOfScope,
    Duplicate,
}

/// <summary>EMP-02. Photos are image data URLs; the API stores them in Supabase Storage and keeps the path.</summary>
public sealed record CreateCaseRequest(
    string Title,
    string Description,
    long ServiceTypeId,
    long RoomId,
    long? AssetId,
    IReadOnlyList<string>? Photos);

/// <summary>DLG-01: the staff member confirms the urgency and who pays.</summary>
public sealed record ApproveCaseRequest(CasePriority Urgency, CostBearer CostBearer);

/// <summary>DLG-02.</summary>
public sealed record RejectCaseRequest(RejectReason Reason, string Note);

/// <summary>DLG-04.</summary>
public sealed record AssignCaseRequest(long TechnicianId);

/// <summary>Cancel (DLG-12), decline (DLG-09) and reopen (DLG-07) only need a written reason.</summary>
public sealed record ReasonRequest(string Reason);

/// <summary>DLG-10: pause the work with a fixed reason and the day it should continue.</summary>
public sealed record HoldCaseRequest(HoldReason Reason, DateOnly ExpectedResumeOn);

/// <summary>
/// DLG-11: the repair record. LaborHours empty = the system measures start to finish (doc "Luồng Chính").
/// Parts = what the technician bought (cost lines); the labour line is added by the system.
/// F5: when an outside party replaced the asset, the new model (and serial) is required.
/// </summary>
public sealed record CompleteWorkRequest(
    string RootCause,
    string? PartsUsed,
    decimal? LaborHours,
    IReadOnlyList<string>? BeforePhotos,
    IReadOnlyList<string> AfterPhotos,
    IReadOnlyList<CostLineRequest>? Parts = null,
    string? ReplacementModel = null,
    string? ReplacementSerial = null);

/// <summary>DLG-14.</summary>
public sealed record RateCaseRequest(int Stars, string? Comment);

/// <summary>One cost line (ERD: Costs). Amount in VND.</summary>
public sealed record CostLineRequest(string Name, string? Description, decimal Amount);

/// <summary>Staff gives the work to another technician (not a decline). The new one must accept again.</summary>
public sealed record ReassignCaseRequest(long TechnicianId, string Reason);

/// <summary>O08: the problem turned out more (or less) urgent. The deadline follows the new level.</summary>
public sealed record ChangeUrgencyRequest(CasePriority Urgency, string Reason);

/// <summary>Why a deadline moves (O09: a fixed list, so the statistics can count them).</summary>
public enum ExtendDueReason
{
    WaitingParts,
    CustomerRescheduled,
    LargerThanExpected,
    Other,
}

/// <summary>O09: only before the deadline passes, and only later.</summary>
public sealed record ExtendDueRequest(DateTimeOffset NewDueAt, ExtendDueReason Reason, string? Note);

/// <summary>Staff corrects the cost lines before closing.</summary>
public sealed record UpdateCostsRequest(IReadOnlyList<CostLineRequest> Lines);

/// <summary>F2: what the technician does with a failed checklist item (BR-07).</summary>
public enum FailAction
{
    /// <summary>Fixed on the spot; the note says what was done.</summary>
    FixedOnSite,

    /// <summary>A repair case is made (F3, Approved for the staff to assign; Emergency = F4 when dangerous).</summary>
    CreateCase,
}

/// <summary>F2 TEC-05: the result of one checklist item. Fail needs a note, a photo and a FailAction.</summary>
public sealed record UpdateTaskRequest(
    Entities.Maintenance.CaseTaskStatus Status,
    string? Note,
    IReadOnlyList<string>? Photos,
    FailAction? FailAction,
    bool Dangerous);

/// <summary>F2: every item has a result; report the preventive work order done.</summary>
public sealed record CompleteChecklistRequest(string? Note, decimal? LaborHours, IReadOnlyList<CostLineRequest>? Parts);

// ----- F4 emergency -------------------------------------------------------------------------------------------------

/// <summary>K04: not an emergency after all — back to F3 with a normal urgency, who pays, and why (before a technician accepts).</summary>
public sealed record DowngradeRequest(CasePriority Urgency, CostBearer CostBearer, string Reason);

/// <summary>N5: this report is the same incident as <see cref="TargetCaseId"/> (open, same building).</summary>
public sealed record MergeCaseRequest(long TargetCaseId, string? Note);

/// <summary>What a line of the emergency log is about (N3 third party, N2 / G2 temporary measure).</summary>
public enum EmergencyLogKind
{
    /// <summary>A contracted outside party was called: name, phone, when they come.</summary>
    ThirdParty,

    /// <summary>A temporary measure for the people affected (another room, a loaned device). Only written down.</summary>
    Workaround,

    /// <summary>News for the customer ("someone is on the way", "expected back at ...").</summary>
    CustomerUpdate,

    Other,
}

public sealed record EmergencyLogRequest(EmergencyLogKind Kind, string Note);

/// <summary>F4 step ⑫ post-incident report (K08, optional): damage, lessons, what to do so it does not happen again.</summary>
public sealed record IncidentReportRequest(string Damage, string Lessons, IncidentFollowUp FollowUp);

// ----- F5 repair or replace -----------------------------------------------------------------------------------------

/// <summary>Step ①–②: the technician cannot finish alone. An assessment with at least one photo; the case waits for the staff.</summary>
public sealed record RequestDecisionRequest(
    string Condition, string Cause, TechnicianRecommendation Recommendation, IReadOnlyList<string> Photos);

/// <summary>Steps ④–⑤: hand the job to the manufacturer (warranty) or a contractor (package contract, or a quote first).</summary>
public sealed record OutsourceRequest(
    OutsideWorkKind Kind, OutsideWorkJob Job, string CompanyName, string Contact, DateTimeOffset? AppointmentAt, string? Note);

/// <summary>The contractor's quote arrived: who pays it; for the customer, to whom it was forwarded (R04, no amount stored).</summary>
public sealed record RecordQuoteRequest(CostBearer Bearer, string? SentTo, string? Note);

/// <summary>Step ⑥: the payer's decision and its evidence (a photo of the email or the signed paper, data URL).</summary>
public sealed record QuoteDecisionRequest(bool Go, string DecidedBy, string? Note, string? Evidence);

/// <summary>Step ⑧: the technician checked what the outside party did.</summary>
public sealed record CheckOutsideWorkRequest(bool Passed, string? Note);

/// <summary>The check failed: the staff asked the party to do it again.</summary>
public sealed record RedoOutsideWorkRequest(DateTimeOffset? AppointmentAt, string Reason);

/// <summary>F5 way 2: a survey of an asset that fails often, assigned like any case.</summary>
public sealed record OpenSurveyRequest(long AssetId, long ServiceTypeId, string? Note);
