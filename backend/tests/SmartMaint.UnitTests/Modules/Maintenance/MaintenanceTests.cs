using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Maintenance;
using SmartMaint.Api.Modules.Maintenance.Dtos;
using SmartMaint.UnitTests.Common;
using SmartMaint.UnitTests.Modules.Cases;

namespace SmartMaint.UnitTests.Modules.Maintenance;

/// <summary>F2: schedule, plans (approval E10), the generator job, checklist work on the generated work orders.</summary>
public sealed class MaintenanceTests : CaseTestBase
{
    private readonly CaseChecklist _acChecklist;

    public MaintenanceTests()
    {
        _acChecklist = new CaseChecklist
        {
            Name = "Split AC monthly service",
            CategoryId = NotCooling.Id,
            Tasks = [new ChecklistTask { Name = "Clean the filters" }, new ChecklistTask { Name = "Check the drain pipe" }],
        };
        Db.CaseChecklists.Add(_acChecklist);
        Db.SaveChanges();
    }

    private MaintenancePlanService Plans => new(Db, Me, new BuildingScope(Db, Me), Notifications, Clock);
    private MaintenanceGenerator Generator => new(Db, Notifications, Clock);
    private DateOnly Today => DateOnly.FromDateTime(Clock.Now.ToOffset(TimeSpan.FromHours(7)).DateTime);

    private PlanRequest NewPlan(params Asset[] assets) => new(
        1, FrequencyUnit.Month, 3, _acChecklist.Id, 1.5m, CasePriority.Normal,
        assets.Select(a => new PlanAssetRequest(a.Id, Technician.Id, Today.AddDays(2), null)).ToList());

    /// <summary>A plan made by Staff and approved by Manager.</summary>
    private async Task<long> ActivePlanAsync()
    {
        As(Staff);
        var plan = await Plans.CreateAsync(NewPlan(AcInRoomA), default);
        As(Manager);
        await Plans.ApproveAsync(plan.Id, default);
        return plan.Id;
    }

