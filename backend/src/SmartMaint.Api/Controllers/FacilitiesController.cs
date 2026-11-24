using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Facilities.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// F1 AST-01: the facility tree, read only. Staff / Manager read their buildings, the Administrator all. Buildings,
/// floors and rooms change through a staff change list a manager approves (meeting note 18/11, AssetChangesController).
/// </summary>
[ApiController]
[Route("api/facilities")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Staff, UserRole.Manager, UserRole.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class FacilitiesController(IFacilityService facilities) : ControllerBase
{
    [HttpGet("tree")]
    public Task<IReadOnlyList<BuildingNodeResponse>> Tree(CancellationToken cancellationToken) =>
        facilities.GetTreeAsync(cancellationToken);
}
