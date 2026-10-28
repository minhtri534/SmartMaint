namespace SmartMaint.Api.Entities.Buildings;

/// <summary>
/// Building -> Floor -> Room -> Asset. Minimal version for F3 (F1 owns the full catalogue;
/// ERD v5: Buildings, Floors, Rooms, Assets).
/// </summary>
public class Building : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public List<Floor> Floors { get; set; } = [];
}

public class Floor : AuditableEntity
{
    public long BuildingId { get; set; }
    public Building Building { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public List<Room> Rooms { get; set; } = [];
}

public class Room : AuditableEntity
{
    public long FloorId { get; set; }
    public Floor Floor { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
}

/// <summary>A piece of equipment in a room. Type, manufacturer and warranty come with F1.</summary>
public class Asset : AuditableEntity
{
    public long RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
}
