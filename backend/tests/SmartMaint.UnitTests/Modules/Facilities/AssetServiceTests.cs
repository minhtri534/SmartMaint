using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Assets.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Facilities;

/// <summary>AST-02..05 and AST-07.</summary>
public sealed class AssetServiceTests : F1TestBase
{
    private AssetService Sut => new(Db, Me, Scope, Audit, Clock);

    private AssetRequest NewAc(string name = "AC 501 B") => new(name, Room501.Id, SplitAc.Id, Daikin.Id, false, new DateOnly(2027, 6, 30));

    private void AddCase(CaseStatus status) =>
        Db.Cases.Add(new Case { Title = "AC broken", AssetId = Ac501.Id, RoomId = Room501.Id, Status = status, SubmittedAt = Clock.Now });

    private const string Header = "building,floor,room,name,type_code,manufacturer,has_copyright,warranty_end_date\n";

    // ----- sort -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task List_SortedByNameDescending_PutsTheLastNameFirst()
    {
        await Sut.CreateAsync(NewAc("Zeta AC"), default);

        var page = await Sut.ListAsync(new AssetListQuery { Sort = "name_desc" }, default);

        Assert.Equal("Zeta AC", page.Items[0].Name);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task List_SortedByWarranty_PutsAssetsWithoutADateLast()
    {
        await Sut.CreateAsync(NewAc("With warranty"), default);

        var page = await Sut.ListAsync(new AssetListQuery { Sort = "warranty_desc" }, default);

        Assert.Equal("With warranty", page.Items[0].Name);
        Assert.Null(page.Items[^1].WarrantyEndDate);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task List_UnknownSort_KeepsTheOrderByPlace()
    {
        var page = await Sut.ListAsync(new AssetListQuery { Sort = "nonsense" }, default);

        Assert.NotEmpty(page.Items);
    }

    // ----- create / list / get -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Create_Admin_SavesTheAssetUnderWarrantyAndAudits()
    {
        var result = await Sut.CreateAsync(NewAc(), default);

        Assert.Equal("Split air conditioner", result.TypeName);
        Assert.Equal("Air conditioning", result.CategoryName);
        Assert.True(result.UnderWarranty);
        Assert.Empty(result.AllowedActions); // meeting note 18/11: changes go through a staff change list
        Assert.Single(Db.AuditLogs, a => a.TargetType == AuditTarget.Asset);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_SameNameInTheSameRoom_Throws() =>
        Assert.Equal("asset.name_taken", await ErrorCodeAsync(() => Sut.CreateAsync(NewAc("ac 501"), default)));

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task List_StaffOfAnotherBuilding_DoesNotSeeTheAsset()
    {
        As(StaffB);

        Assert.Equal(0, (await Sut.ListAsync(new AssetListQuery(), default)).TotalCount);
        Assert.Equal("asset.not_found", await ErrorCodeAsync(() => Sut.GetAsync(Ac501.Id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Get_Staff_SeesTheRepairHistoryButAdminOnlyTheCount()
    {
        AddCase(CaseStatus.Closed);
        await Db.SaveChangesAsync();

        As(Staff);
        var forStaff = await Sut.GetAsync(Ac501.Id, default);
        As(Admin);
        var forAdmin = await Sut.GetAsync(Ac501.Id, default);

        Assert.Equal("AC broken", Assert.Single(forStaff.History!).Title);
        Assert.Null(forAdmin.History); // BR-11
        Assert.Equal(1, forAdmin.CaseCount);
        Assert.Equal([AssetAction.ProposeChange], forStaff.AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task List_WarrantyEndingToday_IsStillUnderWarranty()
    {
        await Sut.CreateAsync(NewAc() with { WarrantyEndDate = new DateOnly(2026, 11, 2) }, default);

        var page = await Sut.ListAsync(new AssetListQuery { Search = "501 b" }, default);

        Assert.True(Assert.Single(page.Items).UnderWarranty);
    }

    // ----- retire / delete -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Retire_AssetWithOnlyClosedCases_HidesItFromTheDefaultList()
    {
        AddCase(CaseStatus.Closed);
        await Db.SaveChangesAsync();

        var result = await Sut.RetireAsync(Ac501.Id, default);

        Assert.Equal(Clock.Now, result.RetiredAt);
        Assert.Equal(0, (await Sut.ListAsync(new AssetListQuery(), default)).TotalCount);
        Assert.Equal(1, (await Sut.ListAsync(new AssetListQuery { IncludeRetired = true }, default)).TotalCount);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Retire_AssetWithAnOpenCase_Throws()
    {
        AddCase(CaseStatus.InProgress);
        await Db.SaveChangesAsync();

        Assert.Equal("asset.open_cases", await ErrorCodeAsync(() => Sut.RetireAsync(Ac501.Id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Delete_AssetWithHistory_Throws()
    {
        AddCase(CaseStatus.Closed);
        await Db.SaveChangesAsync();

        Assert.Equal("asset.has_history", await ErrorCodeAsync(() => Sut.DeleteAsync(Ac501.Id, default)));
    }

    // ----- QR target -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task QrTarget_TenantOfTheRoom_CanReport()
    {
        As(Tenant);

        var target = await Sut.GetQrTargetAsync(Ac501.Id, default);

        Assert.True(target.CanReport);
        Assert.False(target.CanOpenDetail);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task QrTarget_RetiredAsset_CannotBeReported()
    {
        await Sut.RetireAsync(Ac501.Id, default);
        As(Tenant);

        var target = await Sut.GetQrTargetAsync(Ac501.Id, default);

        Assert.True(target.IsRetired);
        Assert.False(target.CanReport);
    }
}
