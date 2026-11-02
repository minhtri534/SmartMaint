using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.PriorityLevels;
using SmartMaint.Api.Modules.PriorityLevels.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>Urgency levels with their SLA hours. Everyone may read; ADM-07: only the Administrator changes the hours.</summary>
[ApiController]
[Route("api/priority-levels")]
[Produces("application/json")]
public sealed class PriorityLevelsController(IPriorityLevelService priorityLevels) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<PriorityLevelResponse>> List(CancellationToken cancellationToken) =>
        priorityLevels.ListAsync(cancellationToken);

    [HttpPut("{id:long}/sla")]
    [AuthorizeRoles(UserRole.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<PriorityLevelResponse> UpdateSla(long id, UpdateSlaRequest request, CancellationToken cancellationToken) =>
        priorityLevels.UpdateSlaAsync(id, request, cancellationToken);
}
