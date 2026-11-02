namespace SmartMaint.Api.Entities.Buildings;

// F1 (Huy): building structure and assets. Tables and columns follow the ERD; NOT IN ERD v5 columns are listed in
// docs/database/ERD_v5_Thay_Doi_Khi_Code.md.

/// <summary>ERD: Buildings(ID, Address, Name). Scope is leased office buildings only (E1).</summary>
public class Building : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public List<Floor> Floors { get; set; } = [];
}

/// <summary>ERD: Floors(ID, FloorNumber, Building).</summary>
public class Floor : AuditableEntity
{
    public long BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    /// <summary>Text, because floors are named "G", "B1", "5", "12A".</summary>
    public string FloorNumber { get; set; } = string.Empty;

    public List<Room> Rooms { get; set; } = [];

    public string DisplayName => $"Floor {FloorNumber}";
}

/// <summary>ERD: Rooms(ID, RoomNumber, Floor).</summary>
public class Room : AuditableEntity
{
    public long FloorId { get; set; }
    public Floor Floor { get; set; } = null!;

    /// <summary>"501" for a leased room, a short name for a common area ("Lobby", "Restroom G").</summary>
    public string RoomNumber { get; set; } = string.Empty;

    /// <summary>NOT IN ERD v5: lobby, corridor, restroom... Every customer of the building may report problems here.</summary>
    public bool IsCommonArea { get; set; }
}

/// <summary>ERD: AssetCategory(ID, Code, CategoryName, Description) — e.g. HVAC, Electrical.</summary>
public class AssetCategory : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>ERD: AssetType(ID, CategoryID, Code, Name, Description) — e.g. split air conditioner.</summary>
public class AssetType : AuditableEntity
{
    public long CategoryId { get; set; }
    public AssetCategory Category { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>ERD: Manufacturers(ID, Name, PhoneNumber, Email) — the maker only (E3).</summary>
public class Manufacturer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

/// <summary>ERD: Assets(ID, Name, RoomID, TypeID, ManufacturerID, HasCopyright, WarrantyEndDate).</summary>
public class Asset : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public long RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public long TypeId { get; set; }
    public AssetType Type { get; set; } = null!;

    /// <summary>Optional: the maker of an old asset may be unknown.</summary>
    public long? ManufacturerId { get; set; }
    public Manufacturer? Manufacturer { get; set; }

    /// <summary>Only the manufacturer may service it (E4).</summary>
    public bool HasCopyright { get; set; }

    public DateOnly? WarrantyEndDate { get; set; }

    /// <summary>
    /// NOT IN ERD v5 (BR-10, Leader 02/11): an asset with history is never deleted, only retired. A retired asset is
    /// hidden from the report form and the QR label stops working; its history stays.
    /// </summary>
    public DateTimeOffset? RetiredAt { get; set; }

    public bool IsRetired => RetiredAt is not null;
}
