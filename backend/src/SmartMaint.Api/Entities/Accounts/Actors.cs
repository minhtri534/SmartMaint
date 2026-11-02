using SmartMaint.Api.Entities.Buildings;

namespace SmartMaint.Api.Entities.Accounts;

// The 5 actor tables of the ERD. Each row has the same ID as its Account row (1-1).

/// <summary>ERD: Customers(ID). A tenant (D1: "Customer", old documents say "Employee").</summary>
public sealed class Customer : Account
{
    private Customer()
    {
    }

    public Customer(string username, string fullName, string passwordHash, bool mustChangePassword, long roomId)
        : base(username, fullName, UserRole.Customer, passwordHash, mustChangePassword) => RoomId = roomId;

    /// <summary>
    /// NOT IN ERD v5 (ERD review #11): the rented room. The customer can report problems in this room and in the
    /// common areas of the same building.
    /// </summary>
    public long RoomId { get; set; }
    public Room Room { get; set; } = null!;
}

/// <summary>ERD: Staff(ID). Facility staff (old name "Facility Officer"), looks after the buildings of StaffBuildingAssignments.</summary>
public sealed class Staff : Account
{
    private Staff()
    {
    }

    public Staff(string username, string fullName, string passwordHash, bool mustChangePassword)
        : base(username, fullName, UserRole.Staff, passwordHash, mustChangePassword)
    {
    }
}

/// <summary>ERD: Technicians(ID, HourlyRate).</summary>
public sealed class Technician : Account
{
    private Technician()
    {
    }

    public Technician(string username, string fullName, string passwordHash, bool mustChangePassword)
        : base(username, fullName, UserRole.Technician, passwordHash, mustChangePassword)
    {
    }

    /// <summary>Labour cost per hour (E9), used for case costs.</summary>
    public decimal? HourlyRate { get; set; }

    public List<TechnicianSpecialty> Specialties { get; set; } = [];
}

/// <summary>ERD: Manager(ID). Looks after the buildings of ManagerBuildingAssignments (E13).</summary>
public sealed class Manager : Account
{
    private Manager()
    {
    }

    public Manager(string username, string fullName, string passwordHash, bool mustChangePassword)
        : base(username, fullName, UserRole.Manager, passwordHash, mustChangePassword)
    {
    }
}

/// <summary>ERD: Admins(ID).</summary>
public sealed class Admin : Account
{
    private Admin()
    {
    }

    public Admin(string username, string fullName, string passwordHash, bool mustChangePassword)
        : base(username, fullName, UserRole.Admin, passwordHash, mustChangePassword)
    {
    }
}

/// <summary>ERD: Specialties(ID, Name) — electrical, plumbing, HVAC...</summary>
public sealed class Specialty : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>ERD: TechnicianSpecialty(Technician, Specialty). Composite key.</summary>
public sealed class TechnicianSpecialty
{
    public long TechnicianId { get; set; }
    public Technician Technician { get; set; } = null!;
    public long SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
}
