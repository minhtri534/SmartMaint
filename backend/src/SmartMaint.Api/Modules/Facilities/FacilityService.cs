using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Facilities.Dtos;

namespace SmartMaint.Api.Modules.Facilities;

internal sealed class FacilityService(AppDbContext db, IBuildingScope scope, IAuditService audit) : IFacilityService
{
    // ----- read -------------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<BuildingNodeResponse>> GetTreeAsync(CancellationToken cancellationToken)
    {
        var buildingIds = await scope.GetBuildingIdsAsync(cancellationToken);
        var buildings = await db.Buildings.AsNoTracking()
            .Where(b => buildingIds == null || buildingIds.Contains(b.Id))
            .Include(b => b.Floors).ThenInclude(f => f.Rooms)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

        var roomIds = buildings.SelectMany(b => b.Floors).SelectMany(f => f.Rooms).Select(r => r.Id).ToList();
        var assetCounts = await db.Assets.AsNoTracking()
            .Where(a => roomIds.Contains(a.RoomId) && a.RetiredAt == null)
            .GroupBy(a => a.RoomId)
            .Select(g => new { RoomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoomId, x => x.Count, cancellationToken);
        var tenants = (await db.Customers.AsNoTracking()
                .Where(c => roomIds.Contains(c.RoomId))
                .Select(c => new { c.RoomId, c.FullName })
                .ToListAsync(cancellationToken))
            .ToLookup(c => c.RoomId, c => c.FullName);

        return buildings.Select(b => ToNode(b, assetCounts, tenants)).ToList();
    }

    // ----- buildings --------------------------------------------------------------------------------------------

    public async Task<BuildingNodeResponse> CreateBuildingAsync(BuildingRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        await EnsureBuildingNameFreeAsync(name, null, cancellationToken);
        var building = new Building { Name = name, Address = Blank(request.Address) };
        db.Buildings.Add(building);
        await db.SaveChangesAsync(cancellationToken);

        audit.Record(AuditTarget.Building, AuditOperation.Create, null, Snapshot(building));
        await db.SaveChangesAsync(cancellationToken);
        return await NodeAsync(building.Id, cancellationToken);
    }

    public async Task<BuildingNodeResponse> UpdateBuildingAsync(long id, BuildingRequest request, CancellationToken cancellationToken)
    {
        var building = await db.Buildings.SingleOrDefaultAsync(b => b.Id == id, cancellationToken) ?? throw FacilityErrors.NotFound();
        var name = request.Name.Trim();
        await EnsureBuildingNameFreeAsync(name, id, cancellationToken);

        var before = Snapshot(building);
        building.Name = name;
        building.Address = Blank(request.Address);
        audit.Record(AuditTarget.Building, AuditOperation.Update, before, Snapshot(building));
        await db.SaveChangesAsync(cancellationToken);
        return await NodeAsync(id, cancellationToken);
    }

    public async Task DeleteBuildingAsync(long id, CancellationToken cancellationToken)
    {
        var building = await db.Buildings.SingleOrDefaultAsync(b => b.Id == id, cancellationToken) ?? throw FacilityErrors.NotFound();
        var inUse = await db.Floors.AnyAsync(f => f.BuildingId == id, cancellationToken)
                    || await db.StaffBuildingAssignments.AnyAsync(a => a.BuildingId == id, cancellationToken)
                    || await db.ManagerBuildingAssignments.AnyAsync(a => a.BuildingId == id, cancellationToken);
        if (inUse)
        {
            throw FacilityErrors.NotEmpty("building");
        }

        db.Buildings.Remove(building);
        audit.Record(AuditTarget.Building, AuditOperation.Delete, Snapshot(building), null);
        await db.SaveChangesAsync(cancellationToken);
    }

    // ----- floors -----------------------------------------------------------------------------------------------

    public async Task<FloorNodeResponse> CreateFloorAsync(FloorRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Buildings.AnyAsync(b => b.Id == request.BuildingId, cancellationToken))
        {
            throw FacilityErrors.NotFound();
        }

        var number = request.FloorNumber.Trim();
        await EnsureFloorFreeAsync(request.BuildingId, number, null, cancellationToken);
        var floor = new Floor { BuildingId = request.BuildingId, FloorNumber = number };
        db.Floors.Add(floor);
        await db.SaveChangesAsync(cancellationToken);

        audit.Record(AuditTarget.Floor, AuditOperation.Create, null, Snapshot(floor));
        await db.SaveChangesAsync(cancellationToken);
        return new FloorNodeResponse(floor.Id, floor.FloorNumber, floor.DisplayName, []);
    }

