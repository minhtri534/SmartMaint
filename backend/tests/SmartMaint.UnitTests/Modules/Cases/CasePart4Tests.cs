using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Cases.Validators;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>Part 4 of F3: reassign, change urgency, extend the deadline, "not fixed", costs, SLA reminders.</summary>
public sealed class CasePart4Tests : CaseTestBase
{
    private const string Why = "The technician is needed elsewhere";

    // ----- Reassign -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Reassign_CaseInProgress_GoesBackToAssignedAndKeepsTheClock()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        var due = (await Sut.GetAsync(id, default)).DueAt;
        As(Staff);

        var result = await Sut.ReassignAsync(id, new ReassignCaseRequest(Technician2.Id, Why), default);
        As(Technician2);
        await Sut.AcceptAsync(id, default);
        var restarted = await Sut.StartAsync(id, default);

        Assert.Equal(CaseStatus.Assigned, result.Status);
        Assert.StartsWith("Reassigned from tech to tech2", result.History[^1].Reason);
        Assert.Equal(due, restarted.DueAt);
        Assert.Contains(Db.Notifications, n => n.UserId == Technician.Id && n.Details.Contains("taken back"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Reassign_ToTheSameTechnician_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);

        Assert.Equal("case.same_technician",
            await ErrorCodeAsync(() => Sut.ReassignAsync(id, new ReassignCaseRequest(Technician.Id, Why), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Reassign_CaseNotAssignedYet_UsesAssignInstead()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.ReassignAsync(id, new ReassignCaseRequest(Technician2.Id, Why), default)));
    }

    // ----- Change urgency -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ChangeUrgency_StartedCase_MovesTheDeadlineToTheNewLevel()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);

        var result = await Sut.ChangeUrgencyAsync(id, new ChangeUrgencyRequest(CasePriority.High, "Water now reaches the server room"), default);

        Assert.Equal(CasePriority.High, result.Urgency);
        Assert.Equal(result.SlaStartedAt!.Value.AddHours(8), result.DueAt);
        Assert.Equal(CaseStatus.InProgress, result.History[^1].To); // same status, history row with the reason
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task ChangeUrgency_ToTheSameLevel_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.same_urgency", await ErrorCodeAsync(() =>
            Sut.ChangeUrgencyAsync(id, new ChangeUrgencyRequest(CasePriority.Normal, "Nothing really changed"), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task ChangeUrgency_ToEmergency_HandsTheCaseToF4()
    {
        var id = await CaseAtAsync(CaseStatus.Assigned);
        As(Staff);

        var result = await Sut.ChangeUrgencyAsync(id, new ChangeUrgencyRequest(CasePriority.Emergency, "Sparks from the socket"), default);

        Assert.True(result.IsEmergencyFlow);
        Assert.Equal(Clock.Now.AddHours(2), result.DueAt); // 24/7 from now, not from the start of work
        Assert.Contains(CaseAction.Downgrade, result.AllowedActions);
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
        Assert.Contains("waiting for parts", result.History[^1].Reason);
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
    public async Task Complete_WithPartsAndAnHourlyRate_AddsThePartsAndALabourLine()
    {
        Technician.HourlyRate = 100_000m;
        var id = await CaseAtAsync(CaseStatus.InProgress);

        var result = await Sut.CompleteAsync(id, new CompleteWorkRequest("Worn capacitor", null, 1.5m, null, Photo,
            [new CostLineRequest("Capacitor 35uF", null, 150_000m)]), default);

        Assert.Equal(2, result.Costs!.Count);
        Assert.Equal(150_000m, result.Costs.Single(c => c.Name == "Labour").Amount);
        Assert.Equal(300_000m, result.TotalCost);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Costs_CustomerWhenTheBuildingPays_SeesNoCosts()
    {
        Technician.HourlyRate = 100_000m;
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Customer);

        Assert.Null((await Sut.GetAsync(id, default)).Costs);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task UpdateCosts_StaffBeforeClosing_ReplacesAllLines()
    {
        Technician.HourlyRate = 100_000m;
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Staff);

        var result = await Sut.UpdateCostsAsync(id, new UpdateCostsRequest([new CostLineRequest("Labour", "Corrected", 0m)]), default);

        Assert.Equal(0m, Assert.Single(result.Costs!).Amount);
        Assert.Equal(0m, result.TotalCost);
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
