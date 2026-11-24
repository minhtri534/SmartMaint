using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Maintenance;
using SmartMaint.Api.Modules.Maintenance.Dtos;
using SmartMaint.Api.Modules.Maintenance.Validators;
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

    /// <summary>One trigger: every month from the day after tomorrow, no end.</summary>
    private PlanRequest NewPlan(params Asset[] assets) => new(
        "Split AC service", null, _acChecklist.Id, 1.5m, CasePriority.Normal,
        [NewTrigger(1, FrequencyUnit.Month, assets)]);

    private TriggerRequest NewTrigger(int every, FrequencyUnit unit, params Asset[] assets) => new(
        null, every, unit, 3, Today.AddDays(2), null, assets.Select(a => new TriggerAssetRequest(a.Id, Technician.Id)).ToList());

    /// <summary>The request the edit form sends for a saved plan, unchanged.</summary>
    private static PlanRequest AsRequest(PlanDetailResponse p) => new(
        p.Name, p.Description, p.ChecklistId, p.EstimatedHours, p.Priority,
        p.Triggers.Select(t => new TriggerRequest(t.Id, t.Frequency, t.FrequencyType, t.LeadDays, t.StartDate, t.EndDate,
            t.Assets.Select(a => new TriggerAssetRequest(a.AssetId, a.Technician?.Id)).ToList())).ToList());

    /// <summary>The same request with its first trigger changed.</summary>
    private static PlanRequest WithTrigger(PlanRequest r, Func<TriggerRequest, TriggerRequest> change) =>
        r with { Triggers = [change(r.Triggers[0]), .. r.Triggers.Skip(1)] };

    private Asset AcInRoomA2 => Db.Assets.Single(a => a.Name == "AC 502");

    /// <summary>A plan made by Staff and approved by Manager.</summary>
    private async Task<long> ActivePlanAsync(PlanRequest? request = null)
    {
        As(Staff);
        var plan = await Plans.CreateAsync(request ?? NewPlan(AcInRoomA), default);
        As(Manager);
        await Plans.ApproveAsync(plan.Id, default);
        return plan.Id;
    }

    // ----- schedule -----

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void Schedule_DueExactlyLeadDaysAhead_IsMadeAndOneDayEarlierIsNot()
    {
        var trigger = new MaintenanceWorkOrderDetail { Frequency = 1, FrequencyType = FrequencyUnit.Month, LeadDays = 3 };
        var asset = new MaintenanceWorkOrderAsset { StartTime = new DateOnly(2026, 11, 10) };

        Assert.Null(MaintenanceSchedule.DueToGenerate(asset, trigger, new DateOnly(2026, 11, 6)));
        Assert.Equal(new DateOnly(2026, 11, 10), MaintenanceSchedule.DueToGenerate(asset, trigger, new DateOnly(2026, 11, 7)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void Schedule_MissedPeriods_OnlyTheLatestIsMade()
    {
        var trigger = new MaintenanceWorkOrderDetail { Frequency = 1, FrequencyType = FrequencyUnit.Week, LeadDays = 0 };
        var asset = new MaintenanceWorkOrderAsset { StartTime = new DateOnly(2026, 10, 1) };

        Assert.Equal(new DateOnly(2026, 10, 29), MaintenanceSchedule.DueToGenerate(asset, trigger, new DateOnly(2026, 11, 2)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void Schedule_EndDay_TheDueDayOnItIsMadeTheNextIsNot()
    {
        var trigger = new MaintenanceWorkOrderDetail
        {
            Frequency = 1, FrequencyType = FrequencyUnit.Week, LeadDays = 0, EndDate = new DateOnly(2026, 11, 8),
        };
        var asset = new MaintenanceWorkOrderAsset { StartTime = new DateOnly(2026, 11, 1), LastDueDate = new DateOnly(2026, 11, 1) };

        Assert.Equal(new DateOnly(2026, 11, 8), MaintenanceSchedule.NextDue(asset, trigger));
        asset.LastDueDate = new DateOnly(2026, 11, 8);
        Assert.Null(MaintenanceSchedule.NextDue(asset, trigger));
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

        var request = WithTrigger(AsRequest(await Plans.GetAsync(id, default)), t => t with { Frequency = 2 });

        var edited = await Plans.UpdateAsync(id, request, default);

        Assert.Equal(TriggerStatus.PendingApproval, edited.Status);
        Assert.Equal(Today.AddDays(2), Assert.Single(Assert.Single(edited.Triggers).Assets).LastDueDate);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task CreatePlan_FirstDueDayInThePast_Throws()
    {
        As(Staff);
        var request = WithTrigger(NewPlan(AcInRoomA), t => t with { StartDate = Today.AddDays(-3) });

        Assert.Equal("maintenance.start_in_past", await ErrorCodeAsync(() => Plans.CreateAsync(request, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task EditPlan_RunningForMonths_KeepsItsOldFirstDueDay()
    {
        var id = await ActivePlanAsync();
        var sameDays = WithTrigger(AsRequest(await Plans.GetAsync(id, default)), t => t with { LeadDays = 5 });
        Clock.Now = Clock.Now.AddMonths(3);
        As(Staff);

        var edited = await Plans.UpdateAsync(id, sameDays, default);

        Assert.Equal(5, Assert.Single(edited.Triggers).LeadDays);
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

    // ----- triggers, end days, archive (meeting note 18/11, like UpKeep) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Plan_TwoTriggers_EachMakesWorkOrdersOnItsOwnSchedule()
    {
        var request = NewPlan(AcInRoomA) with
        {
            Triggers = [NewTrigger(1, FrequencyUnit.Month, AcInRoomA), NewTrigger(3, FrequencyUnit.Month, AcInRoomA2) with { StartDate = Today.AddDays(10) }],
        };
        var id = await ActivePlanAsync(request);

        Assert.Equal(1, await Generator.RunOnceAsync(default)); // AC 501 is due in 2 days, AC 502 in 10
        Clock.Now = Clock.Now.AddDays(8);
        Assert.Equal(1, await Generator.RunOnceAsync(default));

        As(Staff);
        var plan = await Plans.GetAsync(id, default);
        Assert.Equal(2, plan.Triggers.Count);
        Assert.Equal([new PlanEvery(1, FrequencyUnit.Month), new PlanEvery(3, FrequencyUnit.Month)], plan.Every);
        var workOrders = await Plans.WorkOrdersAsync(id, default);
        Assert.Equal(plan.Triggers.Select(t => t.Id).Reverse(), workOrders.Select(w => w.TriggerId)); // newest due first
        Assert.Equal(2, plan.WorkOrderCount);
        Assert.All(workOrders, w => Assert.Equal(Technician.FullName, w.TechnicianName));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void Validator_SameAssetInTwoTriggers_IsInvalid()
    {
        var request = NewPlan(AcInRoomA) with
        {
            Triggers = [NewTrigger(1, FrequencyUnit.Month, AcInRoomA), NewTrigger(1, FrequencyUnit.Year, AcInRoomA)],
        };

        var result = new PlanRequestValidator().Validate(request);

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("two triggers"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task ExtendPlan_OnlyTheEndDayMoved_KeepsRunningWithoutApproval()
    {
        // One due day only (start = end): after its work order the plan has nothing left to make.
        var id = await ActivePlanAsync(WithTrigger(NewPlan(AcInRoomA), t => t with { EndDate = t.StartDate }));
        await Generator.RunOnceAsync(default);
        As(Staff);
        var expired = await Plans.GetAsync(id, default);
        Assert.True(expired.Expired);
        Assert.Null(expired.NextDueDate);

        var extended = await Plans.UpdateAsync(id,
            WithTrigger(AsRequest(expired), t => t with { EndDate = Today.AddMonths(6) }), default);

        Assert.Equal(TriggerStatus.Active, extended.Status);
        Assert.False(extended.Expired);
        Assert.Equal(Today.AddDays(2).AddMonths(1), extended.NextDueDate);
        Assert.Contains(Db.Notifications, n => n.UserId == Manager.Id && n.Details.Contains("keeps running"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ArchivePlan_Staff_LocksItAndMakesNoMoreWorkOrders()
    {
        var id = await ActivePlanAsync();
        As(Staff);

        var archived = await Plans.ArchiveAsync(id, default);

        Assert.Equal(TriggerStatus.Archived, archived.Status);
        Assert.NotNull(archived.ArchivedAt);
        Assert.Empty(archived.AllowedActions);
        Assert.Equal(0, await Generator.RunOnceAsync(default));
        Assert.Equal("maintenance.action_not_allowed",
            await ErrorCodeAsync(() => Plans.UpdateAsync(id, AsRequest(archived), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task EditPlan_RemoveATriggerThatMadeWorkOrders_Throws()
    {
        var id = await ActivePlanAsync();
        await Generator.RunOnceAsync(default);
        As(Staff);
        var request = AsRequest(await Plans.GetAsync(id, default)) with
        {
            Triggers = [NewTrigger(1, FrequencyUnit.Month, AcInRoomA2)],
        };

        Assert.Equal("maintenance.trigger_has_work_orders", await ErrorCodeAsync(() => Plans.UpdateAsync(id, request, default)));
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
        Assert.Equal(CaseStatus.Approved, workOrder.Status); // with the plan's technician (no "Assigned" status)
        Assert.Equal(Technician.Id, workOrder.AssignedTechnicianId);
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
        Assert.DoesNotContain(CaseAction.Complete, detail.AllowedActions); // not before every item has a result

        foreach (var task in detail.Tasks)
        {
            await Sut.UpdateTaskAsync(id, task.Id, new UpdateTaskRequest(CaseTaskStatus.Pass, null, null, null, false), default);
        }

        var done = await Sut.CompleteAsync(id, new CompleteWorkRequest(), default); // meeting 07/11: the same "Completed"
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
            new UpdateTaskRequest(CaseTaskStatus.Fail, "Water leaks onto the socket", [new TaskPhotoRequest(Photo[0], "Socket under the unit")], FailAction.CreateCase, true), default);

        var item = result.Tasks.Single(t => t.Id == task.Id);
        Assert.NotNull(item.RepairCaseId);
        var repair = Db.Cases.Single(c => c.Id == item.RepairCaseId);
        Assert.Equal(CaseStatus.Approved, repair.Status);
        Assert.Equal(CasePriority.Emergency, Db.PriorityLevels.Single(p => p.Id == repair.PriorityLevelId).Code);
        Assert.Equal("Socket under the unit", Assert.Single(item.Photos).Caption); // meeting 07/11: photos with captions
    }

    // ----- checklist templates -----

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task DeleteChecklist_UsedByAPlan_Throws()
    {
        await ActivePlanAsync();

        Assert.Equal("maintenance.checklist_in_use",
            await ErrorCodeAsync(() => new ChecklistService(Db, Clock).DeleteAsync(_acChecklist.Id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SaveChecklist_Edit_ReplacesTheItems()
    {
        var saved = await new ChecklistService(Db, Clock).SaveAsync(_acChecklist.Id,
            new ChecklistRequest("Split AC monthly service", null, [new ChecklistItemRequest("Only one item", null)]), default);

        Assert.Equal("Only one item", Assert.Single(saved.Tasks).Name);
        Assert.Equal(NotCooling.Id, Db.CaseChecklists.Single(c => c.Id == _acChecklist.Id).CategoryId); // an old checklist keeps its own
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task SaveChecklist_New_UsesTheArchivedPreventiveCategory()
    {
        var service = new ChecklistService(Db, Clock);

        var first = await service.SaveAsync(null, new ChecklistRequest("Heater service", null, [new ChecklistItemRequest("Flush the tank", null)]), default);
        var second = await service.SaveAsync(null, new ChecklistRequest("Lift service", null, [new ChecklistItemRequest("Oil the rails", null)]), default);

        var categories = Db.CaseChecklists.Where(c => c.Id == first.Id || c.Id == second.Id).Select(c => c.CategoryId).Distinct().ToList();
        var category = Db.CaseCategories.Single(c => c.Id == Assert.Single(categories));
        Assert.Equal("Preventive maintenance", category.Name);
        Assert.NotNull(category.ArchivedAt);
    }
}
