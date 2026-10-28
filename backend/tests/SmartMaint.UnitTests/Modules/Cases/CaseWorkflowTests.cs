using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>From assignment to rating: assign, accept, decline, start, hold, resume, complete, close, reopen, rate.</summary>
public sealed class CaseWorkflowTests : CaseTestBase
{
    private static DateOnly Today(FixedTimeProvider clock) => DateOnly.FromDateTime(clock.Now.UtcDateTime);

    // ----- Assign -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Assign_ActiveTechnician_CreatesWaitingAssignment()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        var result = await Sut.AssignAsync(id, new AssignCaseRequest(Technician.Id), default);

        Assert.Equal(CaseStatus.Assigned, result.Status);
        Assert.Equal(Technician.Id, result.Technician?.Id);
        var assignment = Assert.Single(result.Assignments);
        Assert.Equal(AssignmentAnswer.Waiting, assignment.Response);
        Assert.Equal(Staff.Id, assignment.AssignedBy.Id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Assign_UserWhoIsNotATechnician_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.invalid_technician",
            await ErrorCodeAsync(() => Sut.AssignAsync(id, new AssignCaseRequest(Customer.Id), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Assign_WhenAlreadyAssigned_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Assigned);
        As(Staff);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.AssignAsync(id, new AssignCaseRequest(Technician2.Id), default)));
    }

    // ----- Accept / Decline -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Accept_AssignedTechnician_AcceptsTheAssignment()
    {
        var id = await CaseAtAsync(CaseStatus.Assigned);
        As(Technician);

        var result = await Sut.AcceptAsync(id, default);

        Assert.Equal(CaseStatus.Accepted, result.Status);
        Assert.Equal(AssignmentAnswer.Accepted, Assert.Single(result.Assignments).Response);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Accept_TechnicianWhoIsNotAssigned_IsNotFound()
    {
        var id = await CaseAtAsync(CaseStatus.Assigned);
        As(Technician2);

        Assert.Equal("case.not_found", await ErrorCodeAsync(() => Sut.AcceptAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Decline_AssignedTechnician_SendsCaseBackToApprovedAndKeepsTheDecline()
    {
        var id = await CaseAtAsync(CaseStatus.Assigned);
        As(Technician);

        var result = await Sut.DeclineAsync(id, new ReasonRequest("Not my specialty, please reassign"), default);

        Assert.Equal(CaseStatus.Approved, result.Status);
        Assert.Null(result.Technician);
        var assignment = Assert.Single(result.Assignments);
        Assert.Equal(AssignmentAnswer.Declined, assignment.Response);
        Assert.Equal("Not my specialty, please reassign", assignment.DeclineReason);
        Assert.Empty(result.AllowedActions); // the technician still sees the case, but has nothing to do
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Decline_ThenAssignToSomeoneElse_KeepsBothAssignments()
    {
        var id = await CaseAtAsync(CaseStatus.Assigned);
        As(Technician);
        await Sut.DeclineAsync(id, new ReasonRequest("Fully booked with another job"), default);
        As(Staff);

        var result = await Sut.AssignAsync(id, new AssignCaseRequest(Technician2.Id), default);

        Assert.Equal(2, result.Assignments.Count);
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
        var id = await CaseAtAsync(CaseStatus.Assigned);
        As(Technician);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.StartAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Start_UrgencyHigh_UsesItsOwnSlaHours()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);
        await Sut.ApproveAsync(id, new ApproveCaseRequest(CasePriority.High, CostBearer.Building), default);
        await Sut.AssignAsync(id, new AssignCaseRequest(Technician.Id), default);
        As(Technician);
        await Sut.AcceptAsync(id, default);

        var result = await Sut.StartAsync(id, default);

        Assert.Equal(Clock.Now.AddHours(8), result.DueAt);
    }

    // ----- Hold / Resume -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Hold_WaitingParts_RecordsReasonAndExpectedDay()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Technician);
        var expected = Today(Clock).AddDays(2);

        var result = await Sut.HoldAsync(id, new HoldCaseRequest(HoldReason.WaitingParts, expected), default);

        Assert.Equal(CaseStatus.OnHold, result.Status);
        Assert.Equal(new HoldResponse(HoldReason.WaitingParts, expected), result.Hold);
        Assert.Contains("Waiting for parts", result.History.Last().Reason);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Hold_WaitingThirdParty_FlagsTheCaseAsOutsourced()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Technician);

        await Sut.HoldAsync(id, new HoldCaseRequest(HoldReason.WaitingThirdParty, Today(Clock)), default);

        var stored = await Db.Cases.FindAsync(id);
        Assert.Equal(Clock.Now, stored!.OutsourcedAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Hold_CaseNotInProgress_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Accepted);
        As(Technician);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.HoldAsync(id, new HoldCaseRequest(HoldReason.WaitingParts, Today(Clock)), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Resume_ByTechnician_ClearsTheHold()
    {
        var id = await CaseAtAsync(CaseStatus.OnHold);
        As(Technician);

        var result = await Sut.ResumeAsync(id, default);

        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Null(result.Hold);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Resume_ByStaff_LetsTheTechnicianContinue()
    {
        var id = await CaseAtAsync(CaseStatus.OnHold);
        As(Staff);

        var result = await Sut.ResumeAsync(id, default);

        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Equal(Staff.Id, result.History.Last().By.Id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Resume_CaseNotOnHold_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Technician);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.ResumeAsync(id, default)));
    }

    // ----- Complete: the repair record -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Complete_WithoutTypedHours_MeasuresStartToFinish()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Clock.Now = Clock.Now.AddHours(3);
        As(Technician);

        var result = await Sut.CompleteAsync(
            id, new CompleteWorkRequest("Blocked condensate pipe", "1x PVC elbow", null, Photo, Photo), default);

        Assert.Equal(CaseStatus.Completed, result.Status); // reported done is not closed (BR-13)
        Assert.Equal(new RepairReportResponse("Blocked condensate pipe", "1x PVC elbow", 3.00m), result.RepairReport);
        Assert.Equal(Clock.Now, result.CompletedAt);
        Assert.Equal(2, result.Photos.Count(p => p.Kind is CaseImageKind.Before or CaseImageKind.After));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Complete_WithTypedHours_TypedHoursWin()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Clock.Now = Clock.Now.AddHours(3);
        As(Technician);

        var result = await Sut.CompleteAsync(id, new CompleteWorkRequest("Worn part", null, 1.25m, null, Photo), default);

        Assert.Equal(1.25m, result.RepairReport?.LaborHours);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Complete_CaseOnHold_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.OnHold);
        As(Technician);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.CompleteAsync(id, new CompleteWorkRequest("Worn part", null, null, null, Photo), default)));
    }

    // ----- Close -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Close_CompletedCase_ByStaff_ClosesAndGivesTheCustomerARating()
    {
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Staff);

        var result = await Sut.CloseAsync(id, default);

        Assert.Equal(CaseStatus.Closed, result.Status);
        As(Customer);
        Assert.Contains(CaseAction.Rate, (await Sut.GetAsync(id, default)).AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Close_ByTechnician_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Technician);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.CloseAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Close_CaseStillInProgress_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.CloseAsync(id, default)));
    }

    // ----- Reopen -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Reopen_CompletedCase_GivesItBackToTheSameTechnicianAndResetsTheClock()
    {
        var id = await CaseAtAsync(CaseStatus.Completed);
        As(Staff);

        var result = await Sut.ReopenAsync(id, new ReasonRequest("Still dripping after the repair"), default);

        Assert.Equal(CaseStatus.Reopened, result.Status);
        Assert.Null(result.DueAt);
        Assert.Null(result.CompletedAt);
        Assert.Equal(2, result.Assignments.Count);
        Assert.Equal(AssignmentAnswer.Waiting, result.Assignments.Last().Response);
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
        As(Staff);

        var result = await Sut.ReopenAsync(id, new ReasonRequest("The customer says it is not fixed"), default);

        Assert.Equal(CaseStatus.Reopened, result.Status);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Reopen_ClosedMoreThanSevenDaysAgo_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);
        Clock.Now = Clock.Now.AddDays(7).AddSeconds(1);
        As(Staff);

        Assert.Equal("case.invalid_transition",
            await ErrorCodeAsync(() => Sut.ReopenAsync(id, new ReasonRequest("The customer says it is not fixed"), default)));
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
