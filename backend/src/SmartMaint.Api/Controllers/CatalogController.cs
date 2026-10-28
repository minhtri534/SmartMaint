using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Users;
using SmartMaint.Api.Modules.Catalog;
using SmartMaint.Api.Modules.Catalog.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>Reference data for the case forms (service types, buildings, assets, technicians).</summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Customer, UserRole.Staff, UserRole.Technician, UserRole.Manager)]
public sealed class CatalogController(ICatalogService catalog) : ControllerBase
{
    [HttpGet("service-types")]
    public Task<IReadOnlyList<ServiceTypeResponse>> ServiceTypes(CancellationToken cancellationToken) =>
        catalog.GetServiceTypesAsync(cancellationToken);

    /// <summary>Buildings with their floors and rooms.</summary>
    [HttpGet("buildings")]
    public Task<IReadOnlyList<BuildingResponse>> Buildings(CancellationToken cancellationToken) =>
        catalog.GetBuildingsAsync(cancellationToken);

    [HttpGet("assets")]
    public Task<IReadOnlyList<AssetOptionResponse>> Assets([FromQuery] long roomId, CancellationToken cancellationToken) =>
        catalog.GetAssetsAsync(roomId, cancellationToken);

    [HttpGet("technicians")]
    [AuthorizeRoles(UserRole.Staff)]
    public Task<IReadOnlyList<TechnicianOptionResponse>> Technicians(CancellationToken cancellationToken) =>
        catalog.GetTechniciansAsync(cancellationToken);
}
