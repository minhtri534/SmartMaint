using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Facilities.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Facilities;

/// <summary>AST-01: the tree and its create / edit / delete.</summary>
public sealed class FacilityServiceTests : F1TestBase
{
    private FacilityService Sut => new(Db, Scope, Audit, Clock);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task GetTree_Staff_SeesOnlyTheirBuildingWithAssetCounts()
    {
        As(Staff);

        var tree = await Sut.GetTreeAsync(default);

        var tower = Assert.Single(tree);
        Assert.Equal("Tower A", tower.Name);
        var room = Assert.Single(tower.Floors).Rooms.Single(r => r.RoomNumber == "501");
        Assert.Equal(1, room.AssetCount);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task CreateFloorAndRoom_Admin_AddsThemAndAudits()
    {
        var floor = await Sut.CreateFloorAsync(new FloorRequest(TowerA.Id, "12"), default);
        var room = await Sut.CreateRoomAsync(new RoomRequest(floor.Id, "1201"), default);

        Assert.Equal("Floor 12", floor.Name);
        Assert.Equal("1201", room.RoomNumber);
        Assert.Equal(2, Db.AuditLogs.Count(a => a.Operation == AuditOperation.Create));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task CreateRoom_SameNumberOnTheSameFloorInOtherCase_Throws() =>
        Assert.Equal("facility.room_taken",
            await ErrorCodeAsync(() => Sut.CreateRoomAsync(new RoomRequest(Floor5.Id, "corridor 5"), default)));

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task DeleteRoom_WithAnAsset_Throws() =>
        Assert.Equal("facility.not_empty", await ErrorCodeAsync(() => Sut.DeleteRoomAsync(Room501.Id, default)));

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task DeleteRoom_AnOlderCustomerStillPointsAtIt_ClearsTheLink()
    {
        var room = await Sut.CreateRoomAsync(new RoomRequest(Floor5.Id, "599"), default);
        var customer = new SmartMaint.Api.Entities.Accounts.Customer("old.tenant", "Old Tenant", "hash", false, room.Id);
        Db.Customers.Add(customer);
        await Db.SaveChangesAsync();

        await Sut.DeleteRoomAsync(room.Id, default);

        Assert.Null(Db.Customers.Single(c => c.Id == customer.Id).RoomId);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task DeleteFloor_WhenItsLastRoomIsGone_Succeeds()
    {
        var floor = await Sut.CreateFloorAsync(new FloorRequest(OfficeB.Id, "4"), default);
        var room = await Sut.CreateRoomAsync(new RoomRequest(floor.Id, "401"), default);
        Assert.Equal("facility.not_empty", await ErrorCodeAsync(() => Sut.DeleteFloorAsync(floor.Id, default)));

        await Sut.DeleteRoomAsync(room.Id, default);
        await Sut.DeleteFloorAsync(floor.Id, default);

        Assert.NotNull(Db.Floors.Single(f => f.Id == floor.Id).DeletedAt); // marked, not removed (meeting note 18/11)
        Assert.DoesNotContain((await Sut.GetTreeAsync(default)).SelectMany(b => b.Floors), f => f.Id == floor.Id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task GetTree_FloorsAreOrderedBasementGroundThenNumbers()
    {
        foreach (var number in new[] { "10", "G", "B1", "2" })
        {
            await Sut.CreateFloorAsync(new FloorRequest(OfficeB.Id, number), default);
        }

        var office = (await Sut.GetTreeAsync(default)).Single(b => b.Name == "Office B");

        Assert.Equal(["B1", "G", "2", "3", "10"], office.Floors.Select(f => f.FloorNumber));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task CreateBuilding_NameAlreadyUsed_Throws() =>
        Assert.Equal("facility.building_name_taken",
            await ErrorCodeAsync(() => Sut.CreateBuildingAsync(new BuildingRequest("TOWER A", null), default)));
}
