using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.CatalogAdmin;
using SmartMaint.Api.Modules.CatalogAdmin.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>ADM-06 Maintenance categories: the lists the other screens choose from. Administrator only.</summary>
[ApiController]
[Route("api/catalog")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class CatalogAdminController(ICatalogAdminService catalog) : ControllerBase
{
    // Service types (ERD: CaseCategories)
    [HttpGet("case-categories")]
    public Task<IReadOnlyList<CaseCategoryAdminResponse>> CaseCategories(CancellationToken ct) => catalog.ListCaseCategoriesAsync(ct);

    [HttpPost("case-categories")]
    public Task<CaseCategoryAdminResponse> CreateCaseCategory(CaseCategoryRequest request, CancellationToken ct) =>
        catalog.SaveCaseCategoryAsync(null, request, ct);

    [HttpPut("case-categories/{id:long}")]
    public Task<CaseCategoryAdminResponse> UpdateCaseCategory(long id, CaseCategoryRequest request, CancellationToken ct) =>
        catalog.SaveCaseCategoryAsync(id, request, ct);

    [HttpDelete("case-categories/{id:long}")]
    public Task DeleteCaseCategory(long id, CancellationToken ct) => catalog.DeleteCaseCategoryAsync(id, ct);

    // Asset categories (ERD: AssetCategory)
    [HttpGet("asset-categories")]
    public Task<IReadOnlyList<AssetCategoryResponse>> AssetCategories(CancellationToken ct) => catalog.ListAssetCategoriesAsync(ct);

    [HttpPost("asset-categories")]
    public Task<AssetCategoryResponse> CreateAssetCategory(AssetCategoryRequest request, CancellationToken ct) =>
        catalog.SaveAssetCategoryAsync(null, request, ct);

    [HttpPut("asset-categories/{id:long}")]
    public Task<AssetCategoryResponse> UpdateAssetCategory(long id, AssetCategoryRequest request, CancellationToken ct) =>
        catalog.SaveAssetCategoryAsync(id, request, ct);

    [HttpDelete("asset-categories/{id:long}")]
    public Task DeleteAssetCategory(long id, CancellationToken ct) => catalog.DeleteAssetCategoryAsync(id, ct);

    // Asset types (ERD: AssetType). Staff / Manager read them through CatalogController (GET api/asset-types).
    [HttpGet("asset-types")]
    public Task<IReadOnlyList<AssetTypeResponse>> AssetTypes(CancellationToken ct) => catalog.ListAssetTypesAsync(ct);

    [HttpPost("asset-types")]
    public Task<AssetTypeResponse> CreateAssetType(AssetTypeRequest request, CancellationToken ct) =>
        catalog.SaveAssetTypeAsync(null, request, ct);

    [HttpPut("asset-types/{id:long}")]
    public Task<AssetTypeResponse> UpdateAssetType(long id, AssetTypeRequest request, CancellationToken ct) =>
        catalog.SaveAssetTypeAsync(id, request, ct);

    [HttpDelete("asset-types/{id:long}")]
    public Task DeleteAssetType(long id, CancellationToken ct) => catalog.DeleteAssetTypeAsync(id, ct);

    // Manufacturers (ERD: Manufacturers)
    [HttpGet("manufacturers")]
    public Task<IReadOnlyList<ManufacturerResponse>> Manufacturers(CancellationToken ct) => catalog.ListManufacturersAsync(ct);

    [HttpPost("manufacturers")]
    public Task<ManufacturerResponse> CreateManufacturer(ManufacturerRequest request, CancellationToken ct) =>
        catalog.SaveManufacturerAsync(null, request, ct);

    [HttpPut("manufacturers/{id:long}")]
    public Task<ManufacturerResponse> UpdateManufacturer(long id, ManufacturerRequest request, CancellationToken ct) =>
        catalog.SaveManufacturerAsync(id, request, ct);

    [HttpDelete("manufacturers/{id:long}")]
    public Task DeleteManufacturer(long id, CancellationToken ct) => catalog.DeleteManufacturerAsync(id, ct);

    // Technician specialties (ERD: Specialties)
    [HttpGet("specialties")]
    public Task<IReadOnlyList<SpecialtyResponse>> Specialties(CancellationToken ct) => catalog.ListSpecialtiesAsync(ct);

    [HttpPost("specialties")]
    public Task<SpecialtyResponse> CreateSpecialty(SpecialtyRequest request, CancellationToken ct) =>
        catalog.SaveSpecialtyAsync(null, request, ct);

    [HttpPut("specialties/{id:long}")]
    public Task<SpecialtyResponse> UpdateSpecialty(long id, SpecialtyRequest request, CancellationToken ct) =>
        catalog.SaveSpecialtyAsync(id, request, ct);

    [HttpDelete("specialties/{id:long}")]
    public Task DeleteSpecialty(long id, CancellationToken ct) => catalog.DeleteSpecialtyAsync(id, ct);
}
