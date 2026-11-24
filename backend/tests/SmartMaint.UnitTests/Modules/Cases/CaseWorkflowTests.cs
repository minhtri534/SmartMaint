using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>From approval to rating: approve with a technician, accept, decline, start, hold, resume, complete, close / reopen (Edit), rate.</summary>
public sealed class CaseWorkflowTests : CaseTestBase
{
    // ----- Approve names the technician (meeting 07/11: no "Assigned" status) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Approve_WithATechnician_RecordsWhoGotItInTheApprovedRow()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);

        var result = await Sut.ApproveAsync(id, Approval(), default);

        Assert.Equal(CaseStatus.Approved, result.Status);
        Assert.Equal(Technician.Id, result.Technician?.Id);
        var row = result.History[^1];
        Assert.Equal(CaseStatus.Pending, row.From);
        Assert.Equal(Staff.Id, row.By.Id);
        Assert.Equal("Assigned to tech.", row.Reason);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Approve_UserWhoIsNotATechnician_Throws()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);

        Assert.Equal("case.invalid_technician",
            await ErrorCodeAsync(() => Sut.ApproveAsync(id, Approval(technician: Customer), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Reassign_ApprovedCaseNotAcceptedYet_JustChangesTheTechnician()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        var result = await Sut.ReassignAsync(id, new ReassignRequest(Technician2.Id), default);

        Assert.Equal(CaseStatus.Approved, result.Status);
        Assert.Equal(Technician2.Id, result.Technician?.Id);
        Assert.Null(result.NextCase); // no new case: nobody had taken the work yet
        Assert.Contains(Db.Notifications, n => n.UserId == Technician2.Id && n.Details.Contains("assigned to you"));
    }

    // ----- Accept / Decline -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Accept_NamedTechnician_AcceptsTheJob()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Technician);

        var result = await Sut.AcceptAsync(id, default);

        Assert.Equal(CaseStatus.Accepted, result.Status);
        Assert.Equal(Technician.Id, result.History[^1].By.Id);
        Assert.Empty(result.Rejections);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Accept_TechnicianWhoIsNotNamed_IsNotFound()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Technician2);

        Assert.Equal("case.not_found", await ErrorCodeAsync(() => Sut.AcceptAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Decline_NamedTechnician_LeavesTheCaseApprovedWithNobodyAndKeepsTheDecline()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Technician);

        var result = await Sut.DeclineAsync(id, new ReasonRequest("Not my specialty, please reassign"), default);

        Assert.Equal(CaseStatus.Approved, result.Status);
        Assert.Null(result.Technician);
        var rejection = Assert.Single(result.Rejections);
        Assert.Equal(Technician.Id, rejection.Technician.Id);
        Assert.Equal("Not my specialty, please reassign", rejection.Reason);
        Assert.Equal(Clock.Now, rejection.RejectedAt);
        Assert.Empty(result.AllowedActions); // the technician still sees the case, but has nothing to do
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Decline_ThenReassignToSomeoneElse_KeepsTheDeclineAndTheNewTechnician()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Technician);
        await Sut.DeclineAsync(id, new ReasonRequest("Fully booked with another job"), default);
        As(Staff);

        var result = await Sut.ReassignAsync(id, new ReassignRequest(Technician2.Id), default);

        Assert.Equal(Technician.Id, Assert.Single(result.Rejections).Technician.Id);
        Assert.Equal(Technician2.Id, result.Technician?.Id);
    }
    // ----- Start: the SLA clock -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Start_AcceptedCase_StartsTheSlaClock()
    {
        var id = await CaseAtAsync(CaseStatus.Accepted);
        Clock.Now = new DateTimeOffset(2026, 10, 28, 9, 30, 0, TimeSpan.Zero);
        As(Technician);

        var result = await Sut.StartAsync(id, default);

        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Equal(Clock.Now, result.SlaStartedAt);
        Assert.Equal(Clock.Now.AddHours(24), result.DueAt); // Normal = 24 h
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Start_BeforeAccepting_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Technician);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.StartAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Start_UrgencyHigh_UsesItsOwnSlaHours()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);
        await Sut.ApproveAsync(id, Approval(CasePriority.High), default);
        As(Technician);
        await Sut.AcceptAsync(id, default);

        var result = await Sut.StartAsync(id, default);

        Assert.Equal(Clock.Now.AddHours(8), result.DueAt);
    }

    // ----- Hold / Resume: the status dropdown and the work clock (meeting 07/11) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Hold_InProgress_StopsTheClockWithoutAReason()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Clock.Now = Clock.Now.AddHours(2);
        As(Technician);

        var result = await Sut.HoldAsync(id, default);

        Assert.Equal(CaseStatus.OnHold, result.Status);
        Assert.Null(result.Hold); // no reason, no expected day any more
        Assert.Equal(2m, result.WorkedHours);
        Assert.Null(result.ClockStartedAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Resume_AfterAHold_TheClockOnlyCountsTheWork()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Technician);
        Clock.Now = Clock.Now.AddHours(2);
        await Sut.HoldAsync(id, default);
        Clock.Now = Clock.Now.AddHours(5); // waiting: not counted
        await Sut.ResumeAsync(id, default);
        Clock.Now = Clock.Now.AddHours(1);

        var result = await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);

        Assert.Equal(3m, result.WorkedHours);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Hold_CaseNotInProgress_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Accepted);
        As(Technician);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.HoldAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Resume_ByTechnician_RunsTheClockAgain()
    {
        var id = await CaseAtAsync(CaseStatus.OnHold);
        As(Technician);

        var result = await Sut.ResumeAsync(id, default);

        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Equal(Clock.Now, result.ClockStartedAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task EditContinue_ByStaff_LetsTheTechnicianContinue()
    {
        var id = await CaseAtAsync(CaseStatus.OnHold);

        var result = await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Continue }, default);

        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Equal(Staff.Id, result.History.Last().By.Id);
        Assert.NotNull(result.ClockStartedAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Resume_CaseNotOnHold_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Technician);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.ResumeAsync(id, default)));
    }

    // ----- Complete: nothing to fill in (meeting 07/11) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Complete_NothingToFill_StopsTheClockAndKeepsTheHours()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Clock.Now = Clock.Now.AddHours(3);
        As(Technician);

        var result = await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);

        Assert.Equal(CaseStatus.Completed, result.Status); // reported done is not closed (BR-13)
        Assert.Equal(3m, result.WorkedHours);
        Assert.Null(result.RepairReport); // no repair record any more
        Assert.Equal(Clock.Now, result.CompletedAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task LogHours_TypedTotal_ReplacesTheMeasuredTimeAndTheClockGoesOn()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Clock.Now = Clock.Now.AddHours(3);
        As(Technician);
        await Sut.LogHoursAsync(id, new LogHoursRequest(1.25m), default);
        Clock.Now = Clock.Now.AddHours(1);

        var result = await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);

        Assert.Equal(2.25m, result.WorkedHours);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task LogHours_AfterCompletion_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Technician);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.LogHoursAsync(id, new LogHoursRequest(2m), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Complete_Again_AfterAReopen_ReplacesTheLabourLine()
    {
        Technician.HourlyRate = 100_000m;
        var id = await CaseAtAsync(CaseStatus.Completed);
        await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Reopen, Reason = "Still drips water onto the desk" }, default);
        As(Technician);
        await Sut.AcceptAsync(id, default);
        await Sut.StartAsync(id, default);
        await Sut.LogHoursAsync(id, new LogHoursRequest(2m), default);

        var result = await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);

        Assert.Equal(200_000m, Assert.Single(result.Costs!, c => c.Name == "Labour").Amount);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Complete_CaseOnHold_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.OnHold);
        As(Technician);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.CompleteAsync(id, new CompleteWorkRequest(), default)));
    }

    // ----- The technician's costs and task notes (meeting 07/11) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task AddCost_WithReceipt_IsKeptAndTellsTheStaff()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Technician);

        var result = await Sut.AddCostAsync(id, new AddCostRequest("Capacitor 35uF", "Shop on Le Loi", 150_000m, "R-102", Photo[0]), default);

        var cost = Assert.Single(result.Costs!);
        Assert.Equal("R-102", cost.ReceiptCode);
        Assert.NotNull(cost.ReceiptUrl);
        Assert.Contains(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("added a cost"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task UpdateCosts_StaffKeepsALine_ItsReceiptStays()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Technician);
        var added = await Sut.AddCostAsync(id, new AddCostRequest("Capacitor 35uF", null, 150_000m, "R-102", null), default);
        As(Staff);

        var result = await Sut.UpdateCostsAsync(id, new UpdateCostsRequest(
            [new CostLineRequest("Capacitor 35uF", "Checked", 140_000m, added.Costs![0].Id), new CostLineRequest("Taxi", null, 50_000m)]), default);

        Assert.Equal(2, result.Costs!.Count);
        Assert.Equal("R-102", result.Costs[0].ReceiptCode);
        Assert.Equal(140_000m, result.Costs[0].Amount);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task UpdateTask_NoteAndCaptionedPhotoWhileStillToDo_AreKept()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Technician);
        var taskId = (await Sut.GetAsync(id, default)).Tasks.Single().Id;

        var result = await Sut.UpdateTaskAsync(id, taskId, new UpdateTaskRequest(
            SmartMaint.Api.Entities.Maintenance.CaseTaskStatus.Pending, "Drain pipe blocked, cleaning it",
            [new TaskPhotoRequest(Photo[0], "Before cleaning")], null, false), default);

        var task = Assert.Single(result.Tasks);
        Assert.Null(task.DoneBy);
        Assert.Equal("Before cleaning", Assert.Single(task.Photos).Caption);
        Assert.Single(task.Notes);
    }

    // ----- Close and reopen: from the staff's Edit (meeting 07/11) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task EditClose_CompletedCase_ClosesAndGivesTheCustomerARating()
    {
        var id = await CaseAtAsync(CaseStatus.Completed);

        var result = await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Close }, default);

        Assert.Equal(CaseStatus.Closed, result.Status);
        As(Customer);
        Assert.Contains(CaseAction.Rate, (await Sut.GetAsync(id, default)).AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Get_CompletedCase_OffersCloseAndReopenInTheEditForm()
    {
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Staff);

        var result = await Sut.GetAsync(id, default);

        Assert.Equal([EditStatusChange.Close, EditStatusChange.Reopen], result.StatusChanges);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task EditClose_CaseStillInProgress_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        var edit = await EditOfAsync(id);

        Assert.Equal("case.status_change_not_allowed",
            await ErrorCodeAsync(() => Sut.EditAsync(id, edit with { StatusChange = EditStatusChange.Close }, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Edit_ByTechnician_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Completed);
        var edit = await EditOfAsync(id);
        As(Technician);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.EditAsync(id, edit with { StatusChange = EditStatusChange.Close }, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task EditReopen_CompletedCase_GivesItBackToTheSameTechnicianAndResetsTheClock()
    {
        var id = await CaseAtAsync(CaseStatus.Completed);

        var result = await Sut.EditAsync(id, await EditOfAsync(id) with
        {
            StatusChange = EditStatusChange.Reopen,
            Reason = "Still dripping after the repair",
        }, default);

        Assert.Equal(CaseStatus.Reopened, result.Status);
        Assert.Equal("Still dripping after the repair", result.History[^1].Reason);
        Assert.Null(result.DueAt);
        Assert.Null(result.CompletedAt);
        Assert.Equal(Technician.Id, result.Technician?.Id);
        As(Technician);
        Assert.Contains(CaseAction.Accept, (await Sut.GetAsync(id, default)).AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Reopen_ClosedExactlySevenDaysAgo_IsStillAllowed()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        Clock.Now = Clock.Now.AddDays(7);

        var result = await Sut.ReopenAsync(id, new ReasonRequest("The customer says it is not fixed"), default);

        Assert.Equal(CaseStatus.Reopened, result.Status);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task ClosedCase_IsReadOnlyButForReopen()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        As(Staff);

        var actions = (await Sut.GetAsync(id, default)).AllowedActions;

        Assert.Contains(CaseAction.Reopen, actions);
        Assert.DoesNotContain(CaseAction.Edit, actions);
        Assert.DoesNotContain(CaseAction.UpdateCosts, actions);
        Assert.DoesNotContain(CaseAction.ManageFollowers, actions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Edit_ClosedMoreThanSevenDaysAgo_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        var edit = await EditOfAsync(id);
        Clock.Now = Clock.Now.AddDays(7).AddSeconds(1);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.EditAsync(id, edit with
        {
            StatusChange = EditStatusChange.Reopen,
            Reason = "The customer says it is not fixed",
        }, default)));
    }

    // ----- Rate -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Rate_ClosedCase_SavesStarsAndComment()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        As(Customer);

        var result = await Sut.RateAsync(id, new RateCaseRequest(4, "  Quick fix  "), default);

        Assert.Equal(new RatingResponse(4, "Quick fix", Clock.Now), result.Rating);
        Assert.DoesNotContain(CaseAction.Rate, result.AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Rate_TwiceOrBeforeClosing_Throws()
    {
        var closed = await CaseAtAsync(CaseStatus.Closed);
        var completed = await CaseAtAsync(CaseStatus.Completed);
        As(Customer);
        await Sut.RateAsync(closed, new RateCaseRequest(5, null), default);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.RateAsync(closed, new RateCaseRequest(1, null), default)));
        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.RateAsync(completed, new RateCaseRequest(5, null), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Rate_ByAnotherCustomer_IsNotFound()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        As(Customer2);

        Assert.Equal("case.not_found", await ErrorCodeAsync(() => Sut.RateAsync(id, new RateCaseRequest(5, null), default)));
    }

    // ----- The state machine itself -----

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void Transitions_NoAction_LeavesAFinalStatus()
    {
        var finals = new[] { CaseStatus.Closed, CaseStatus.Rejected, CaseStatus.Cancelled };

        // Closed is the only final status that still has exits (reopen within 7 days, rate); Rejected and Cancelled have none.
        Assert.All(new[] { CaseStatus.Rejected, CaseStatus.Cancelled },
            s => Assert.DoesNotContain(CaseTransitions.Rules, r => r.From.Contains(s)));
        Assert.Contains(finals, s => CaseTransitions.Rules.Any(r => r.From.Contains(s)));
    }
}
