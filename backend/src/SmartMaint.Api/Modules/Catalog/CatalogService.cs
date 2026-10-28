using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Users;
using SmartMaint.Api.Modules.Catalog.Dtos;

namespace SmartMaint.Api.Modules.Catalog;

internal sealed class CatalogService(AppDbContext db) : ICatalogService
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

        return buildings
            .Select(b => new BuildingResponse(
                b.Id,
                b.Name,
                b.Floors.OrderBy(f => f.Id)
                    .Select(f => new FloorResponse(f.Id, f.Name, f.Rooms.OrderBy(r => r.Id).Select(r => new RoomResponse(r.Id, r.Name)).ToList()))
                    .ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<AssetOptionResponse>> GetAssetsAsync(long roomId, CancellationToken cancellationToken) =>
        await db.Assets.AsNoTracking()
            .Where(a => a.RoomId == roomId)
            .OrderBy(a => a.Name)
            .Select(a => new AssetOptionResponse(a.Id, a.Name, a.RoomId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TechnicianOptionResponse>> GetTechniciansAsync(CancellationToken cancellationToken)
    {
        var technicians = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Technician && u.Status == UserStatus.Active)
            .Select(u => new { u.Id, u.FullName })
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