    public async Task<FloorNodeResponse> UpdateFloorAsync(long id, FloorRequest request, CancellationToken cancellationToken)
    {
        var floor = await db.Floors.SingleOrDefaultAsync(f => f.Id == id, cancellationToken) ?? throw FacilityErrors.NotFound();
        var number = request.FloorNumber.Trim();
        await EnsureFloorFreeAsync(floor.BuildingId, number, id, cancellationToken);

        var before = Snapshot(floor);
        floor.FloorNumber = number;
        audit.Record(AuditTarget.Floor, AuditOperation.Update, before, Snapshot(floor));
        await db.SaveChangesAsync(cancellationToken);
        return (await NodeAsync(floor.BuildingId, cancellationToken)).Floors.Single(f => f.Id == id);
    }

    public async Task DeleteFloorAsync(long id, CancellationToken cancellationToken)
    {
        var floor = await db.Floors.SingleOrDefaultAsync(f => f.Id == id, cancellationToken) ?? throw FacilityErrors.NotFound();
        if (await db.Rooms.AnyAsync(r => r.FloorId == id, cancellationToken))
        {
            throw FacilityErrors.NotEmpty("floor");
        }

        db.Floors.Remove(floor);
        audit.Record(AuditTarget.Floor, AuditOperation.Delete, Snapshot(floor), null);
        await db.SaveChangesAsync(cancellationToken);
    }

    // ----- rooms ------------------------------------------------------------------------------------------------

    public async Task<RoomNodeResponse> CreateRoomAsync(RoomRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Floors.AnyAsync(f => f.Id == request.FloorId, cancellationToken))
        {
            throw FacilityErrors.NotFound();
        }

        var number = request.RoomNumber.Trim();
        await EnsureRoomFreeAsync(request.FloorId, number, null, cancellationToken);
        var room = new Room { FloorId = request.FloorId, RoomNumber = number, IsCommonArea = request.IsCommonArea };
        db.Rooms.Add(room);
        await db.SaveChangesAsync(cancellationToken);

