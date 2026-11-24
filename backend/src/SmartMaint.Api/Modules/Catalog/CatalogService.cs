using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Catalog.Dtos;

namespace SmartMaint.Api.Modules.Catalog;

internal sealed class CatalogService(AppDbContext db, ICurrentUser currentUser) : ICatalogService
{
    public async Task<IReadOnlyList<ServiceTypeResponse>> GetServiceTypesAsync(CancellationToken cancellationToken) =>
        await db.CaseCategories.AsNoTracking()
            .Where(c => c.ArchivedAt == null)
            .OrderBy(c => c.Name)
            .Select(c => new ServiceTypeResponse(c.Id, c.GroupName, c.Name, c.AssetTypeId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<BuildingResponse>> GetBuildingsAsync(CancellationToken cancellationToken)
    {
        // Deleted places (meeting note 18/11, soft delete) are no longer offered.
        var buildings = await db.Buildings.AsNoTracking()
            .Where(b => b.DeletedAt == null)
            .Include(b => b.Floors.Where(f => f.DeletedAt == null)).ThenInclude(f => f.Rooms.Where(r => r.DeletedAt == null))
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

        return buildings
            .Select(b => new BuildingResponse(
                b.Id,
                b.Name,
                b.Floors
                    .OrderBy(f => f.Id)
                    .Select(f => new FloorResponse(
                        f.Id,
                        f.DisplayName,
                        f.Rooms.OrderBy(r => r.Id).Select(r => new RoomResponse(r.Id, r.RoomNumber)).ToList()))
                    .ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<AssetOptionResponse>> GetAssetsAsync(long roomId, CancellationToken cancellationToken) =>
        await db.Assets.AsNoTracking()
            .Where(a => a.RoomId == roomId && a.RetiredAt == null)
            .OrderBy(a => a.Name)
            .Select(a => new AssetOptionResponse(a.Id, a.Name, a.RoomId, a.TypeId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AssetTypeOptionResponse>> GetAssetTypesAsync(CancellationToken cancellationToken) =>
        await db.AssetTypes.AsNoTracking()
            .OrderBy(t => t.Category.CategoryName).ThenBy(t => t.Name)
            .Select(t => new AssetTypeOptionResponse(t.Id, t.Name, t.CategoryId, t.Category.CategoryName))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ManufacturerOptionResponse>> GetManufacturersAsync(CancellationToken cancellationToken) =>
        await db.Manufacturers.AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerOptionResponse(m.Id, m.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SpecialtyOptionResponse>> GetSpecialtiesAsync(CancellationToken cancellationToken) =>
        await db.Specialties.AsNoTracking()
            .Where(s => s.ArchivedAt == null)
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtyOptionResponse(s.Id, s.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TechnicianOptionResponse>> GetTechniciansAsync(
        CancellationToken cancellationToken, long? caseId = null) =>
        await TechnicianOptions.LoadAsync(db, currentUser.Id, caseId, cancellationToken);
}
