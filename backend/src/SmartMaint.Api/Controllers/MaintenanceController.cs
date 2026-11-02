using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Maintenance;
using SmartMaint.Api.Modules.Maintenance.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// F2 preventive maintenance: checklist templates (Admin, Staff) and plans (Staff make, Manager approve, both read).
/// The work orders themselves are cases (CasesController: tasks, complete-checklist).
/// </summary>
[ApiController]
[Route("api/maintenance")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class MaintenanceController(IChecklistService checklists, IMaintenancePlanService plans) : ControllerBase
{
    // ----- checklist templates -----

    [HttpGet("checklists")]
    [AuthorizeRoles(UserRole.Admin, UserRole.Staff, UserRole.Manager)]
    public Task<IReadOnlyList<ChecklistResponse>> Checklists(CancellationToken ct) => checklists.ListAsync(ct);

    [HttpPost("checklists")]
    [AuthorizeRoles(UserRole.Admin, UserRole.Staff)]
    public Task<ChecklistResponse> CreateChecklist(ChecklistRequest request, CancellationToken ct) =>
        checklists.SaveAsync(null, request, ct);

    [HttpPut("checklists/{id:long}")]
    [AuthorizeRoles(UserRole.Admin, UserRole.Staff)]
    public Task<ChecklistResponse> UpdateChecklist(long id, ChecklistRequest request, CancellationToken ct) =>
        checklists.SaveAsync(id, request, ct);

    [HttpDelete("checklists/{id:long}")]
    [AuthorizeRoles(UserRole.Admin, UserRole.Staff)]
    public Task DeleteChecklist(long id, CancellationToken ct) => checklists.DeleteAsync(id, ct);

    // ----- plans (PM-01..03) -----

    [HttpGet("plans")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    public Task<IReadOnlyList<PlanSummaryResponse>> Plans(CancellationToken ct) => plans.ListAsync(ct);

    [HttpGet("plans/{id:long}")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    public Task<PlanDetailResponse> Plan(long id, CancellationToken ct) => plans.GetAsync(id, ct);

    [HttpPost("plans")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<PlanDetailResponse> CreatePlan(PlanRequest request, CancellationToken ct) => plans.CreateAsync(request, ct);

    [HttpPut("plans/{id:long}")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<PlanDetailResponse> UpdatePlan(long id, PlanRequest request, CancellationToken ct) =>
        plans.UpdateAsync(id, request, ct);

    [HttpPost("plans/{id:long}/approve")]
    [AuthorizeRoles(UserRole.Manager)]
    public Task<PlanDetailResponse> Approve(long id, CancellationToken ct) => plans.ApproveAsync(id, ct);

    [HttpPost("plans/{id:long}/reject")]
    [AuthorizeRoles(UserRole.Manager)]
    public Task<PlanDetailResponse> Reject(long id, RejectPlanRequest request, CancellationToken ct) =>
        plans.RejectAsync(id, request, ct);

    [HttpPost("plans/{id:long}/pause")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<PlanDetailResponse> Pause(long id, CancellationToken ct) => plans.PauseAsync(id, ct);

    [HttpPost("plans/{id:long}/resume")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<PlanDetailResponse> Resume(long id, CancellationToken ct) => plans.ResumeAsync(id, ct);

    [HttpPost("plans/{id:long}/end")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<PlanDetailResponse> End(long id, CancellationToken ct) => plans.EndAsync(id, ct);
}
