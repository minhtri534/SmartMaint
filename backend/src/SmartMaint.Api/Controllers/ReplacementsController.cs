using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Assets.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// A06 / A07 asset replacement requests (F5 → F1). The Administrator registers the new asset and retires the old one;
/// staff and managers read the requests of their buildings. Roles are set per action.
/// </summary>
[ApiController]
[Route("api/asset-replacements")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class ReplacementsController(IReplacementService replacements) : ControllerBase
{
    [HttpGet]
    [AuthorizeRoles(UserRole.Admin, UserRole.Staff, UserRole.Manager)]
    public Task<IReadOnlyList<ReplacementRequestResponse>> List(CancellationToken cancellationToken) =>
        replacements.ListAsync(cancellationToken);

    /// <summary>Register the new asset in place of the old one (which is retired).</summary>
    [HttpPost("{id:long}/complete")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<ReplacementRequestResponse> Complete(long id, CompleteReplacementRequest request, CancellationToken cancellationToken) =>
        replacements.CompleteAsync(id, request, cancellationToken);

    [HttpPost("{id:long}/dismiss")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<ReplacementRequestResponse> Dismiss(long id, DismissReplacementRequest request, CancellationToken cancellationToken) =>
        replacements.DismissAsync(id, request, cancellationToken);
}
