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
    public async Task GetServiceTypes_ReturnsEveryActiveFault() =>
        Assert.Equal(3, (await Catalog.GetServiceTypesAsync(default)).Count);

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task GetServiceTypes_ArchivedFault_IsNotOffered()
    {
        Db.CaseCategories.Single(c => c.Name == "Burst pipe or flooding").ArchivedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync();

        var types = await Catalog.GetServiceTypesAsync(default);

        Assert.DoesNotContain(types, t => t.Name == "Burst pipe or flooding");
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task GetSpecialties_ArchivedSpecialty_IsNotOffered()
    {
        Db.Specialties.Add(new Specialty { Name = "Old trade", ArchivedAt = DateTimeOffset.UtcNow });
        await Db.SaveChangesAsync();

        Assert.DoesNotContain(await Catalog.GetSpecialtiesAsync(default), s => s.Name == "Old trade");
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
    public async Task GetBuildings_Customer_ReturnsEveryBuildingAndRoom()
    {
        As(Customer);

        var buildings = await Catalog.GetBuildingsAsync(default);

        Assert.Equal(2, buildings.Count);
        var tower = Assert.Single(buildings, b => b.Name == "Tower A");
        Assert.Equal(["501", "502", "Corridor 5"], Assert.Single(tower.Floors).Rooms.Select(r => r.Name));
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
    [Trait(TestType.Key, TestType.Normal)]
    public async Task GetTechnicians_ForACase_PutsTheOneWhoFitsFirstEvenWhenBusier()
    {
        var hvac = new Specialty { Name = "HVAC" };
        Db.Specialties.Add(hvac);
        await Db.SaveChangesAsync();
        Db.TechnicianSpecialties.Add(new TechnicianSpecialty { TechnicianId = Technician.Id, SpecialtyId = hvac.Id });
        await Db.SaveChangesAsync();
        await CaseAtAsync(CaseStatus.InProgress); // Technician is busy, Technician2 is free
        var job = (await NewCaseAsync()).Id;
        As(Staff);

        var list = await Catalog.GetTechniciansAsync(default, job);

        Assert.Equal(Technician.Id, list[0].Id);
        Assert.True(list[0].Fits);
        Assert.False(list[1].Fits);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData("HVAC", "air conditioning not cooling", true)]
    [InlineData("Plumbing", "water leaking pipe", true)]
    [InlineData("Electrical", "water leaking pipe", false)]
    [InlineData("General repair", "general door broken", false)]
    public void Fits_ComparesTheTradeWithTheWork(string specialty, string work, bool fits) =>
        Assert.Equal(fits, TechnicianOptions.Fits([specialty], work.Split(' ').ToHashSet()));

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
