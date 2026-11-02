using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Assets.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// F1 assets (AST-02..05, AST-07). Staff / Manager read the assets of their buildings, the Administrator all and is the
/// only one who changes them. The QR target is open to every role (roles are set per action: class + action attributes
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

    /// <summary>AST-04.</summary>
    [HttpPost]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<AssetDetailResponse> Create(AssetRequest request, CancellationToken cancellationToken) =>
        assets.CreateAsync(request, cancellationToken);

    [HttpPut("{id:long}")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<AssetDetailResponse> Update(long id, AssetRequest request, CancellationToken cancellationToken) =>
        assets.UpdateAsync(id, request, cancellationToken);

    [HttpPost("{id:long}/retire")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<AssetDetailResponse> Retire(long id, CancellationToken cancellationToken) =>
        assets.RetireAsync(id, cancellationToken);

    [HttpDelete("{id:long}")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task Delete(long id, CancellationToken cancellationToken) =>
        assets.DeleteAsync(id, cancellationToken);

    /// <summary>AST-05.</summary>
    [HttpPost("import")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<ImportAssetsResponse> Import(ImportAssetsRequest request, CancellationToken cancellationToken) =>
        assets.ImportAsync(request, cancellationToken);

    /// <summary>AST-07: every signed-in role.</summary>
    [HttpGet("{id:long}/qr-target")]
    [AuthorizeRoles(UserRole.Customer, UserRole.Staff, UserRole.Technician, UserRole.Manager, UserRole.Admin)]
    public Task<QrTargetResponse> QrTarget(long id, CancellationToken cancellationToken) =>
        assets.GetQrTargetAsync(id, cancellationToken);
}
