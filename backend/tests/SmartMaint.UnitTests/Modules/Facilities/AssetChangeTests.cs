using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.AssetChanges;
using SmartMaint.Api.Modules.AssetChanges.Dtos;
using SmartMaint.Api.Modules.AssetChanges.Validators;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Notifications;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Facilities;

/// <summary>Meeting note 18/11 (decision 7), tree version 24/11: staff change lists of the building tree, approved by a manager.</summary>
public sealed class AssetChangeTests : F1TestBase
{
    private readonly Manager _manager;

    public AssetChangeTests()
    {
        _manager = new Manager("manager", "Manager A", "hash", false);
        Db.Accounts.Add(_manager);
        Db.SaveChanges();
        Db.ManagerBuildingAssignments.Add(new ManagerBuildingAssignment { ManagerId = _manager.Id, BuildingId = TowerA.Id });
        Db.SaveChanges();
    }

    private AssetChangeService Sut => new(
        Db, Me, Scope, new AssetService(Db, Me, Scope, Audit, Clock), new FacilityService(Db, Scope, Audit, Clock),
        new NotificationService(Db, Me, Clock), Clock);

    private static ChangeItemRequest Line(AssetChangeKind kind, string key) =>
        new(kind, key, null, null, null, null, null, null, null, null, null, null, null);

    private static ChangeListRequest List(params ChangeItemRequest[] items) => new("Changes", null, items);

    /// <summary>A new room 503 on floor 5 and a new AC in it, plus the 501 AC's warranty moved.</summary>
    private ChangeListRequest FitOut() => List(
        Line(AssetChangeKind.AddRoom, "n1") with { FloorId = Floor5.Id, Name = "503" },
        Line(AssetChangeKind.AddAsset, "n2") with { ParentKey = "n1", Name = "AC 503", TypeId = SplitAc.Id, HasCopyright = false },
        Line(AssetChangeKind.EditAsset, "n3") with
        {
            AssetId = Ac501.Id, RoomId = Room501.Id, Name = "AC 501", TypeId = SplitAc.Id, ManufacturerId = Daikin.Id,
            WarrantyEndDate = new DateOnly(2028, 6, 30),
        });

