using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Catalog.Dtos;

namespace SmartMaint.Api.Modules.Catalog;

internal sealed class CatalogService(AppDbContext db, ICurrentUser currentUser) : ICatalogService
{
    /// <summary>Statuses where a technician is still holding the case.</summary>
    private static readonly CaseStatus[] OpenStatuses =
    [
        CaseStatus.Assigned, CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold, CaseStatus.Reopened,
    ];

    public async Task<IReadOnlyList<ServiceTypeResponse>> GetServiceTypesAsync(CancellationToken cancellationToken) =>
        await db.CaseCategories.AsNoTracking()
            .OrderBy(c => c.GroupName).ThenBy(c => c.Name)
            .Select(c => new ServiceTypeResponse(
                c.Id, c.GroupName, c.Name, c.DefaultPriority.Code, c.IsEmergency, c.DefaultCostBearer, c.PhotoRequired))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<BuildingResponse>> GetBuildingsAsync(CancellationToken cancellationToken)
    {
        var buildings = await db.Buildings.AsNoTracking()
            .Include(b => b.Floors).ThenInclude(f => f.Rooms)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

        var isCustomer = currentUser.Role == UserRole.Customer;
        var home = isCustomer
            ? await db.Customers.AsNoTracking()
                .Where(c => c.Id == currentUser.Id)
                .Select(c => new { c.RoomId, c.Room.Floor.BuildingId })
                .SingleAsync(cancellationToken)
            : null;

        // A customer only sees where they may report: their own room + the common areas of their building.
        bool CanSee(Building b, Room r) =>
            home is null || r.Id == home.RoomId || (r.IsCommonArea && b.Id == home.BuildingId);

        return buildings
            .Select(b => new BuildingResponse(
                b.Id,
                b.Name,
                b.Floors
                    .OrderBy(f => f.Id)
                    .Select(f => new FloorResponse(
                        f.Id,
                        f.DisplayName,
                        f.Rooms.Where(r => CanSee(b, r)).OrderBy(r => r.Id)
                            .Select(r => new RoomResponse(r.Id, r.RoomNumber, r.IsCommonArea)).ToList()))
                    .Where(f => !isCustomer || f.Rooms.Count > 0)
                    .ToList()))
            .Where(b => !isCustomer || b.Floors.Count > 0)
            .ToList();
    }

    public async Task<IReadOnlyList<AssetOptionResponse>> GetAssetsAsync(long roomId, CancellationToken cancellationToken) =>
        await db.Assets.AsNoTracking()
            .Where(a => a.RoomId == roomId)
            .OrderBy(a => a.Name)
            .Select(a => new AssetOptionResponse(a.Id, a.Name, a.RoomId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SpecialtyOptionResponse>> GetSpecialtiesAsync(CancellationToken cancellationToken) =>
        await db.Specialties.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtyOptionResponse(s.Id, s.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TechnicianOptionResponse>> GetTechniciansAsync(CancellationToken cancellationToken)
    {
        var technicians = await db.Technicians.AsNoTracking()
            .Where(t => t.Status == UserStatus.Active)
            .Select(t => new { t.Id, t.FullName })
            .ToListAsync(cancellationToken);

        var open = await db.Cases.AsNoTracking()
            .Where(c => c.AssignedTechnicianId != null && OpenStatuses.Contains(c.Status))
            .GroupBy(c => c.AssignedTechnicianId!.Value)
            .Select(g => new { TechnicianId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return technicians
            .Select(t => new TechnicianOptionResponse(t.Id, t.FullName, open.FirstOrDefault(o => o.TechnicianId == t.Id)?.Count ?? 0))
            .OrderBy(t => t.OpenCases).ThenBy(t => t.FullName)
            .ToList();
    }
}
