using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.CatalogAdmin;
using SmartMaint.Api.Modules.CatalogAdmin.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Facilities;

/// <summary>ADM-06 lists.</summary>
public sealed class CatalogAdminServiceTests : F1TestBase
{
    private CatalogAdminService Sut => new(Db, Audit);

    public CatalogAdminServiceTests()
    {
        Db.PriorityLevels.AddRange(
            new PriorityLevel { Code = CasePriority.Normal, Name = "Normal", SlaHours = 24 },
            new PriorityLevel { Code = CasePriority.Emergency, Name = "Emergency", SlaHours = 2 });
        Db.SaveChanges();
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SaveCaseCategory_NewThenRename_KeepsOneRowWithTheNewName()
    {
        var created = await Sut.SaveCaseCategoryAsync(null,
            new CaseCategoryRequest("Water", "Leak", null, false, CasePriority.Normal, CostBearer.Building, true), default);

        var renamed = await Sut.SaveCaseCategoryAsync(created.Id,
            new CaseCategoryRequest("Water", "Leaking pipe", "Any leak", false, CasePriority.Normal, CostBearer.Building, true), default);

        Assert.Equal("Leaking pipe", renamed.Name);
        Assert.Single(Db.CaseCategories);
        Assert.Equal(2, Db.AuditLogs.Count());
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SaveAssetType_CodeAlreadyUsedInOtherCase_Throws() =>
        Assert.Equal("catalog.duplicate", await ErrorCodeAsync(() =>
            Sut.SaveAssetTypeAsync(null, new AssetTypeRequest(Hvac.Id, "ac-split", "Another AC", null), default)));

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task DeleteManufacturer_StillUsedByAnAsset_Throws() =>
        Assert.Equal("catalog.in_use", await ErrorCodeAsync(() => Sut.DeleteManufacturerAsync(Daikin.Id, default)));

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task DeleteManufacturer_NoLongerUsed_Succeeds()
    {
        Ac501.ManufacturerId = null;
        await Db.SaveChangesAsync();

        await Sut.DeleteManufacturerAsync(Daikin.Id, default);

        Assert.Empty(Db.Manufacturers);
        Assert.Equal(0, (await Sut.ListAssetTypesAsync(default)).Count(t => t.UsedBy == 0));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SaveSpecialty_New_IsListed()
    {
        await Sut.SaveSpecialtyAsync(null, new SpecialtyRequest("Elevator"), default);

        Assert.Equal("Elevator", Assert.Single(await Sut.ListSpecialtiesAsync(default)).Name);
    }
}
