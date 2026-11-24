using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// Cases. One endpoint per arrow of the state machine; the roles here are the coarse gate, the service decides per
/// case (data scope + status). Admin has no access to cases (BR-11).
/// </summary>
[ApiController]
[Route("api/cases")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Customer, UserRole.Staff, UserRole.Technician, UserRole.Manager)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class CasesController(ICaseService cases) : ControllerBase
{
    /// <summary>EMP-03 / OFF-02 / OFF-03 / TEC-02: the cases the caller may see, newest first.</summary>
    [HttpGet]
    public Task<PagedResult<CaseSummaryResponse>> List([FromQuery] PageRequest page, CancellationToken cancellationToken) =>
        cases.ListAsync(page, cancellationToken);

    /// <summary>COM-08 / TEC-03: one case with its history, photos and the actions the caller may take.</summary>
    [HttpGet("{id:long}")]
    public Task<CaseDetailResponse> Get(long id, CancellationToken cancellationToken) =>
        cases.GetAsync(id, cancellationToken);

    /// <summary>The followers dialog: who follows the case now and the customers found by username or e-mail.</summary>
    [HttpGet("{id:long}/follower-options")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<FollowerOptionsResponse> FollowerOptions(long id, [FromQuery] string? search, CancellationToken cancellationToken) =>
        cases.GetFollowerOptionsAsync(id, search, cancellationToken);

    [HttpPut("{id:long}/followers")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> SetFollowers(long id, SetFollowersRequest request, CancellationToken cancellationToken) =>
        cases.SetFollowersAsync(id, request, cancellationToken);

    /// <summary>Another technician for the case (see ICaseService.ReassignAsync).</summary>
    [HttpPost("{id:long}/reassign")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Reassign(long id, ReassignRequest request, CancellationToken cancellationToken) =>
        cases.ReassignAsync(id, request, cancellationToken);

    /// <summary>The technician corrects a cost line they added.</summary>
    [HttpPut("{id:long}/costs/{costId:long}")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> EditCost(long id, long costId, EditCostRequest request, CancellationToken cancellationToken) =>
        cases.EditCostAsync(id, costId, request, cancellationToken);

    /// <summary>EMP-02: report a problem.</summary>
    [HttpPost]
    [AuthorizeRoles(UserRole.Customer)]
    public async Task<CreatedAtActionResult> Create(CreateCaseRequest request, CancellationToken cancellationToken)
    {
        var created = await cases.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>DLG-12.</summary>
    [HttpPost("{id:long}/cancel")]
    [AuthorizeRoles(UserRole.Customer, UserRole.Staff)]
    public Task<CaseDetailResponse> Cancel(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        cases.CancelAsync(id, request, cancellationToken);

    /// <summary>DLG-01: approve with the work order form (technician, planned window, tasks).</summary>
    [HttpPost("{id:long}/approve")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Approve(long id, ApproveCaseRequest request, CancellationToken cancellationToken) =>
        cases.ApproveAsync(id, request, cancellationToken);

    /// <summary>DLG-02.</summary>
    [HttpPost("{id:long}/reject")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Reject(long id, RejectCaseRequest request, CancellationToken cancellationToken) =>
        cases.RejectAsync(id, request, cancellationToken);

    /// <summary>The staff's Edit (meeting 07/11).</summary>
    [HttpPut("{id:long}")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Edit(long id, EditCaseRequest request, CancellationToken cancellationToken) =>
        cases.EditAsync(id, request, cancellationToken);

    [HttpPost("{id:long}/accept")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> Accept(long id, CancellationToken cancellationToken) =>
        cases.AcceptAsync(id, cancellationToken);

    /// <summary>DLG-09.</summary>
    [HttpPost("{id:long}/decline")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> Decline(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        cases.DeclineAsync(id, request, cancellationToken);

    /// <summary>Start work on site: the SLA clock starts.</summary>
    [HttpPost("{id:long}/start")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> Start(long id, CancellationToken cancellationToken) =>
        cases.StartAsync(id, cancellationToken);

    /// <summary>Status → On hold (meeting 07/11: no reason, no date).</summary>
    [HttpPost("{id:long}/hold")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> Hold(long id, CancellationToken cancellationToken) =>
        cases.HoldAsync(id, cancellationToken);

    /// <summary>Meeting 07/11: the technician types the total work hours.</summary>
    [HttpPut("{id:long}/hours")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> LogHours(long id, LogHoursRequest request, CancellationToken cancellationToken) =>
        cases.LogHoursAsync(id, request, cancellationToken);

    /// <summary>Meeting 07/11: the technician records a cost they paid, with the receipt.</summary>
    [HttpPost("{id:long}/costs")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> AddCost(long id, AddCostRequest request, CancellationToken cancellationToken) =>
        cases.AddCostAsync(id, request, cancellationToken);

    /// <summary>The technician continues after a hold.</summary>
    [HttpPost("{id:long}/resume")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> Resume(long id, CancellationToken cancellationToken) =>
        cases.ResumeAsync(id, cancellationToken);

    /// <summary>Status → Completed (meeting 07/11: nothing else to fill in; the body is {}).</summary>
    [HttpPost("{id:long}/complete")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> Complete(long id, CompleteWorkRequest request, CancellationToken cancellationToken) =>
        cases.CompleteAsync(id, request, cancellationToken);

    /// <summary>DLG-14.</summary>
    [HttpPost("{id:long}/rating")]
    [AuthorizeRoles(UserRole.Customer)]
    public Task<CaseDetailResponse> Rate(long id, RateCaseRequest request, CancellationToken cancellationToken) =>
        cases.RateAsync(id, request, cancellationToken);

    [HttpPost("{id:long}/due")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> ExtendDue(long id, ExtendDueRequest request, CancellationToken cancellationToken) =>
        cases.ExtendDueAsync(id, request, cancellationToken);

    [HttpPost("{id:long}/not-fixed")]
    [AuthorizeRoles(UserRole.Customer)]
    public Task<CaseDetailResponse> ReportNotFixed(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        cases.ReportNotFixedAsync(id, request, cancellationToken);

    [HttpPost("{id:long}/not-fixed/accept")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> AcceptNotFixed(long id, CancellationToken cancellationToken) =>
        cases.AcceptNotFixedAsync(id, cancellationToken);

    /// <summary>Meeting note 18/11: a closed case is read only; reopening is the one action left.</summary>
    [HttpPost("{id:long}/reopen")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Reopen(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        cases.ReopenAsync(id, request, cancellationToken);

    [HttpPost("{id:long}/not-fixed/decline")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> DeclineNotFixed(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        cases.DeclineNotFixedAsync(id, request, cancellationToken);

    [HttpPut("{id:long}/costs")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> UpdateCosts(long id, UpdateCostsRequest request, CancellationToken cancellationToken) =>
        cases.UpdateCostsAsync(id, request, cancellationToken);

    /// <summary>TEC-05: the result of one task.</summary>
    [HttpPost("{id:long}/tasks/{taskId:long}")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> UpdateTask(long id, long taskId, UpdateTaskRequest request, CancellationToken cancellationToken) =>
        cases.UpdateTaskAsync(id, taskId, request, cancellationToken);

    /// <summary>Staff: the task was done by an outside party (or back to the technician).</summary>
    [HttpPost("{id:long}/tasks/{taskId:long}/third-party")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> SetTaskThirdParty(
        long id, long taskId, SetTaskThirdPartyRequest request, CancellationToken cancellationToken) =>
        cases.SetTaskThirdPartyAsync(id, taskId, request, cancellationToken);

    // ----- F4 emergency -----

    /// <summary>F4 step ②: first response.</summary>
    [HttpPost("{id:long}/acknowledge")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Acknowledge(long id, CancellationToken cancellationToken) =>
        cases.AcknowledgeAsync(id, cancellationToken);

    /// <summary>Emergency log line (third party, temporary measure, customer update).</summary>
    [HttpPost("{id:long}/log")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> AddLog(long id, EmergencyLogRequest request, CancellationToken cancellationToken) =>
        cases.AddLogAsync(id, request, cancellationToken);

    /// <summary>K08: post-incident report of a closed emergency.</summary>
    [HttpPut("{id:long}/incident-report")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> WriteIncidentReport(long id, IncidentReportRequest request, CancellationToken cancellationToken) =>
        cases.WriteIncidentReportAsync(id, request, cancellationToken);

    // ----- F5 repair or replace -----

    /// <summary>Meeting 07/11: ask the staff for an outside party (description, reason, photos); waits on hold.</summary>
    [HttpPost("{id:long}/request-decision")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> RequestDecision(long id, RequestDecisionRequest request, CancellationToken cancellationToken) =>
        cases.RequestDecisionAsync(id, request, cancellationToken);

    /// <summary>Meeting 07/11: handed to an outside party (agreed outside the system).</summary>
    [HttpPost("{id:long}/outsource")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Outsource(long id, CancellationToken cancellationToken) =>
        cases.OutsourceAsync(id, cancellationToken);

    /// <summary>F5 way 2: survey of an asset that fails often.</summary>
    [HttpPost("surveys")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> OpenSurvey(OpenSurveyRequest request, CancellationToken cancellationToken) =>
        cases.OpenSurveyAsync(request, cancellationToken);
}
