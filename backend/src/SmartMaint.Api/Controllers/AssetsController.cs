using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Assets.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// F1 assets (AST-02, AST-03, AST-07). Staff / Manager read the assets of their buildings, the Administrator all. Since
/// the meeting note of 18/11 nobody changes them here: a manager approves a staff change list (AssetChangesController). The QR target is open to every role (roles are set per action: class + action attributes
/// would both have to pass). (GET /api/assets?roomId= of the report form is in
/// CatalogController.)
/// </summary>
[ApiController]
[Route("api/asset-register")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class AssetsController(IAssetService assets) : ControllerBase
{
    /// <summary>AST-02.</summary>
    [HttpGet]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager, UserRole.Admin)]
    public Task<PagedResult<AssetSummaryResponse>> List([FromQuery] AssetListQuery query, CancellationToken cancellationToken) =>
        assets.ListAsync(query, cancellationToken);

    /// <summary>AST-03.</summary>
    [HttpGet("{id:long}")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager, UserRole.Admin)]
    public Task<AssetDetailResponse> Get(long id, CancellationToken cancellationToken) =>
        assets.GetAsync(id, cancellationToken);

    /// <summary>AST-07: every signed-in role.</summary>
    [HttpGet("{id:long}/qr-target")]
    [AuthorizeRoles(UserRole.Customer, UserRole.Staff, UserRole.Technician, UserRole.Manager, UserRole.Admin)]
    public Task<QrTargetResponse> QrTarget(long id, CancellationToken cancellationToken) =>
        assets.GetQrTargetAsync(id, cancellationToken);
}