    // ----- schedule -----

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void Schedule_DueExactlyLeadDaysAhead_IsMadeAndOneDayEarlierIsNot()
    {
        var plan = new MaintenanceTrigger { Frequency = 1, FrequencyType = FrequencyUnit.Month, LeadDays = 3 };
        var asset = new MaintenanceWorkOrderAsset { StartTime = new DateOnly(2026, 11, 10) };

        Assert.Null(MaintenanceSchedule.DueToGenerate(asset, plan, new DateOnly(2026, 11, 6)));
        Assert.Equal(new DateOnly(2026, 11, 10), MaintenanceSchedule.DueToGenerate(asset, plan, new DateOnly(2026, 11, 7)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void Schedule_MissedPeriods_OnlyTheLatestIsMade()
    {
        var plan = new MaintenanceTrigger { Frequency = 1, FrequencyType = FrequencyUnit.Week, LeadDays = 0 };
        var asset = new MaintenanceWorkOrderAsset { StartTime = new DateOnly(2026, 10, 1) };

        Assert.Equal(new DateOnly(2026, 10, 29), MaintenanceSchedule.DueToGenerate(asset, plan, new DateOnly(2026, 11, 2)));
    }

    // ----- plans -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task CreatePlan_Staff_WaitsForTheBuildingManager()
    {
        As(Staff);

        var plan = await Plans.CreateAsync(NewPlan(AcInRoomA), default);

        Assert.Equal(TriggerStatus.PendingApproval, plan.Status);
        Assert.Equal(Today.AddDays(2), plan.NextDueDate);
        Assert.Contains(Db.Notifications, n => n.UserId == Manager.Id && n.Details.Contains("waits for your approval"));
        As(Manager);
        Assert.Equal([PlanAction.Approve, PlanAction.Reject], (await Plans.GetAsync(plan.Id, default)).AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task CreatePlan_AssetOfAnotherBuilding_Throws()
    {
        As(Staff);

        var error = await ErrorCodeAsync(() => Plans.CreateAsync(NewPlan(AcInRoomA, AssetInRoomB), default));

        Assert.Equal("maintenance.several_buildings", error);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task ApprovePlan_ByTheStaffWhoMadeIt_IsNotAllowed()
    {
        As(Staff);
        var plan = await Plans.CreateAsync(NewPlan(AcInRoomA), default);

        Assert.Equal("maintenance.action_not_allowed", await ErrorCodeAsync(() => Plans.ApproveAsync(plan.Id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task EditPlan_ApprovedPlan_GoesBackToApprovalAndKeepsTheSchedule()
    {
        var id = await ActivePlanAsync();
        await Generator.RunOnceAsync(default);
        As(Staff);

        var edited = await Plans.UpdateAsync(id, NewPlan(AcInRoomA) with { Frequency = 2 }, default);

        Assert.Equal(TriggerStatus.PendingApproval, edited.Status);
        Assert.Equal(Today.AddDays(2), Assert.Single(edited.Assets).LastDueDate);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task CreatePlan_FirstDueDayInThePast_Throws()
    {
        As(Staff);
        var request = NewPlan(AcInRoomA);
        request = request with { Assets = [request.Assets[0] with { StartDate = Today.AddDays(-3) }] };

        Assert.Equal("maintenance.start_in_past", await ErrorCodeAsync(() => Plans.CreateAsync(request, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task EditPlan_RunningForMonths_KeepsItsOldFirstDueDay()
    {
        var id = await ActivePlanAsync();
        var sameDays = NewPlan(AcInRoomA) with { LeadDays = 5 };
        Clock.Now = Clock.Now.AddMonths(3);
        As(Staff);

        var edited = await Plans.UpdateAsync(id, sameDays, default);

        Assert.Equal(5, edited.LeadDays);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task RejectPlan_Manager_TellsTheStaffWhy()
    {
        As(Staff);
        var plan = await Plans.CreateAsync(NewPlan(AcInRoomA), default);
        As(Manager);

        var result = await Plans.RejectAsync(plan.Id, new RejectPlanRequest("Monthly is too often for these units"), default);

        Assert.Equal(TriggerStatus.Rejected, result.Status);
        Assert.Contains(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("too often"));
    }

    // ----- generator -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Generator_ActivePlan_MakesOneAssignedWorkOrderWithTheChecklistOnce()
    {
        await ActivePlanAsync();

        Assert.Equal(1, await Generator.RunOnceAsync(default));
        Assert.Equal(0, await Generator.RunOnceAsync(default));

        var workOrder = Db.Cases.Single(c => c.TriggerWorkOrderId != null);
        Assert.Equal(CaseStatus.Assigned, workOrder.Status);
        Assert.Null(workOrder.RequestingCustomerId);
        Assert.Equal(2, Db.CaseTasks.Count(t => t.CaseId == workOrder.Id && t.Status == CaseTaskStatus.Pending));
        Assert.Contains(Db.Notifications, n => n.UserId == Technician.Id && n.Details.Contains("preventive maintenance"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Generator_PendingPlanOrRetiredAsset_MakesNothing()
    {
        As(Staff);
        await Plans.CreateAsync(NewPlan(AcInRoomA), default);
        Assert.Equal(0, await Generator.RunOnceAsync(default));

        var id = (await Plans.ListAsync(default)).Single().Id;
        As(Manager);
        await Plans.ApproveAsync(id, default);
        AcInRoomA.RetiredAt = Clock.Now;
        await Db.SaveChangesAsync();

        Assert.Equal(0, await Generator.RunOnceAsync(default));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Generator_AssetWithAnOpenCase_WarnsTheStaff()
    {
        await NewCaseAsync(); // open case on room 501; attach it to the AC
        Db.Cases.Single().AssetId = AcInRoomA.Id;
        await Db.SaveChangesAsync();
        await ActivePlanAsync();

        await Generator.RunOnceAsync(default);

        Assert.Contains(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("has an open case"));
    }

    // ----- work on the generated work order -----

    private async Task<long> WorkOrderInProgressAsync()
    {
        await ActivePlanAsync();
        await Generator.RunOnceAsync(default);
        var id = Db.Cases.Single(c => c.TriggerWorkOrderId != null).Id;
        As(Technician);
        await Sut.AcceptAsync(id, default);
        await Sut.StartAsync(id, default);
        return id;
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Checklist_AllItemsPassed_CanBeReportedDone()
    {
        var id = await WorkOrderInProgressAsync();
        var detail = await Sut.GetAsync(id, default);
        Assert.DoesNotContain(CaseAction.CompleteChecklist, detail.AllowedActions);
        Assert.DoesNotContain(CaseAction.Complete, detail.AllowedActions); // the F3 repair record is not for F2

        foreach (var task in detail.Tasks)
        {
            await Sut.UpdateTaskAsync(id, task.Id, new UpdateTaskRequest(CaseTaskStatus.Pass, null, null, null, false), default);
        }

        var done = await Sut.CompleteChecklistAsync(id, new CompleteChecklistRequest(null, 1m, null), default);
        Assert.Equal(CaseStatus.Completed, done.Status);
        Assert.True(done.IsPreventive);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Checklist_FailWithoutPhoto_Throws()
    {
        var id = await WorkOrderInProgressAsync();
        var task = (await Sut.GetAsync(id, default)).Tasks[0];

        var code = await ErrorCodeAsync(() => Sut.UpdateTaskAsync(id, task.Id,
            new UpdateTaskRequest(CaseTaskStatus.Fail, "Filter torn", null, FailAction.FixedOnSite, false), default));

        Assert.Equal("case.photo_required", code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Checklist_DangerousFail_MakesAnEmergencyRepairCaseLinkedToTheItem()
    {
        var id = await WorkOrderInProgressAsync();
        var task = (await Sut.GetAsync(id, default)).Tasks[1];

        var result = await Sut.UpdateTaskAsync(id, task.Id,
            new UpdateTaskRequest(CaseTaskStatus.Fail, "Water leaks onto the socket", Photo, FailAction.CreateCase, true), default);

        var item = result.Tasks.Single(t => t.Id == task.Id);
        Assert.NotNull(item.RepairCaseId);
        var repair = Db.Cases.Single(c => c.Id == item.RepairCaseId);
        Assert.Equal(CaseStatus.Approved, repair.Status);
        Assert.Equal(CasePriority.Emergency, Db.PriorityLevels.Single(p => p.Id == repair.PriorityLevelId).Code);
        Assert.Single(item.PhotoUrls);
    }

    // ----- checklist templates -----

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task DeleteChecklist_UsedByAPlan_Throws()
    {
        await ActivePlanAsync();

        Assert.Equal("maintenance.checklist_in_use",
            await ErrorCodeAsync(() => new ChecklistService(Db).DeleteAsync(_acChecklist.Id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SaveChecklist_Edit_ReplacesTheItems()
    {
        var saved = await new ChecklistService(Db).SaveAsync(_acChecklist.Id,
            new ChecklistRequest("Split AC monthly service", null, NotCooling.Id, [new ChecklistItemRequest("Only one item", null)]), default);

        Assert.Equal("Only one item", Assert.Single(saved.Tasks).Name);
    }
}
