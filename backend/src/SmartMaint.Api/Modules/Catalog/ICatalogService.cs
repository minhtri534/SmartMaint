using SmartMaint.Api.Modules.Catalog.Dtos;

namespace SmartMaint.Api.Modules.Catalog;

/// <summary>
/// Read-only reference data for the F3 forms. F1 (Huy) owns the full catalogue and will replace this with real CRUD;
/// until then the customer sees every building (ERD review #11: customers are not tied to a rented area yet).
/// </summary>
public interface ICatalogService
{
    Task<IReadOnlyList<ServiceTypeResponse>> GetServiceTypesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<BuildingResponse>> GetBuildingsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetOptionResponse>> GetAssetsAsync(long roomId, CancellationToken cancellationToken);

    /// <summary>For the "assign technician" dialog (staff only), least busy first.</summary>
    Task<IReadOnlyList<TechnicianOptionResponse>> GetTechniciansAsync(CancellationToken cancellationToken);
}
