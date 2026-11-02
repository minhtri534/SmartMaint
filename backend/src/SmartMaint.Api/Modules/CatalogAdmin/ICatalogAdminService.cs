using SmartMaint.Api.Modules.CatalogAdmin.Dtos;

namespace SmartMaint.Api.Modules.CatalogAdmin;

/// <summary>
/// ADM-06 (Administrator): keeps the lists the other screens choose from — service types (case categories, shared by
/// every building, A03), asset categories and types, manufacturers, technician specialties. An item still in use is
/// never deleted. Every change is in the audit log.
/// </summary>
public interface ICatalogAdminService
{
    Task<IReadOnlyList<CaseCategoryAdminResponse>> ListCaseCategoriesAsync(CancellationToken cancellationToken);
    Task<CaseCategoryAdminResponse> SaveCaseCategoryAsync(long? id, CaseCategoryRequest request, CancellationToken cancellationToken);
    Task DeleteCaseCategoryAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetCategoryResponse>> ListAssetCategoriesAsync(CancellationToken cancellationToken);
    Task<AssetCategoryResponse> SaveAssetCategoryAsync(long? id, AssetCategoryRequest request, CancellationToken cancellationToken);
    Task DeleteAssetCategoryAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetTypeResponse>> ListAssetTypesAsync(CancellationToken cancellationToken);
    Task<AssetTypeResponse> SaveAssetTypeAsync(long? id, AssetTypeRequest request, CancellationToken cancellationToken);
    Task DeleteAssetTypeAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ManufacturerResponse>> ListManufacturersAsync(CancellationToken cancellationToken);
    Task<ManufacturerResponse> SaveManufacturerAsync(long? id, ManufacturerRequest request, CancellationToken cancellationToken);
    Task DeleteManufacturerAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<SpecialtyResponse>> ListSpecialtiesAsync(CancellationToken cancellationToken);
    Task<SpecialtyResponse> SaveSpecialtyAsync(long? id, SpecialtyRequest request, CancellationToken cancellationToken);
    Task DeleteSpecialtyAsync(long id, CancellationToken cancellationToken);
}
