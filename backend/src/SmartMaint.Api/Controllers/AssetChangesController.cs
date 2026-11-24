using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.AssetChanges;
using SmartMaint.Api.Modules.AssetChanges.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// F1, meeting note 18/11 (tree version 24/11): change lists of buildings, floors, rooms and assets. Staff write them on
/// the building tree and send them, a manager of the buildings approves (applied at once) or rejects. The only way
/// buildings, floors, rooms and assets change.
/// </summary>
[ApiController]
[Route("api/asset-changes")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class AssetChangesController(IAssetChangeService changes) : ControllerBase
{
    [HttpGet]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    public Task<IReadOnlyList<ChangeListSummaryResponse>> List(CancellationToken ct) => changes.ListAsync(ct);

    [HttpGet("{id:long}")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    public Task<ChangeListDetailResponse> Get(long id, CancellationToken ct) => changes.GetAsync(id, ct);

    [HttpPost]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<ChangeListDetailResponse> Create(ChangeListRequest request, CancellationToken ct) => changes.CreateAsync(request, ct);

    [HttpPut("{id:long}")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<ChangeListDetailResponse> Update(long id, ChangeListRequest request, CancellationToken ct) =>
        changes.UpdateAsync(id, request, ct);

    [HttpDelete("{id:long}")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task Delete(long id, CancellationToken ct) => changes.DeleteAsync(id, ct);

    [HttpPost("{id:long}/submit")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<ChangeListDetailResponse> Submit(long id, CancellationToken ct) => changes.SubmitAsync(id, ct);

    [HttpPost("{id:long}/approve")]
    [AuthorizeRoles(UserRole.Manager)]
    public Task<ChangeListDetailResponse> Approve(long id, CancellationToken ct) => changes.ApproveAsync(id, ct);

    [HttpPost("{id:long}/reject")]
    [AuthorizeRoles(UserRole.Manager)]
    public Task<ChangeListDetailResponse> Reject(long id, RejectChangeListRequest request, CancellationToken ct) =>
        changes.RejectAsync(id, request, ct);
}
