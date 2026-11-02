using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Facilities;

/// <summary>
/// Shared world for the F1 tests: Tower A (floor 5: room 501 rented by a customer, a corridor; staff Staff) and
/// Office B (floor 3: room 301; staff StaffB). One AC type, one manufacturer, one asset in 501.
/// </summary>
public abstract class F1TestBase : IDisposable
{
    protected readonly FixedTimeProvider Clock = new(new DateTimeOffset(2026, 11, 2, 8, 0, 0, TimeSpan.Zero));
    protected readonly FakeCurrentUser Me = new();
    protected readonly AppDbContext Db;
    internal readonly AuditService Audit;
    internal readonly BuildingScope Scope;

    protected readonly Admin Admin;
    protected readonly Staff Staff, StaffB;
    protected readonly Customer Tenant;
    protected readonly Building TowerA, OfficeB;
    protected readonly Floor Floor5, Floor3;
    protected readonly Room Room501, Corridor, Room301;
    protected readonly AssetCategory Hvac;
    protected readonly AssetType SplitAc;
    protected readonly Manufacturer Daikin;
    protected readonly Asset Ac501;

    protected F1TestBase()
    {
        Db = TestDbContextFactory.Create(Clock);
        TowerA = new Building { Name = "Tower A" };
        OfficeB = new Building { Name = "Office B" };
        Floor5 = new Floor { Building = TowerA, FloorNumber = "5" };
        Floor3 = new Floor { Building = OfficeB, FloorNumber = "3" };
        Room501 = new Room { Floor = Floor5, RoomNumber = "501" };
        Corridor = new Room { Floor = Floor5, RoomNumber = "Corridor 5", IsCommonArea = true };
        Room301 = new Room { Floor = Floor3, RoomNumber = "301" };
        Hvac = new AssetCategory { Code = "HVAC", CategoryName = "Air conditioning" };
        SplitAc = new AssetType { Category = Hvac, Code = "AC-SPLIT", Name = "Split air conditioner" };
        Daikin = new Manufacturer { Name = "Daikin" };
        Ac501 = new Asset { Room = Room501, Type = SplitAc, Manufacturer = Daikin, Name = "AC 501" };
        Db.AddRange(TowerA, OfficeB, Floor5, Floor3, Room501, Corridor, Room301, Hvac, SplitAc, Daikin, Ac501);
        Db.SaveChanges();

        Admin = new Admin("root", "Root Admin", "hash", false);
        Staff = new Staff("staff", "Staff A", "hash", false);
        StaffB = new Staff("staffb", "Staff B", "hash", false);
        Tenant = new Customer("tenant", "Tenant 501", "hash", false, Room501.Id);
        Db.Accounts.AddRange(Admin, Staff, StaffB, Tenant);
        Db.SaveChanges();
        Db.StaffBuildingAssignments.AddRange(
            new StaffBuildingAssignment { StaffId = Staff.Id, BuildingId = TowerA.Id, IsPrimary = true },
            new StaffBuildingAssignment { StaffId = StaffB.Id, BuildingId = OfficeB.Id, IsPrimary = true });
        Db.SaveChanges();

        As(Admin);
        Audit = new AuditService(Db, Me, Clock);
        Scope = new BuildingScope(Db, Me);
    }

    public void Dispose() => Db.Dispose();

    protected void As(Account user)
    {
        Me.Id = user.Id;
        Me.Role = user.Role;
    }

    protected static async Task<string> ErrorCodeAsync(Func<Task> action) =>
        (await Assert.ThrowsAsync<SmartMaint.Api.Common.Errors.AppException>(action)).Code;
}
