using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Cases.Validators;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>The staff steps in: Edit (new technician, urgency, tasks), the deadline, "not fixed", costs, SLA reminders.</summary>
public sealed class CasePart4Tests : CaseTestBase
{

    // ----- A new technician on taken work = a new case (meeting 07/11, Leader 09/11: the old case closes) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Edit_NewTechnicianWhileInProgress_ContinuesInANewCaseAndClosesTheOld()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        var before = await Sut.GetAsync(id, default);
        var edit = await EditOfAsync(id);

        var old = await Sut.EditAsync(id, edit with { TechnicianId = Technician2.Id }, default);

        Assert.Equal(CaseStatus.Closed, old.Status);
        Assert.StartsWith("Reassigned to tech2", old.History[^1].Reason);
        Assert.NotNull(old.NextCase);
        var next = await Sut.GetAsync(old.NextCase!.Id, default);
        Assert.Equal(CaseStatus.Approved, next.Status);
        Assert.Equal(Technician2.Id, next.Technician?.Id);
        Assert.Equal(id, next.PreviousCase?.Id);
        Assert.Equal(before.DueAt, next.DueAt); // the SLA clock keeps running
        Assert.Equal(Customer.Id, next.Requester?.Id);
        Assert.Contains(Db.Notifications, n => n.UserId == Technician.Id && n.Details.Contains("was given to tech2"));
        Assert.Contains(Db.Notifications, n => n.UserId == Customer.Id && n.Details.Contains("continues as"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Edit_NewTechnician_FinishedTasksStayAndOpenOnesMove()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        var edit = await EditOfAsync(id);
        var withTwo = await Sut.EditAsync(id, edit with
        {
            Tasks = [.. edit.Tasks, new TaskLineRequest(null, "Check the drain pipe", null)],
        }, default);
        As(Technician);
        await Sut.UpdateTaskAsync(id, withTwo.Tasks[0].Id,
            new UpdateTaskRequest(SmartMaint.Api.Entities.Maintenance.CaseTaskStatus.Pass, "Capacitor replaced", null, null, false), default);

        var old = await Sut.EditAsync(id, await EditOfAsync(id) with { TechnicianId = Technician2.Id }, default);

        var kept = Assert.Single(old.Tasks);
        Assert.Equal(SmartMaint.Api.Entities.Maintenance.TaskDoneBy.Technician, kept.DoneBy);
        var moved = Assert.Single((await Sut.GetAsync(old.NextCase!.Id, default)).Tasks);
        Assert.Equal("Check the drain pipe", moved.Name);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Edit_RemoveTheTechnicianOfTakenWork_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        var edit = await EditOfAsync(id);

        Assert.Equal("case.technician_required",
            await ErrorCodeAsync(() => Sut.EditAsync(id, edit with { TechnicianId = null }, default)));
    }

    // ----- The urgency, from Edit -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Edit_UrgencyOfAStartedCase_MovesTheDeadlineToTheNewLevel()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        var historyBefore = (await Sut.GetAsync(id, default)).History.Count;

        var result = await Sut.EditAsync(id, await EditOfAsync(id) with { Urgency = CasePriority.High }, default);

        Assert.Equal(CasePriority.High, result.Urgency);
        Assert.Equal(result.SlaStartedAt!.Value.AddHours(8), result.DueAt);
        Assert.Equal(historyBefore, result.History.Count); // meeting 07/11: only status changes are kept
        Assert.Contains(Db.Notifications, n => n.UserId == Technician.Id && n.Details.Contains("Urgency Normal → High"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Edit_UrgencyEmergency_HandsTheCaseToF4AndBackAgain()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);

        var raised = await Sut.EditAsync(id, await EditOfAsync(id) with { Urgency = CasePriority.Emergency }, default);
        Assert.True(raised.IsEmergencyFlow);
        Assert.Equal(Clock.Now.AddHours(2), raised.DueAt); // 24/7 from now, not from the start of work

        var lowered = await Sut.EditAsync(id, EditOf(raised) with { Urgency = CasePriority.Normal }, default);
        Assert.False(lowered.IsEmergencyFlow);
        Assert.Null(lowered.DueAt); // F3 counts from the start of work again
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Edit_RemoveATaskThatHasAResult_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        var taskId = (await Sut.GetAsync(id, default)).Tasks.Single().Id;
        As(Technician);
        await Sut.UpdateTaskAsync(id, taskId,
            new UpdateTaskRequest(SmartMaint.Api.Entities.Maintenance.CaseTaskStatus.Pass, null, null, null, false), default);
        var edit = await EditOfAsync(id);

        Assert.Equal("case.task_done", await ErrorCodeAsync(() =>
            Sut.EditAsync(id, edit with { Tasks = [new TaskLineRequest(null, "Something else", null)] }, default)));
    }

    // ----- Extend the deadline -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ExtendDue_BeforeTheDeadline_MovesItAndSaysWhy()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        var newDue = Clock.Now.AddDays(3);
        As(Staff);

        var result = await Sut.ExtendDueAsync(id, new ExtendDueRequest(newDue, ExtendDueReason.WaitingParts, null), default);

        Assert.Equal(newDue, result.DueAt);
        Assert.Contains(Db.Notifications, n => n.UserId == Technician.Id && n.Details.Contains("waiting for parts"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task ExtendDue_EarlierThanNow_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);

        Assert.Equal("case.due_not_later", await ErrorCodeAsync(() =>
            Sut.ExtendDueAsync(id, new ExtendDueRequest(Clock.Now.AddHours(1), ExtendDueReason.Other, "test"), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task ExtendDue_ExactlyAtTheDeadline_IsTooLate()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Clock.Now = (await Sut.GetAsync(id, default)).DueAt!.Value;
        As(Staff);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() =>
            Sut.ExtendDueAsync(id, new ExtendDueRequest(Clock.Now.AddDays(1), ExtendDueReason.WaitingParts, null), default)));
    }

    // ----- "Not fixed" -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task NotFixed_ReportedThenAccepted_ReopensTheCase()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        As(Customer);
        var reported = await Sut.ReportNotFixedAsync(id, new ReasonRequest("Still warm air after one day"), default);
        As(Staff);

        var result = await Sut.AcceptNotFixedAsync(id, default);

        Assert.NotNull(reported.NotFixed);
        Assert.Equal(CaseStatus.Reopened, result.Status);
        Assert.Null(result.NotFixed);
        Assert.Contains(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("not fixed"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task NotFixed_Declined_StaysClosedAndTellsTheCustomer()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        As(Customer);
        await Sut.ReportNotFixedAsync(id, new ReasonRequest("Still warm air after one day"), default);
        As(Staff);

        var result = await Sut.DeclineNotFixedAsync(id, new ReasonRequest("Checked on site, it cools to 24 degrees"), default);

        Assert.Equal(CaseStatus.Closed, result.Status);
        Assert.Null(result.NotFixed);
        Assert.Contains(Db.Notifications, n => n.UserId == Customer.Id && n.Details.Contains("declined"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task NotFixed_SevenDaysAfterClosing_IsTheLastMoment()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        var closedAt = Clock.Now;
        As(Customer);

        Clock.Now = closedAt.AddDays(7).AddSeconds(1);
        Assert.DoesNotContain(CaseAction.ReportNotFixed, (await Sut.GetAsync(id, default)).AllowedActions);
        Clock.Now = closedAt.AddDays(7);
        Assert.Contains(CaseAction.ReportNotFixed, (await Sut.GetAsync(id, default)).AllowedActions);
    }

    // ----- Costs -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Complete_WithLoggedHoursAndAnHourlyRate_AddsALabourLine()
    {
        Technician.HourlyRate = 100_000m;
        var id = await CaseAtAsync(CaseStatus.InProgress);
        await Sut.AddCostAsync(id, new AddCostRequest("Capacitor 35uF", null, 150_000m, "R-102", null), default);
        await Sut.LogHoursAsync(id, new LogHoursRequest(1.5m), default);

        var result = await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);

        Assert.Equal(2, result.Costs!.Count);
        Assert.Equal(150_000m, result.Costs.Single(c => c.Name == "Labour").Amount);
        Assert.Equal(300_000m, result.TotalCost);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Costs_Customer_SeesNoCosts()
    {
        Technician.HourlyRate = 100_000m;
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Customer);

        Assert.Null((await Sut.GetAsync(id, default)).Costs);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task UpdateCosts_StaffAfterCompletion_ReplacesAllLines()
    {
        Technician.HourlyRate = 100_000m;
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Staff);

        var result = await Sut.UpdateCostsAsync(id, new UpdateCostsRequest([new CostLineRequest("Labour", "Corrected", 0m)]), default);

        Assert.Equal(0m, Assert.Single(result.Costs!).Amount);
        Assert.Equal(0m, result.TotalCost);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task UpdateCosts_StaffWhileInProgress_RecordsTitleDescriptionAndAmount()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);

        var result = await Sut.UpdateCostsAsync(id, new UpdateCostsRequest(
            [new CostLineRequest("Capacitor 35uF", "Receipt R-102", 150_000m), new CostLineRequest("Taxi", null, 50_000m)]), default);

        Assert.Equal(2, result.Costs!.Count);
        Assert.Equal(200_000m, result.TotalCost);
        Assert.Equal("Receipt R-102", result.Costs[0].Description);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task UpdateCosts_ByTechnician_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() =>
            Sut.UpdateCostsAsync(id, new UpdateCostsRequest([new CostLineRequest("Capacitor", null, 1m)]), default)));
    }

    // ----- SLA reminders -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SlaReminder_At80PercentOfTheTime_WarnsTheStaffOnce()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress); // normal: 24 h
        var job = new SlaReminder(Db, Notifications, Clock);

        Clock.Now = Clock.Now.AddHours(19); // 5 h left = 20.8 %: not yet
        Assert.Equal(0, await job.RunOnceAsync(default));
        Clock.Now = Clock.Now.AddMinutes(12); // 4.8 h left = 20 %
        Assert.Equal(1, await job.RunOnceAsync(default));
        Assert.Equal(0, await job.RunOnceAsync(default));

        Assert.Single(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("close to its deadline"));
        Assert.NotNull(Db.Cases.Single(c => c.Id == id).SlaWarnedAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task SlaReminder_ExactlyAtTheDeadline_TellsStaffAndManagerItIsOverdue()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Clock.Now = Db.Cases.Single(c => c.Id == id).DueAt!.Value;

        await new SlaReminder(Db, Notifications, Clock).RunOnceAsync(default);

        Assert.Contains(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("past its deadline"));
        Assert.Contains(Db.Notifications, n => n.UserId == Manager.Id && n.Details.Contains("past its deadline"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SlaReminder_CompletedCase_IsNotReminded()
    {
        await CaseAtAsync(CaseStatus.Completed);
        Clock.Now = Clock.Now.AddDays(5);

        Assert.Equal(0, await new SlaReminder(Db, Notifications, Clock).RunOnceAsync(default));
    }

    // ----- validators -----

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(30, true)]
    [InlineData(31, false)]
    public void ExtendDueValidator_AtMost30DaysAhead(int days, bool valid) =>
        Assert.Equal(valid, new ExtendDueRequestValidator(Clock)
            .Validate(new ExtendDueRequest(Clock.Now.AddDays(days), ExtendDueReason.WaitingParts, null)).IsValid);
}
