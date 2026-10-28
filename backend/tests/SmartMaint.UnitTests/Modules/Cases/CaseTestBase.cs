using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Users;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>
/// Shared world for the Cases tests: two buildings (A has Staff, B has StaffB), priorities with their SLA hours, three
/// service types, users of every role. Tests switch the caller with <see cref="As"/> and drive cases through the real service.
/// </summary>
public abstract class CaseTestBase : IDisposable
{
    protected readonly FixedTimeProvider Clock = new(new DateTimeOffset(2026, 10, 28, 8, 0, 0, TimeSpan.Zero));
    protected readonly FakeCurrentUser Me = new();
    protected readonly AppDbContext Db;
    internal readonly CaseService Sut;

    protected readonly User Customer, Customer2, Staff, StaffB, Technician, Technician2, Manager, Admin;
    protected readonly Room RoomA, RoomB;
    protected readonly Asset AcInRoomA, AssetInRoomB;
    protected readonly CaseCategory NotCooling, Leak, BurstPipe;

    protected CaseTestBase()
    {
        Db = TestDbContextFactory.Create(Clock);

        User NewUser(string name, UserRole role) => new($"{name}@test.local", name, role, "hash", false);
        Customer = NewUser("customer", UserRole.Customer);
        Customer2 = NewUser("customer2", UserRole.Customer);
        Staff = NewUser("staff", UserRole.Staff);
        StaffB = NewUser("staffb", UserRole.Staff);
        Technician = NewUser("tech", UserRole.Technician);
        Technician2 = NewUser("tech2", UserRole.Technician);
        Manager = NewUser("manager", UserRole.Manager);
        Admin = NewUser("admin", UserRole.Admin);
        Db.Users.AddRange(Customer, Customer2, Staff, StaffB, Technician, Technician2, Manager, Admin);

        var buildingA = new Building { Name = "Tower A" };
        var buildingB = new Building { Name = "Office B" };
        var floorA = new Floor { Building = buildingA, Name = "Floor 5" };
        var floorB = new Floor { Building = buildingB, Name = "Floor 3" };
        RoomA = new Room { Floor = floorA, Name = "Room 501" };
        RoomB = new Room { Floor = floorB, Name = "Room 301" };
        AcInRoomA = new Asset { Room = RoomA, Name = "AC 501" };
        AssetInRoomB = new Asset { Room = RoomB, Name = "AC 301" };
        Db.AddRange(buildingA, buildingB, floorA, floorB, RoomA, RoomB, AcInRoomA, AssetInRoomB);

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

        Sut = new CaseService(Db, Me, Clock);
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
    protected void As(User user)
    {
        Me.Id = user.Id;
        Me.Role = user.Role;
    }

    protected static CreateCaseRequest NewRequest(CaseCategory category, Room room, long? assetId = null, IReadOnlyList<string>? photos = null) =>
        new("Air conditioner blows warm air", "The AC has been running since 8am but the room is still hot.", category.Id, room.Id, assetId, photos);

    protected async Task<CaseDetailResponse> NewCaseAsync(CaseCategory? category = null, Room? room = null)
    {
        As(Customer);
        return await Sut.CreateAsync(NewRequest(category ?? NotCooling, room ?? RoomA), default);
    }

    protected static readonly IReadOnlyList<string> Photo = ["data:image/jpeg;base64,AAAA"];

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
