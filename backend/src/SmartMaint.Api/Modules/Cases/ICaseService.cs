using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// Cases (F3 incidents, F4 emergencies, F2 work orders). Every method reads the caller from ICurrentUser and applies the
/// data scope itself (B3): a customer sees own and followed cases, staff and managers the buildings assigned to them,
/// a technician the cases assigned to them. A case outside the caller's scope is "not found". Status changes follow
/// CaseTransitions, else "case.invalid_transition".
/// </summary>
public interface ICaseService
{
    Task<PagedResult<CaseSummaryResponse>> ListAsync(PageRequest page, CancellationToken cancellationToken);

    Task<CaseDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff: the customers of the case's building who may follow it (meeting 07/11).</summary>
    Task<IReadOnlyList<PersonResponse>> GetFollowerOptionsAsync(long id, CancellationToken cancellationToken);

    /// <summary>Customer: a new request.</summary>
    Task<CaseDetailResponse> CreateAsync(CreateCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> CancelAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Staff: the work order form. Corrects the request, names the technician, plans the window and the tasks
    /// (meeting 07/11: no separate "Assign" step). Urgency "emergency" hands the case over to F4.
    /// </summary>
    Task<CaseDetailResponse> ApproveAsync(long id, ApproveCaseRequest request, CancellationToken cancellationToken);

    /// <summary>Staff. A duplicate may hand its content and its customer to the case it repeats.</summary>
    Task<CaseDetailResponse> RejectAsync(long id, RejectCaseRequest request, CancellationToken cancellationToken);

    /// <summary>Staff, meeting 07/11: the one Edit (work order form + continue / close / reopen).</summary>
    Task<CaseDetailResponse> EditAsync(long id, EditCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> AcceptAsync(long id, CancellationToken cancellationToken);

    Task<CaseDetailResponse> DeclineAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>Status → In progress. The SLA clock starts here (due = now + SLA hours) and the work clock runs.</summary>
    Task<CaseDetailResponse> StartAsync(long id, CancellationToken cancellationToken);

    /// <summary>Status → On hold (meeting 07/11: no reason, no date). The work clock stops.</summary>
    Task<CaseDetailResponse> HoldAsync(long id, CancellationToken cancellationToken);

    /// <summary>Status → In progress again; the work clock runs.</summary>
    Task<CaseDetailResponse> ResumeAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Status → Completed, nothing to fill in (meeting 07/11). A work order needs every checklist item done. The work
    /// clock stops and the labour line is set. Not Closed: the staff still checks.
    /// </summary>
    Task<CaseDetailResponse> CompleteAsync(long id, CompleteWorkRequest request, CancellationToken cancellationToken);

    /// <summary>Technician: the total work hours, typed (meeting 07/11).</summary>
    Task<CaseDetailResponse> LogHoursAsync(long id, LogHoursRequest request, CancellationToken cancellationToken);

    /// <summary>Technician: a cost they paid, with the receipt (meeting 07/11).</summary>
    Task<CaseDetailResponse> AddCostAsync(long id, AddCostRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> RateAsync(long id, RateCaseRequest request, CancellationToken cancellationToken);

    /// <summary>O09: a later deadline, only before the current one passes.</summary>
    Task<CaseDetailResponse> ExtendDueAsync(long id, ExtendDueRequest request, CancellationToken cancellationToken);

    /// <summary>Customer, within 7 days of closing: "it is not fixed". Waits for the staff member.</summary>
    Task<CaseDetailResponse> ReportNotFixedAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>Staff agrees: Reopened, same technician.</summary>
    Task<CaseDetailResponse> AcceptNotFixedAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff disagrees, with a reason the customer is told.</summary>
    Task<CaseDetailResponse> DeclineNotFixedAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>Staff records the cost lines (title, description, amount), any time from approval until shortly after closing.</summary>
    Task<CaseDetailResponse> UpdateCostsAsync(long id, UpdateCostsRequest request, CancellationToken cancellationToken);

    /// <summary>Technician: status, note and captioned photos of one task; a failed checklist item can make a repair case (BR-07).</summary>
    Task<CaseDetailResponse> UpdateTaskAsync(long id, long taskId, UpdateTaskRequest request, CancellationToken cancellationToken);

    /// <summary>Staff: this task was done by an outside party (or give it back to the technician).</summary>
    Task<CaseDetailResponse> SetTaskThirdPartyAsync(long id, long taskId, SetTaskThirdPartyRequest request, CancellationToken cancellationToken);

    // ----- F4 emergency ---------------------------------------------------------------------------------------------

    /// <summary>Staff, F4 step ②: first response; stops the escalation to the backup staff and the manager.</summary>
    Task<CaseDetailResponse> AcknowledgeAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff: a line of the emergency log (third party called, temporary measure, news for the customer).</summary>
    Task<CaseDetailResponse> AddLogAsync(long id, EmergencyLogRequest request, CancellationToken cancellationToken);

    /// <summary>Technician, K06: the danger is under control (first milestone).</summary>
    Task<CaseDetailResponse> MarkSafeAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff, K08: optional post-incident report of a closed emergency.</summary>
    Task<CaseDetailResponse> WriteIncidentReportAsync(long id, IncidentReportRequest request, CancellationToken cancellationToken);

    // ----- F5 repair or replace -------------------------------------------------------------------------------------

    /// <summary>Technician, steps ①–②: the assessment; On Hold until the staff decide.</summary>
    Task<CaseDetailResponse> RequestDecisionAsync(long id, RequestDecisionRequest request, CancellationToken cancellationToken);

    /// <summary>Staff, meeting 07/11: the job goes to an outside party, agreed outside the system. Nothing else is recorded.</summary>
    Task<CaseDetailResponse> OutsourceAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff, F5 way 2: a survey case for an asset that fails often.</summary>
    Task<CaseDetailResponse> OpenSurveyAsync(OpenSurveyRequest request, CancellationToken cancellationToken);
}
