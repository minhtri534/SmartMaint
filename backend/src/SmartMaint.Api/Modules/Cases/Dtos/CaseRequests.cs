using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Cases.Dtos;

/// <summary>Why an officer rejects a case. "Duplicate" replaces the old merge: the customer is told a case already exists (28/10).</summary>
public enum RejectReason
{
    MissingInfo,
    OutOfScope,
    Duplicate,
}

/// <summary>
/// EMP-02. Photos are image data URLs; the API stores them in Supabase Storage and keeps the path.
/// Every request names an asset (Outscope 23). Urgency = what the customer chose, Normal when empty. Even "Emergency"
/// stays Pending until the staff approve it (meeting note 18/11).
/// </summary>
public sealed record CreateCaseRequest(
    string Title,
    string Description,
    long ServiceTypeId,
    long RoomId,
    long? AssetId,
    IReadOnlyList<string>? Photos,
    CasePriority? Urgency = null);

/// <summary>One task of the work order form. Id empty = a new task; an existing task left out of the list is removed.</summary>
public sealed record TaskLineRequest(long? Id, string Name, string? Description);

/// <summary>
/// DLG-01 Approve = the work order form (meeting 07/11): the customer's request as the staff corrects it (title,
/// description, category, urgency) + the technician, the planned window and the tasks. There is no "Assign" step any
/// more. Title / Description empty = keep the customer's. No tasks = one task named after the case. The planned start is
/// required unless it is an emergency: the expected due time is counted from it (meeting note 18/11).
/// </summary>
public sealed record ApproveCaseRequest(
    CasePriority Urgency,
    long TechnicianId,
    long? ServiceTypeId = null,
    DateTimeOffset? PlannedStartAt = null,
    DateTimeOffset? PlannedEndAt = null,
    IReadOnlyList<TaskLineRequest>? Tasks = null,
    string? Title = null,
    string? Description = null);

/// <summary>What the staff does to the status from the Edit form (meeting 07/11: no separate buttons any more).</summary>
public enum EditStatusChange
{
    /// <summary>On hold → In progress: the technician continues (was "Let technician continue").</summary>
    Continue,

    /// <summary>Completed → Closed after checking the work (was "Verify and close").</summary>
    Close,

    /// <summary>Completed / Closed (within 7 days) → Reopened. Needs a reason.</summary>
    Reopen,
}

/// <summary>
/// The one "Edit" of the staff (meeting 07/11), replacing change urgency, let continue, close and reopen. The technician
/// and the followers have their own buttons (meeting note 18/11).
/// </summary>
public sealed record EditCaseRequest(
    CasePriority Urgency,
    long ServiceTypeId,
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    IReadOnlyList<TaskLineRequest> Tasks,
    string Title,
    string Description,
    EditStatusChange? StatusChange = null,
    string? Reason = null);

/// <summary>
/// Meeting note 18/11. Nobody has accepted yet: the case simply gets this technician. The job was already accepted: a new
/// case continues it with the unfinished tasks; the finished ones stay on this case.
/// </summary>
public sealed record ReassignRequest(long TechnicianId);

/// <summary>Every customer who follows the case besides the requester.</summary>
public sealed record SetFollowersRequest(IReadOnlyList<long> CustomerIds);

/// <summary>
/// Meeting 07/11 (replaces merge): the request repeats <see cref="CaseId"/>. Its content may go there as a task, its
/// photos may be copied, and its customer may follow that case; then this request is rejected as a duplicate.
/// </summary>
public sealed record DuplicateOfRequest(long CaseId, bool AddAsTask, bool CopyPhotos, bool AddFollower);

/// <summary>DLG-02. Duplicate is only read when Reason = Duplicate.</summary>
public sealed record RejectCaseRequest(RejectReason Reason, string Note, DuplicateOfRequest? Duplicate = null);

/// <summary>The staff marks a task as done by an outside party (true) or gives it back to the technician (false).</summary>
public sealed record SetTaskThirdPartyRequest(bool ThirdParty);

/// <summary>Cancel (DLG-12), decline (DLG-09) and reopen (DLG-07) only need a written reason.</summary>
public sealed record ReasonRequest(string Reason);

/// <summary>
/// The technician sets the status to Completed (meeting 07/11): nothing else to fill in — the tasks, the hours and the
/// costs were recorded on the way. Only an asset replaced by an outside party (old F5 flow) needs the new model.
/// </summary>
public sealed record CompleteWorkRequest;

/// <summary>DLG-14.</summary>
public sealed record RateCaseRequest(int Stars, string? Comment);

/// <summary>
/// One cost line (ERD: Costs). Amount in VND. Id = an existing line the staff keeps (its receipt stays); empty = new.
/// </summary>
public sealed record CostLineRequest(string Name, string? Description, decimal Amount, long? Id = null);

/// <summary>
/// Meeting 07/11: the technician records a cost they paid — title, description, amount and the receipt (its number,
/// a photo of it, or both).
/// </summary>
public sealed record AddCostRequest(string Name, string? Description, decimal Amount, string? ReceiptCode, string? ReceiptPhoto);

/// <summary>Meeting note 18/11: the technician corrects their own cost line. ReceiptPhoto empty = keep the current photo.</summary>
public sealed record EditCostRequest(string Name, string? Description, decimal Amount, string? ReceiptCode, string? ReceiptPhoto);

/// <summary>Meeting 07/11: the technician types the total work hours (e.g. forgot to start the clock).</summary>
public sealed record LogHoursRequest(decimal Hours);

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

/// <summary>The staff records the costs (meeting 07/11: the staff, not the technician, keeps the cost lines).</summary>
public sealed record UpdateCostsRequest(IReadOnlyList<CostLineRequest> Lines);

/// <summary>F2: what the technician does with a failed checklist item (BR-07).</summary>
public enum FailAction
{
    /// <summary>Fixed on the spot; the note says what was done.</summary>
    FixedOnSite,

    /// <summary>A repair case is made (F3, Approved for the staff to assign; Emergency = F4 when dangerous).</summary>
    CreateCase,
}

/// <summary>A photo for a task, with an optional caption (ERD: TaskImages(Image, Notes)).</summary>
public sealed record TaskPhotoRequest(string Image, string? Caption);

/// <summary>
/// TEC-05, meeting 07/11: the technician updates one task — its status (still to do, successful, failed, N/A), a note
/// and photos with captions. A failure needs a note and a photo; on a work order it may make a repair case (BR-07).
/// </summary>
public sealed record UpdateTaskRequest(
    Entities.Maintenance.CaseTaskStatus Status,
    string? Note,
    IReadOnlyList<TaskPhotoRequest>? Photos,
    FailAction? FailAction,
    bool Dangerous);

// ----- F4 emergency -------------------------------------------------------------------------------------------------

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

/// <summary>
/// Meeting 07/11 (replaces the technician's assessment): the technician asks the staff to hand the job to an outside
/// party — what is wrong (description), why they cannot do it (reason), and at least one photo. The case waits On Hold.
/// </summary>
public sealed record RequestDecisionRequest(string Description, string Reason, IReadOnlyList<string> Photos);

/// <summary>F5 way 2: a survey of an asset that fails often, assigned like any case.</summary>
public sealed record OpenSurveyRequest(long AssetId, long ServiceTypeId, string? Note);
