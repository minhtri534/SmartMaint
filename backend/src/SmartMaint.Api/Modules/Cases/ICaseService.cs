using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// F3 (normal incident). Every method reads the caller from ICurrentUser and applies the data scope itself (B3):
/// a customer sees own cases, staff and managers the buildings assigned to them, a technician the cases assigned to them.
/// A case outside the caller's scope is "not found". Status changes follow CaseTransitions, else "case.invalid_transition".
/// </summary>
public interface ICaseService
{
    Task<PagedResult<CaseSummaryResponse>> ListAsync(PageRequest page, CancellationToken cancellationToken);

    Task<CaseDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>F3 step 1 (customer).</summary>
    Task<CaseDetailResponse> CreateAsync(CreateCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> CancelAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>F3 step 2 (staff): confirm urgency and who pays. Urgency "emergency" hands the case over to F4.</summary>
    Task<CaseDetailResponse> ApproveAsync(long id, ApproveCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> RejectAsync(long id, RejectCaseRequest request, CancellationToken cancellationToken);

    /// <summary>F3 step 3 (staff).</summary>
    Task<CaseDetailResponse> AssignAsync(long id, AssignCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> AcceptAsync(long id, CancellationToken cancellationToken);

    Task<CaseDetailResponse> DeclineAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>The SLA clock starts here: due = now + the urgency's SLA hours.</summary>
    Task<CaseDetailResponse> StartAsync(long id, CancellationToken cancellationToken);

    Task<CaseDetailResponse> HoldAsync(long id, HoldCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> ResumeAsync(long id, CancellationToken cancellationToken);

    /// <summary>F3 step 5: the repair record, then Completed (not Closed: the staff member still verifies).</summary>
    Task<CaseDetailResponse> CompleteAsync(long id, CompleteWorkRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> CloseAsync(long id, CancellationToken cancellationToken);

    Task<CaseDetailResponse> ReopenAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> RateAsync(long id, RateCaseRequest request, CancellationToken cancellationToken);

    /// <summary>Staff gives the work to another technician (back to Assigned; the SLA clock keeps running).</summary>
    Task<CaseDetailResponse> ReassignAsync(long id, ReassignCaseRequest request, CancellationToken cancellationToken);

    /// <summary>O08: a new urgency with a reason; the deadline follows it; Emergency hands the case to F4.</summary>
    Task<CaseDetailResponse> ChangeUrgencyAsync(long id, ChangeUrgencyRequest request, CancellationToken cancellationToken);

    /// <summary>O09: a later deadline, only before the current one passes.</summary>
    Task<CaseDetailResponse> ExtendDueAsync(long id, ExtendDueRequest request, CancellationToken cancellationToken);

    /// <summary>Customer, within 7 days of closing: "it is not fixed". Waits for the staff member.</summary>
    Task<CaseDetailResponse> ReportNotFixedAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>Staff agrees: Reopened, same technician.</summary>
    Task<CaseDetailResponse> AcceptNotFixedAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff disagrees, with a reason the customer is told.</summary>
    Task<CaseDetailResponse> DeclineNotFixedAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>Staff corrects the cost lines before closing (status Completed).</summary>
    Task<CaseDetailResponse> UpdateCostsAsync(long id, UpdateCostsRequest request, CancellationToken cancellationToken);

    /// <summary>F2 TEC-05: the result of one checklist item; a failed item can make a repair case (BR-07).</summary>
    Task<CaseDetailResponse> UpdateTaskAsync(long id, long taskId, UpdateTaskRequest request, CancellationToken cancellationToken);

    /// <summary>F2: every item has a result -> Completed (the staff verify as in F3).</summary>
    Task<CaseDetailResponse> CompleteChecklistAsync(long id, CompleteChecklistRequest request, CancellationToken cancellationToken);

    // ----- F4 emergency ---------------------------------------------------------------------------------------------

    /// <summary>Staff, F4 step ②: first response; stops the escalation to the backup staff and the manager.</summary>
    Task<CaseDetailResponse> AcknowledgeAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff, K04: not an emergency — back to F3 with an urgency, who pays and why (before a technician accepts).</summary>
    Task<CaseDetailResponse> DowngradeAsync(long id, DowngradeRequest request, CancellationToken cancellationToken);

    /// <summary>Staff, N5: same incident as another open case of the building; this one is closed as merged.</summary>
    Task<CaseDetailResponse> MergeAsync(long id, MergeCaseRequest request, CancellationToken cancellationToken);

    /// <summary>Staff: a line of the emergency log (third party called, temporary measure, news for the customer).</summary>
    Task<CaseDetailResponse> AddLogAsync(long id, EmergencyLogRequest request, CancellationToken cancellationToken);

    /// <summary>Technician, K06: the danger is under control (first milestone).</summary>
    Task<CaseDetailResponse> MarkSafeAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff, K08: optional post-incident report of a closed emergency.</summary>
    Task<CaseDetailResponse> WriteIncidentReportAsync(long id, IncidentReportRequest request, CancellationToken cancellationToken);

    // ----- F5 repair or replace -------------------------------------------------------------------------------------

    /// <summary>Technician, steps ①–②: the assessment; On Hold until the staff decide.</summary>
    Task<CaseDetailResponse> RequestDecisionAsync(long id, RequestDecisionRequest request, CancellationToken cancellationToken);

    /// <summary>Staff, steps ④–⑤: hand the job to the manufacturer or a contractor.</summary>
    Task<CaseDetailResponse> OutsourceAsync(long id, OutsourceRequest request, CancellationToken cancellationToken);

    /// <summary>Staff: the contractor's quote arrived (who pays; forwarded to whom).</summary>
    Task<CaseDetailResponse> RecordQuoteAsync(long id, RecordQuoteRequest request, CancellationToken cancellationToken);

    /// <summary>Staff, step ⑥: the payer's decision on the quote, with evidence.</summary>
    Task<CaseDetailResponse> DecideQuoteAsync(long id, QuoteDecisionRequest request, CancellationToken cancellationToken);

    /// <summary>Staff: the outside party refused (warranty claim declined...).</summary>
    Task<CaseDetailResponse> OutsideDeclinedAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>Technician, step ⑧: the outside party's work checked; OK → In Progress.</summary>
    Task<CaseDetailResponse> CheckOutsideWorkAsync(long id, CheckOutsideWorkRequest request, CancellationToken cancellationToken);

    /// <summary>Staff: the check failed, the same party redoes it.</summary>
    Task<CaseDetailResponse> RedoOutsideWorkAsync(long id, RedoOutsideWorkRequest request, CancellationToken cancellationToken);

    /// <summary>Staff, F5 way 2: a survey case for an asset that fails often.</summary>
    Task<CaseDetailResponse> OpenSurveyAsync(OpenSurveyRequest request, CancellationToken cancellationToken);
}
