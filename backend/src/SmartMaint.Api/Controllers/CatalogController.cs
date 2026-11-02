using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Catalog;
using SmartMaint.Api.Modules.Catalog.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>Reference data for the forms (service types, buildings, assets, technicians, specialties).</summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Customer, UserRole.Staff, UserRole.Technician, UserRole.Manager, UserRole.Admin)]
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

    /// <summary>Asset types, for the asset filters and form (F1).</summary>
    [HttpGet("asset-types")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager, UserRole.Admin)]
    public Task<IReadOnlyList<AssetTypeOptionResponse>> AssetTypes(CancellationToken cancellationToken) =>
        catalog.GetAssetTypesAsync(cancellationToken);

    /// <summary>Technician specialties, for the account form (ADM-03).</summary>
    [HttpGet("specialties")]
    [AuthorizeRoles(UserRole.Admin, UserRole.Staff)]
    public Task<IReadOnlyList<SpecialtyOptionResponse>> Specialties(CancellationToken cancellationToken) =>
        catalog.GetSpecialtiesAsync(cancellationToken);
}
