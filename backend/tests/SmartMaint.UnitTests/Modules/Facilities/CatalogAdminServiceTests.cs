using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.CatalogAdmin;
using SmartMaint.Api.Modules.CatalogAdmin.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Facilities;

/// <summary>ADM-06 lists.</summary>
public sealed class CatalogAdminServiceTests : F1TestBase
{
    private CatalogAdminService Sut => new(Db, Audit, TimeProvider.System);

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
            new CaseCategoryRequest("Leak", null, null), default);

        var renamed = await Sut.SaveCaseCategoryAsync(created.Id,
            new CaseCategoryRequest("Leaking pipe", "Any leak", null), default);

        Assert.Equal("Leaking pipe", renamed.Name);
        Assert.Single(Db.CaseCategories);
        Assert.Equal(2, Db.AuditLogs.Count());
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SaveCaseCategory_LinkedToAnAssetType_ShowsThatType()
    {
        var type = Db.AssetTypes.First();

        var saved = await Sut.SaveCaseCategoryAsync(null, new CaseCategoryRequest("Not cooling", null, type.Id), default);

        Assert.Equal(type.Id, saved.AssetTypeId);
        Assert.Equal(type.Name, saved.AssetTypeName);
        Assert.Equal(type.Name, saved.GroupName);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SaveCaseCategory_UnknownAssetType_Throws() =>
        Assert.Equal("catalog.unknown_asset_type", await ErrorCodeAsync(() =>
            Sut.SaveCaseCategoryAsync(null, new CaseCategoryRequest("Not cooling", null, 9999), default)));

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ArchiveCaseCategory_ThenRestore_ChangesOnlyTheArchiveDate()
    {
        var saved = await Sut.SaveCaseCategoryAsync(null, new CaseCategoryRequest("Leak", null, null), default);

        var archived = await Sut.SetCaseCategoryArchivedAsync(saved.Id, true, default);
        var restored = await Sut.SetCaseCategoryArchivedAsync(saved.Id, false, default);

        Assert.NotNull(archived.ArchivedAt);
        Assert.Null(restored.ArchivedAt);
        Assert.Equal("Leak", restored.Name);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task ArchiveSpecialty_Twice_WritesOneAuditRow()
    {
        var saved = await Sut.SaveSpecialtyAsync(null, new SpecialtyRequest("Glazing"), default);
        var before = Db.AuditLogs.Count();

        await Sut.SetSpecialtyArchivedAsync(saved.Id, true, default);
        await Sut.SetSpecialtyArchivedAsync(saved.Id, true, default);

        Assert.Equal(before + 1, Db.AuditLogs.Count());
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task ArchiveCaseCategory_Unknown_Throws() =>
        Assert.Equal("catalog.not_found", await ErrorCodeAsync(() => Sut.SetCaseCategoryArchivedAsync(9999, true, default)));

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
