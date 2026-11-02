using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>
/// Shared world for the Cases tests. Building A (staff: Staff, manager: Manager) has floor 5 with room 501 (Customer),
/// room 502 (Customer2) and a common corridor; building B (staff: StaffB) has room 301 (CustomerB).
/// Priorities with their SLA hours, three service types, users of every role.
/// Tests switch the caller with <see cref="As"/> and drive cases through the real service.
/// </summary>
public abstract class CaseTestBase : IDisposable
{
    protected readonly FixedTimeProvider Clock = new(new DateTimeOffset(2026, 10, 28, 8, 0, 0, TimeSpan.Zero));
    protected readonly FakeCurrentUser Me = new();
    protected readonly FakeFileStorage Storage = new();
    protected readonly AppDbContext Db;
    internal readonly CaseService Sut;

    protected readonly Customer Customer, Customer2, CustomerB;
    protected readonly Staff Staff, StaffB;
    protected readonly Technician Technician, Technician2;
    protected readonly Manager Manager;
    protected readonly Admin Admin;
    protected readonly Room RoomA, RoomA2, CorridorA, RoomB;
    protected readonly Asset AcInRoomA, AssetInRoomB;
    protected readonly CaseCategory NotCooling, Leak, BurstPipe;

    protected CaseTestBase()
    {
        Db = TestDbContextFactory.Create(Clock);

        var buildingA = new Building { Name = "Tower A" };
        var buildingB = new Building { Name = "Office B" };
        var floorA = new Floor { Building = buildingA, FloorNumber = "5" };
        var floorB = new Floor { Building = buildingB, FloorNumber = "3" };
        RoomA = new Room { Floor = floorA, RoomNumber = "501" };
        RoomA2 = new Room { Floor = floorA, RoomNumber = "502" };
        CorridorA = new Room { Floor = floorA, RoomNumber = "Corridor 5", IsCommonArea = true };
        RoomB = new Room { Floor = floorB, RoomNumber = "301" };
        var acType = new AssetType { Category = new AssetCategory { Code = "HVAC", CategoryName = "HVAC" }, Code = "AC", Name = "Split AC" };
        AcInRoomA = new Asset { Room = RoomA, Type = acType, Name = "AC 501" };
        AssetInRoomB = new Asset { Room = RoomB, Type = acType, Name = "AC 301" };
        Db.AddRange(buildingA, buildingB, floorA, floorB, RoomA, RoomA2, CorridorA, RoomB, AcInRoomA, AssetInRoomB);
        Db.SaveChanges();

        Customer = new Customer("customer", "customer", "hash", false, RoomA.Id);
        Customer2 = new Customer("customer2", "customer2", "hash", false, RoomA2.Id);
        CustomerB = new Customer("customerb", "customerb", "hash", false, RoomB.Id);
        Staff = new Staff("staff", "staff", "hash", false);
        StaffB = new Staff("staffb", "staffb", "hash", false);
        Technician = new Technician("tech", "tech", "hash", false);
        Technician2 = new Technician("tech2", "tech2", "hash", false);
        Manager = new Manager("manager", "manager", "hash", false);
        Admin = new Admin("admin", "admin", "hash", false);
        Db.Accounts.AddRange(Customer, Customer2, CustomerB, Staff, StaffB, Technician, Technician2, Manager, Admin);

        var priorities = new[]
        {
            new PriorityLevel { Code = CasePriority.Emergency, Name = "Emergency", SlaHours = 2 },
            new PriorityLevel { Code = CasePriority.High, Name = "High", SlaHours = 8 },
            new PriorityLevel { Code = CasePriority.Normal, Name = "Normal", SlaHours = 24 },
            new PriorityLevel { Code = CasePriority.Low, Name = "Low", SlaHours = 72 },
        };
        Db.PriorityLevels.AddRange(priorities);
        NotCooling = Category("Air conditioning", "Not cooling", priorities[2], emergency: false, photo: false);
        Leak = Category("Water", "Leaking pipe or tap", priorities[2], emergency: false, photo: true);
        BurstPipe = Category("Water", "Burst pipe or flooding", priorities[0], emergency: true, photo: false);

        Db.SaveChanges();
        Db.StaffBuildingAssignments.AddRange(
            new StaffBuildingAssignment { StaffId = Staff.Id, BuildingId = buildingA.Id, IsPrimary = true },
            new StaffBuildingAssignment { StaffId = StaffB.Id, BuildingId = buildingB.Id, IsPrimary = true });
        Db.ManagerBuildingAssignments.Add(new ManagerBuildingAssignment { ManagerId = Manager.Id, BuildingId = buildingA.Id });
        Db.SaveChanges();

        Sut = new CaseService(Db, Me, Storage, Clock);
    }

