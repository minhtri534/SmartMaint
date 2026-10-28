using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Users;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// F3 (normal incident). One endpoint per arrow of the state machine; the roles here are the coarse gate,
/// the service decides per case (data scope + status). Admin has no access to cases (BR-11).
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

    /// <summary>DLG-01.</summary>
    [HttpPost("{id:long}/approve")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Approve(long id, ApproveCaseRequest request, CancellationToken cancellationToken) =>
        cases.ApproveAsync(id, request, cancellationToken);

    /// <summary>DLG-02.</summary>
    [HttpPost("{id:long}/reject")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Reject(long id, RejectCaseRequest request, CancellationToken cancellationToken) =>
        cases.RejectAsync(id, request, cancellationToken);

    /// <summary>DLG-04.</summary>
    [HttpPost("{id:long}/assign")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Assign(long id, AssignCaseRequest request, CancellationToken cancellationToken) =>
        cases.AssignAsync(id, request, cancellationToken);

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

    /// <summary>DLG-10.</summary>
    [HttpPost("{id:long}/hold")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> Hold(long id, HoldCaseRequest request, CancellationToken cancellationToken) =>
        cases.HoldAsync(id, request, cancellationToken);

    /// <summary>The technician continues, or the staff member lets them continue after "beyond ability".</summary>
    [HttpPost("{id:long}/resume")]
    [AuthorizeRoles(UserRole.Technician, UserRole.Staff)]
    public Task<CaseDetailResponse> Resume(long id, CancellationToken cancellationToken) =>
        cases.ResumeAsync(id, cancellationToken);

    /// <summary>DLG-11: repair record + report completed.</summary>
    [HttpPost("{id:long}/complete")]
    [AuthorizeRoles(UserRole.Technician)]
    public Task<CaseDetailResponse> Complete(long id, CompleteWorkRequest request, CancellationToken cancellationToken) =>
        cases.CompleteAsync(id, request, cancellationToken);

    /// <summary>DLG-06.</summary>
    [HttpPost("{id:long}/close")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Close(long id, CancellationToken cancellationToken) =>
        cases.CloseAsync(id, cancellationToken);

    /// <summary>DLG-07.</summary>
    [HttpPost("{id:long}/reopen")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<CaseDetailResponse> Reopen(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        cases.ReopenAsync(id, request, cancellationToken);

    /// <summary>DLG-14.</summary>
    [HttpPost("{id:long}/rating")]
    [AuthorizeRoles(UserRole.Customer)]
    public Task<CaseDetailResponse> Rate(long id, RateCaseRequest request, CancellationToken cancellationToken) =>
        cases.RateAsync(id, request, cancellationToken);
}
