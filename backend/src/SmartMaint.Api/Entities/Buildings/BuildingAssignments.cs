using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Entities.Buildings;

/// <summary>
/// Which buildings a Staff member looks after (ERD: StaffBuildingAssignments, E7). A building has several staff;
/// IsPrimary marks the main one (E8). Composite key (StaffId, BuildingId).
/// </summary>
public class StaffBuildingAssignment
{
    public long StaffId { get; set; }
    public User Staff { get; set; } = null!;
    public long BuildingId { get; set; }
    public Building Building { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

/// <summary>Which buildings a Manager oversees (ERD: ManagerBuildingAssignments, E13). Composite key (ManagerId, BuildingId).</summary>
public class ManagerBuildingAssignment
{
    public long ManagerId { get; set; }
    public User Manager { get; set; } = null!;
    public long BuildingId { get; set; }
    public Building Building { get; set; } = null!;
}
