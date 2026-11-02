using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Catalog;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

public sealed class CatalogServiceTests : CaseTestBase
{
    private CatalogService Catalog => new(Db, Me);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task GetServiceTypes_ReturnsGroupsWithTheirDefaults()
    {
        var types = await Catalog.GetServiceTypesAsync(default);

        Assert.Equal(3, types.Count);
        var burst = Assert.Single(types, t => t.Name == "Burst pipe or flooding");
        Assert.True(burst.IsEmergency);
        Assert.Equal(CasePriority.Emergency, burst.DefaultUrgency);
        Assert.True(Assert.Single(types, t => t.Name == "Leaking pipe or tap").PhotoRequired);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task GetBuildings_Staff_ReturnsEveryFloorAndRoom()
    {
        As(Staff);

        var buildings = await Catalog.GetBuildingsAsync(default);

        Assert.Equal(2, buildings.Count);
        var tower = Assert.Single(buildings, b => b.Name == "Tower A");
        var floor = Assert.Single(tower.Floors);
        Assert.Equal("Floor 5", floor.Name);
        Assert.Equal(["501", "502", "Corridor 5"], floor.Rooms.Select(r => r.Name));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task GetBuildings_Customer_ReturnsOnlyOwnRoomAndCommonAreasOfOwnBuilding()
    {
        As(Customer);

        var buildings = await Catalog.GetBuildingsAsync(default);

        var tower = Assert.Single(buildings);
        Assert.Equal("Tower A", tower.Name);
        Assert.Equal(["501", "Corridor 5"], Assert.Single(tower.Floors).Rooms.Select(r => r.Name));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task GetAssets_ReturnsOnlyTheAssetsOfTheRoom()
    {
        var assets = await Catalog.GetAssetsAsync(RoomA.Id, default);

        Assert.Equal("AC 501", Assert.Single(assets).Name);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task GetAssets_UnknownRoom_ReturnsEmptyList() =>
        Assert.Empty(await Catalog.GetAssetsAsync(9999, default));

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task GetTechnicians_CountsTheCasesEachOneHoldsAndListsLeastBusyFirst()
    {
        await CaseAtAsync(CaseStatus.InProgress); // held by Technician
        await CaseAtAsync(CaseStatus.Completed);  // finished work does not count as open

        var list = await Catalog.GetTechniciansAsync(default);

        Assert.Equal(2, list.Count);
        Assert.Equal(Technician2.Id, list[0].Id);
        Assert.Equal(0, list[0].OpenCases);
        Assert.Equal(1, list[1].OpenCases);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task GetTechnicians_DeactivatedTechnician_IsNotOffered()
    {
        Technician2.Status = UserStatus.Deactivated;
        await Db.SaveChangesAsync();

        var list = await Catalog.GetTechniciansAsync(default);

        Assert.Equal(Technician.Id, Assert.Single(list).Id);
    }
}
