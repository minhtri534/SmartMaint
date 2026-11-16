using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>Create, list, get (the data scope, B3) and the review step: cancel, approve, reject.</summary>
public sealed class CaseIntakeTests : CaseTestBase
{
    // ----- Create -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Create_ValidRequest_SavesPendingCaseWithFirstHistoryRow()
    {
        var result = await NewCaseAsync();

        Assert.Equal(CaseStatus.Pending, result.Status);
        Assert.Equal($"CS-{result.Id:D6}", result.Code);
        Assert.Equal(CasePriority.Normal, result.Urgency);
        Assert.Equal("Air conditioning · Not cooling", result.ServiceTypeName);
        Assert.Equal(Customer.Id, result.Requester?.Id);
        var first = Assert.Single(result.History);
        Assert.Null(first.From);
        Assert.Equal(CaseStatus.Pending, first.To);
        Assert.Null(result.DueAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_ServiceTypeNeedsPhotoButHasNone_Throws()
    {
        As(Customer);

        var code = await ErrorCodeAsync(() => Sut.CreateAsync(NewRequest(Leak, RoomA), default));

        Assert.Equal("case.photo_required", code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Create_ServiceTypeNeedsPhotoAndHasExactlyOne_Succeeds()
    {
        As(Customer);

        var result = await Sut.CreateAsync(NewRequest(Leak, RoomA, photos: Photo), default);

        Assert.Single(result.Photos);
        Assert.Equal(CaseImageKind.Report, result.Photos[0].Kind);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_AssetFromAnotherRoom_Throws()
    {
        As(Customer);

        var code = await ErrorCodeAsync(() => Sut.CreateAsync(NewRequest(NotCooling, RoomA, AssetInRoomB.Id), default));

        Assert.Equal("case.asset_not_in_room", code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_UnknownServiceTypeOrRoom_Throws()
    {
        As(Customer);

        Assert.Equal("case.unknown_service_type",
            await ErrorCodeAsync(() => Sut.CreateAsync(NewRequest(NotCooling, RoomA) with { ServiceTypeId = 9999 }, default)));
        Assert.Equal("case.unknown_room",
            await ErrorCodeAsync(() => Sut.CreateAsync(NewRequest(NotCooling, RoomA) with { RoomId = 9999 }, default)));
    }

    // ----- Create: where a customer may report (own room + common areas of their building) -----

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_RoomOfAnotherTenant_Throws()
    {
        As(Customer);

        var code = await ErrorCodeAsync(() => Sut.CreateAsync(NewRequest(NotCooling, RoomA2), default));

        Assert.Equal("case.room_not_allowed", code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_RoomInAnotherBuilding_Throws()
    {
        As(Customer);

        var code = await ErrorCodeAsync(() => Sut.CreateAsync(NewRequest(NotCooling, RoomB), default));

        Assert.Equal("case.room_not_allowed", code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Create_CommonAreaOfOwnBuilding_Succeeds()
    {
        As(Customer2);

        var result = await Sut.CreateAsync(NewRequest(NotCooling, CorridorA), default);

        Assert.Equal(CorridorA.Id, result.Location.RoomId);
        Assert.Equal("Floor 5", result.Location.FloorName);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Create_EmergencyServiceType_SkipsPendingAndStartsTheClock()
    {
        var created = await NewCaseAsync(BurstPipe);

        Assert.True(created.IsEmergencyFlow);
        Assert.Equal(CaseStatus.Approved, created.Status);
        Assert.Equal(Clock.Now.AddHours(2), created.DueAt);
        Assert.Empty(created.AllowedActions); // the customer cannot cancel an emergency
        As(Staff);
        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.ApproveAsync(created.Id, Approval(CasePriority.Normal), default)));
    }

    // ----- Create: the urgency the customer chose (meeting 07/11) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Create_WithChosenUrgency_KeepsItInsteadOfTheTypeDefault()
    {
        As(Customer);

        var result = await Sut.CreateAsync(NewRequest(NotCooling, RoomA) with { Urgency = CasePriority.High }, default);

        Assert.Equal(CasePriority.High, result.Urgency);
        Assert.Equal(CaseStatus.Pending, result.Status);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Create_CustomerChoosesEmergency_StaysPendingForTheStaffToConfirm()
    {
        As(Customer);

        var result = await Sut.CreateAsync(NewRequest(NotCooling, RoomA) with { Urgency = CasePriority.Emergency }, default);

        Assert.Equal(CasePriority.Emergency, result.Urgency);
        Assert.Equal(CaseStatus.Pending, result.Status);
        Assert.False(result.IsEmergencyFlow);
        Assert.Null(result.DueAt);

        // The staff confirms it: only now does it go to F4.
        As(Staff);
        var approved = await Sut.ApproveAsync(result.Id, Approval(CasePriority.Emergency), default);
        Assert.True(approved.IsEmergencyFlow);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_EmergencyServiceTypeWithLowUrgency_IsStillAnEmergency()
    {
        As(Customer);

        var result = await Sut.CreateAsync(NewRequest(BurstPipe, RoomA) with { Urgency = CasePriority.Low }, default);

        Assert.True(result.IsEmergencyFlow);
        Assert.Equal(CasePriority.Emergency, result.Urgency);
    }

    // ----- List / Get: data scope -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task List_Customer_SeesOnlyOwnCases()
    {
        await NewCaseAsync();
        As(Customer2);
        await Sut.CreateAsync(NewRequest(NotCooling, RoomA2), default);

        As(Customer);
        var page = await Sut.ListAsync(new PageRequest(), default);

        Assert.Equal(1, page.TotalCount);
        Assert.All(page.Items, c => Assert.Equal(Customer.Id, c.Requester?.Id));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task List_StaffAndManager_SeeOnlyTheirBuildings()
    {
        await NewCaseAsync(room: RoomA);
        await NewCaseAsync(room: RoomB);

        As(Staff);
        Assert.Equal(1, (await Sut.ListAsync(new PageRequest(), default)).TotalCount);
        As(StaffB);
        Assert.Equal(RoomB.Id, Assert.Single((await Sut.ListAsync(new PageRequest(), default)).Items).Location.RoomId);
        As(Manager);
        var managerPage = await Sut.ListAsync(new PageRequest(), default);
        Assert.Equal(RoomA.Id, Assert.Single(managerPage.Items).Location.RoomId);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task List_Admin_SeesNothing()
    {
        await NewCaseAsync();
        As(Admin);

        Assert.Equal(0, (await Sut.ListAsync(new PageRequest(), default)).TotalCount);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Get_CaseOfAnotherCustomer_IsNotFound()
    {
        var id = (await NewCaseAsync()).Id;
        As(Customer2);

        Assert.Equal("case.not_found", await ErrorCodeAsync(() => Sut.GetAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task List_SecondPageOfOne_ReturnsTheOlderCase()
    {
        var first = (await NewCaseAsync()).Id;
        Clock.Now = Clock.Now.AddMinutes(1);
        var second = (await NewCaseAsync()).Id;
        As(Customer);

        var page1 = await Sut.ListAsync(new PageRequest { Page = 1, PageSize = 1 }, default);
        var page2 = await Sut.ListAsync(new PageRequest { Page = 2, PageSize = 1 }, default);

        Assert.Equal(2, page1.TotalCount);
        Assert.Equal(second, Assert.Single(page1.Items).Id);
        Assert.Equal(first, Assert.Single(page2.Items).Id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Get_Technician_SeesOnlyCasesAssignedToThem()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);

        As(Technician);
        Assert.Equal(id, (await Sut.GetAsync(id, default)).Id);
        As(Technician2);
        Assert.Equal("case.not_found", await ErrorCodeAsync(() => Sut.GetAsync(id, default)));
    }

    // ----- Cancel -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Cancel_CustomerPendingCase_CancelsWithReasonInHistory()
    {
        var id = (await NewCaseAsync()).Id;

        var result = await Sut.CancelAsync(id, new ReasonRequest("Fixed it myself, thanks"), default);

        Assert.Equal(CaseStatus.Cancelled, result.Status);
        Assert.Equal("Fixed it myself, thanks", result.History.Last().Reason);
        Assert.Equal(Customer.Id, result.History.Last().By.Id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Cancel_CustomerAfterApproval_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Customer);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.CancelAsync(id, new ReasonRequest("Changed my mind about this"), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Cancel_StaffWhileInProgress_IsStillAllowed()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);

        var result = await Sut.CancelAsync(id, new ReasonRequest("Outside the building's scope"), default);

        Assert.Equal(CaseStatus.Cancelled, result.Status);
    }

    // ----- Approve -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Approve_StaffOfTheBuilding_SetsUrgencyTechnicianTasksAndStaff()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);

        var result = await Sut.ApproveAsync(id, Approval(CasePriority.High), default);

        Assert.Equal(CaseStatus.Approved, result.Status);
        Assert.Equal(CasePriority.High, result.Urgency);
        Assert.Equal(Technician.Id, result.Technician?.Id); // no separate "assign" step (meeting 07/11)
        Assert.Equal("Air conditioner blows warm air", Assert.Single(result.Tasks).Name); // no task given: one named after the case
        Assert.Contains("Assigned to", result.History.Last().Reason);
        Assert.Equal(Staff.Id, result.Staff?.Id);
        Assert.Null(result.DueAt); // the SLA clock starts when work starts, not at approval
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Approve_StaffOfAnotherBuilding_IsNotFound()
    {
        var id = (await NewCaseAsync()).Id;
        As(StaffB);

        Assert.Equal("case.not_found",
            await ErrorCodeAsync(() => Sut.ApproveAsync(id, Approval(CasePriority.Normal), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Approve_AlreadyApprovedCase_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.ApproveAsync(id, Approval(CasePriority.Normal), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Approve_WithEmergencyUrgency_HandsTheCaseOverToF4()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);

        var result = await Sut.ApproveAsync(id, Approval(CasePriority.Emergency), default);

        Assert.True(result.IsEmergencyFlow);
        Assert.Equal(Clock.Now.AddHours(2), result.DueAt);
        Assert.NotNull(result.Emergency!.AcknowledgedAt); // the staff decided it, nobody to chase
        Assert.Contains("F4", result.History.Last().Reason);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Approve_WithAnotherCategory_CorrectsItAndWritesItInTheHistory()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);

        var result = await Sut.ApproveAsync(id, Approval(CasePriority.Normal) with { ServiceTypeId = Leak.Id }, default);

        Assert.Equal(Leak.Id, result.ServiceTypeId);
        Assert.Equal("Water · Leaking pipe or tap", result.ServiceTypeName);
        Assert.Contains("Category corrected from Air conditioning · Not cooling", result.History.Last().Reason);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Approve_WithUnknownCategory_Throws()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);

        Assert.Equal("case.unknown_service_type",
            await ErrorCodeAsync(() => Sut.ApproveAsync(id, Approval(CasePriority.Normal) with { ServiceTypeId = 9999 }, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Approve_WithTheSameCategory_WritesNoCorrection()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);

        var result = await Sut.ApproveAsync(id, Approval(CasePriority.Normal) with { ServiceTypeId = NotCooling.Id }, default);

        Assert.Equal(NotCooling.Id, result.ServiceTypeId);
        Assert.DoesNotContain("Category corrected", result.History.Last().Reason);
    }

    // ----- Reject -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Reject_Duplicate_RejectsWithLabelAndNote()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);

        var result = await Sut.RejectAsync(id, new RejectCaseRequest(RejectReason.Duplicate, "Same as CS-000001, already assigned"), default);

        Assert.Equal(CaseStatus.Rejected, result.Status);
        Assert.Equal("A case for this problem already exists: Same as CS-000001, already assigned", result.History.Last().Reason);
        Assert.Empty(result.AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Reject_ByCustomer_Throws()
    {
        var id = (await NewCaseAsync()).Id;
        As(Customer);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.RejectAsync(id, new RejectCaseRequest(RejectReason.MissingInfo, "Please add a photo here"), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Reject_AfterApproval_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.RejectAsync(id, new RejectCaseRequest(RejectReason.OutOfScope, "Tenant's own equipment"), default)));
    }
}