        audit.Record(AuditTarget.Room, AuditOperation.Create, null, Snapshot(room));
        await db.SaveChangesAsync(cancellationToken);
        return new RoomNodeResponse(room.Id, room.RoomNumber, room.IsCommonArea, 0, []);
    }

    public async Task<RoomNodeResponse> UpdateRoomAsync(long id, RoomRequest request, CancellationToken cancellationToken)
    {
        var room = await db.Rooms.Include(r => r.Floor).SingleOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw FacilityErrors.NotFound();
        var number = request.RoomNumber.Trim();
        await EnsureRoomFreeAsync(room.FloorId, number, id, cancellationToken);
        if (request.IsCommonArea && !room.IsCommonArea && await db.Customers.AnyAsync(c => c.RoomId == id, cancellationToken))
        {
            throw FacilityErrors.RoomRented();
        }

        var before = Snapshot(room);
        room.RoomNumber = number;
        room.IsCommonArea = request.IsCommonArea;
        audit.Record(AuditTarget.Room, AuditOperation.Update, before, Snapshot(room));
        await db.SaveChangesAsync(cancellationToken);
        return (await NodeAsync(room.Floor.BuildingId, cancellationToken))
            .Floors.SelectMany(f => f.Rooms).Single(r => r.Id == id);
    }

    public async Task DeleteRoomAsync(long id, CancellationToken cancellationToken)
    {
        var room = await db.Rooms.SingleOrDefaultAsync(r => r.Id == id, cancellationToken) ?? throw FacilityErrors.NotFound();
        var inUse = await db.Assets.AnyAsync(a => a.RoomId == id, cancellationToken)
                    || await db.Customers.AnyAsync(c => c.RoomId == id, cancellationToken)
                    || await db.Cases.AnyAsync(c => c.RoomId == id, cancellationToken);
        if (inUse)
        {
            throw FacilityErrors.NotEmpty("room");
        }

        db.Rooms.Remove(room);
        audit.Record(AuditTarget.Room, AuditOperation.Delete, Snapshot(room), null);
        await db.SaveChangesAsync(cancellationToken);
    }

    // ----- helpers ----------------------------------------------------------------------------------------------

    private async Task EnsureBuildingNameFreeAsync(string name, long? exceptId, CancellationToken cancellationToken)
    {
        if (await db.Buildings.AnyAsync(b => b.Name.ToLower() == name.ToLower() && b.Id != exceptId, cancellationToken))
        {
            throw FacilityErrors.BuildingNameTaken();
        }
    }

    private async Task EnsureFloorFreeAsync(long buildingId, string number, long? exceptId, CancellationToken cancellationToken)
    {
        if (await db.Floors.AnyAsync(
                f => f.BuildingId == buildingId && f.FloorNumber.ToLower() == number.ToLower() && f.Id != exceptId,
                cancellationToken))
        {
            throw FacilityErrors.FloorTaken();
        }
    }

    private async Task EnsureRoomFreeAsync(long floorId, string number, long? exceptId, CancellationToken cancellationToken)
    {
        if (await db.Rooms.AnyAsync(
                r => r.FloorId == floorId && r.RoomNumber.ToLower() == number.ToLower() && r.Id != exceptId,
                cancellationToken))
        {
            throw FacilityErrors.RoomTaken();
        }
    }

    private async Task<BuildingNodeResponse> NodeAsync(long buildingId, CancellationToken cancellationToken) =>
        (await GetTreeAsync(cancellationToken)).Single(b => b.Id == buildingId);

    private static BuildingNodeResponse ToNode(Building b, IReadOnlyDictionary<long, int> assetCounts, ILookup<long, string> tenants) =>
        new(
            b.Id,
            b.Name,
            b.Address,
            b.Floors
                .OrderBy(f => FloorOrder(f.FloorNumber)).ThenBy(f => f.FloorNumber)
                .Select(f => new FloorNodeResponse(
                    f.Id,
                    f.FloorNumber,
                    f.DisplayName,
                    f.Rooms
                        .OrderBy(r => r.IsCommonArea).ThenBy(r => r.RoomNumber)
                        .Select(r => new RoomNodeResponse(
                            r.Id, r.RoomNumber, r.IsCommonArea, assetCounts.GetValueOrDefault(r.Id), tenants[r.Id].Order().ToList()))
                        .ToList()))
                .ToList());

    /// <summary>Basements first, then ground, then numbered floors in number order ("B1", "G", "2", "10").</summary>
    private static int FloorOrder(string number) => number.ToUpperInvariant() switch
    {
        var n when n.StartsWith('B') && int.TryParse(n[1..], out var basement) => -basement,
        "G" => 0,
        var n when int.TryParse(n, out var level) => level,
        _ => 1000,
    };

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static object Snapshot(Building b) => new { b.Id, b.Name, b.Address };
    private static object Snapshot(Floor f) => new { f.Id, f.BuildingId, f.FloorNumber };
    private static object Snapshot(Room r) => new { r.Id, r.FloorId, r.RoomNumber, r.IsCommonArea };
}