    public void Dispose() => Db.Dispose();

    private CaseCategory Category(string group, string name, PriorityLevel priority, bool emergency, bool photo)
    {
        var category = new CaseCategory
        {
            GroupName = group,
            Name = name,
            DefaultPriority = priority,
            DefaultCostBearer = CostBearer.Building,
            IsEmergency = emergency,
            PhotoRequired = photo,
        };
        Db.CaseCategories.Add(category);
        return category;
    }

    /// <summary>Switch the caller.</summary>
    protected void As(Account user)
    {
        Me.Id = user.Id;
        Me.Role = user.Role;
    }

    protected static CreateCaseRequest NewRequest(CaseCategory category, Room room, long? assetId = null, IReadOnlyList<string>? photos = null) =>
        new("Air conditioner blows warm air", "The AC has been running since 8am but the room is still hot.", category.Id, room.Id, assetId, photos);

    /// <summary>A new case reported by the tenant of the room (Customer for building A, CustomerB for building B).</summary>
    protected async Task<CaseDetailResponse> NewCaseAsync(CaseCategory? category = null, Room? room = null)
    {
        room ??= RoomA;
        As(room == RoomB ? CustomerB : Customer);
        return await Sut.CreateAsync(NewRequest(category ?? NotCooling, room), default);
    }

    /// <summary>The smallest valid JPEG data URL the parser accepts.</summary>
    protected static readonly IReadOnlyList<string> Photo = ["data:image/jpeg;base64,/9j/4AAQ"];

    /// <summary>Drives a new case through the real service up to <paramref name="target"/> (main path only).</summary>
    protected async Task<long> CaseAtAsync(CaseStatus target)
    {
        var id = (await NewCaseAsync()).Id;
        if (target == CaseStatus.Pending) return id;

        As(Staff);
        await Sut.ApproveAsync(id, new ApproveCaseRequest(CasePriority.Normal, CostBearer.Building), default);
        if (target == CaseStatus.Approved) return id;

        await Sut.AssignAsync(id, new AssignCaseRequest(Technician.Id), default);
        if (target == CaseStatus.Assigned) return id;

        As(Technician);
        await Sut.AcceptAsync(id, default);
        if (target == CaseStatus.Accepted) return id;

        await Sut.StartAsync(id, default);
        if (target == CaseStatus.InProgress) return id;

        if (target == CaseStatus.OnHold)
        {
            await Sut.HoldAsync(id, new HoldCaseRequest(HoldReason.WaitingParts, DateOnly.FromDateTime(Clock.Now.UtcDateTime)), default);
            return id;
        }

        await Sut.CompleteAsync(id, new CompleteWorkRequest("Worn part", null, null, null, Photo), default);
        if (target == CaseStatus.Completed) return id;

        As(Staff);
        await Sut.CloseAsync(id, default);
        return id;
    }

    protected static async Task<string> ErrorCodeAsync(Func<Task> action) =>
        (await Assert.ThrowsAsync<SmartMaint.Api.Common.Errors.AppException>(action)).Code;
}
