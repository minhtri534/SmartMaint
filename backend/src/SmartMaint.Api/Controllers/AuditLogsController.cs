using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Audit.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>ADM-08: the audit log, read only (rows are never edited or deleted).</summary>
[ApiController]
[Route("api/audit-logs")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Admin)]
public sealed class AuditLogsController(IAuditService audit) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<AuditLogResponse>> List([FromQuery] AuditLogQuery query, CancellationToken cancellationToken) =>
        audit.ListAsync(query, cancellationToken);
}
