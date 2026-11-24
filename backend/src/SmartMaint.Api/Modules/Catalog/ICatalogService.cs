using SmartMaint.Api.Modules.Catalog.Dtos;

namespace SmartMaint.Api.Modules.Catalog;

/// <summary>
/// Read-only reference data for the F3 forms. Archived catalog items are left out; every role sees every room
/// (meeting note 18/11: customers may report anywhere).
/// </summary>
public interface ICatalogService
{
    Task<IReadOnlyList<ServiceTypeResponse>> GetServiceTypesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<BuildingResponse>> GetBuildingsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetOptionResponse>> GetAssetsAsync(long roomId, CancellationToken cancellationToken);

    /// <summary>
    /// For choosing a technician (staff only). With a case: the ones who fit it first, then the least busy, and that case
    /// does not count as one of their open jobs.
    /// </summary>
    Task<IReadOnlyList<TechnicianOptionResponse>> GetTechniciansAsync(CancellationToken cancellationToken, long? caseId = null);

    Task<IReadOnlyList<SpecialtyOptionResponse>> GetSpecialtiesAsync(CancellationToken cancellationToken);

    /// <summary>For the asset filters and the asset form.</summary>
    Task<IReadOnlyList<AssetTypeOptionResponse>> GetAssetTypesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ManufacturerOptionResponse>> GetManufacturersAsync(CancellationToken cancellationToken);
}