    private async Task<long> SentAsync(ChangeListRequest request)
    {
        As(Staff);
        var list = await Sut.CreateAsync(request, default);
        await Sut.SubmitAsync(list.Id, default);
        As(_manager);
        return list.Id;
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Approve_FitOut_AppliesEveryLineAndTellsTheStaff()
    {
        var id = await SentAsync(FitOut());
        Assert.Contains(Db.Notifications, n => n.UserId == _manager.Id && n.Details.Contains("waits for your approval"));

        var approved = await Sut.ApproveAsync(id, default);

        Assert.Equal(AssetChangeListStatus.Approved, approved.Status);
        var room = Db.Rooms.Single(r => r.RoomNumber == "503");
        Assert.Equal(Floor5.Id, room.FloorId);
        Assert.Equal(room.Id, Db.Assets.Single(a => a.Name == "AC 503").RoomId);
        Assert.Equal(new DateOnly(2028, 6, 30), Db.Assets.Single(a => a.Id == Ac501.Id).WarrantyEndDate);
        Assert.Equal(room.Id, approved.Items.Single(i => i.Key == "n1").RoomId); // the new ids are kept on the lines
        Assert.Equal(["Tower A"], approved.BuildingNames);
        Assert.Contains(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("approved"));
        Assert.Empty(approved.AllowedActions);
        Assert.Empty(Db.AuditLogs); // the audit log keeps Administrator actions only (ERD); the list is the record
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Approve_NewBuildingWithAFloorARoomAndAnAsset_BuildsItAndAssignsThePeople()
    {
        var id = await SentAsync(List(
            Line(AssetChangeKind.AddBuilding, "n1") with { Name = "Tower C", Address = "1 Le Loi" },
            Line(AssetChangeKind.AddFloor, "n2") with { ParentKey = "n1", Name = "1" },
            Line(AssetChangeKind.AddRoom, "n3") with { ParentKey = "n2", Name = "101" },
            Line(AssetChangeKind.AddAsset, "n4") with { ParentKey = "n3", Name = "AC 101", TypeId = SplitAc.Id }));

        await Sut.ApproveAsync(id, default);

        var tower = Db.Buildings.Single(b => b.Name == "Tower C");
        Assert.Equal("101", Db.Assets.Single(a => a.Name == "AC 101").Room.RoomNumber);
        Assert.Contains(Db.StaffBuildingAssignments, a => a.BuildingId == tower.Id && a.StaffId == Staff.Id && a.IsPrimary);
        Assert.Contains(Db.ManagerBuildingAssignments, a => a.BuildingId == tower.Id && a.ManagerId == _manager.Id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Submit_RetireAnAssetWithAnOpenCase_NamesTheLine()
    {
        Db.Cases.Add(new Case { Title = "AC broken", AssetId = Ac501.Id, RoomId = Room501.Id, Status = CaseStatus.InProgress, SubmittedAt = Clock.Now });
        await Db.SaveChangesAsync();
        As(Staff);
        var list = await Sut.CreateAsync(List(Line(AssetChangeKind.RetireAsset, "n1") with { AssetId = Ac501.Id }), default);

        Assert.Contains("open case", Assert.Single(list.Items).Problem);
        Assert.Equal("asset_change.has_problems", await ErrorCodeAsync(() => Sut.SubmitAsync(list.Id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Check_AFloorInABuildingTheStaffDoesNotLookAfter_IsAProblem()
    {
        As(Staff);

        var list = await Sut.CreateAsync(List(Line(AssetChangeKind.AddFloor, "n1") with { BuildingId = OfficeB.Id, Name = "4" }), default);

        Assert.Contains("not one of yours", Assert.Single(list.Items).Problem);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Submit_EmptyDraft_Throws()
    {
        As(Staff);
        var list = await Sut.CreateAsync(List(), default);

        Assert.Equal("asset_change.empty", await ErrorCodeAsync(() => Sut.SubmitAsync(list.Id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Check_RoomDeletedOnlyAfterItsAssetMovesOut_IsFine()
    {
        As(Staff);
        // The AC leaves 501 for the corridor in the same list: 501 is then empty and can go.
        var list = await Sut.CreateAsync(List(
            Line(AssetChangeKind.DeleteRoom, "n1") with { RoomId = Room501.Id },
            Line(AssetChangeKind.EditAsset, "n2") with { AssetId = Ac501.Id, RoomId = Corridor.Id, Name = "AC 501", TypeId = SplitAc.Id }), default);
        var keepsTheAc = await Sut.UpdateAsync(list.Id, List(Line(AssetChangeKind.DeleteRoom, "n1") with { RoomId = Room501.Id }), default);

        Assert.All(list.Items, i => Assert.Null(i.Problem));
        Assert.Contains("still has assets", Assert.Single(keepsTheAc.Items).Problem);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Approve_DeleteARoom_OnlyMarksIt()
    {
        var id = await SentAsync(List(Line(AssetChangeKind.DeleteRoom, "n1") with { RoomId = Corridor.Id }));

        await Sut.ApproveAsync(id, default);

        Assert.NotNull(Db.Rooms.Single(r => r.Id == Corridor.Id).DeletedAt);
        As(Staff);
        var tree = await new FacilityService(Db, Scope, Audit, Clock).GetTreeAsync(default);
        Assert.DoesNotContain(tree.SelectMany(b => b.Floors).SelectMany(f => f.Rooms), r => r.Id == Corridor.Id);
        // a deleted room keeps its name: a new room cannot take it
        var again = await Sut.CreateAsync(List(Line(AssetChangeKind.AddRoom, "n1") with { FloorId = Floor5.Id, Name = "Corridor 5" }), default);
        Assert.Contains("already has a room", Assert.Single(again.Items).Problem);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Reject_Manager_StaffCanEditAndSendAgain()
    {
        var id = await SentAsync(FitOut());

        var rejected = await Sut.RejectAsync(id, new RejectChangeListRequest("Room 503 is called 5-03 on the plans."), default);

        Assert.Equal(AssetChangeListStatus.Rejected, rejected.Status);
        Assert.Empty(Db.Rooms.Where(r => r.RoomNumber == "503"));
        As(Staff);
        Assert.Contains(ChangeListAction.Submit, (await Sut.GetAsync(id, default)).AllowedActions);
        As(StaffB); // another staff member: does not see it
        Assert.Equal("asset_change.not_found", await ErrorCodeAsync(() => Sut.GetAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Check_TwoNewRoomsWithTheSameNumber_TheSecondHasAProblem()
    {
        As(Staff);

        var list = await Sut.CreateAsync(List(
            Line(AssetChangeKind.AddRoom, "n1") with { FloorId = Floor5.Id, Name = "503" },
            Line(AssetChangeKind.AddRoom, "n2") with { FloorId = Floor5.Id, Name = " 503 " }), default);

        Assert.Null(list.Items[0].Problem);
        Assert.Contains("already has a room", list.Items[1].Problem);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void Validator_AddAssetWithoutRoomOrTypeOrWithABadKey_IsInvalid()
    {
        var result = new ChangeListRequestValidator().Validate(List(Line(AssetChangeKind.AddAsset, "b1") with { Name = "AC" }));

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Choose the room of the asset.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Choose the asset type.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "A line key looks like n1.");
    }
}
