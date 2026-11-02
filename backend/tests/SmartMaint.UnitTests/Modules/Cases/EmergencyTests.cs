using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Cases.Validators;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>F4 emergency: intake, acknowledgement and escalation, downgrade, merge, milestones, 24/7 clock, report.</summary>
public sealed class EmergencyTests : CaseTestBase
{
    private readonly Staff _backup;

    public EmergencyTests()
    {
        _backup = new Staff("backup", "backup", "hash", false);
        Db.Accounts.Add(_backup);
        Db.SaveChanges();
        Db.StaffBuildingAssignments.Add(new StaffBuildingAssignment
        {
            StaffId = _backup.Id,
            BuildingId = Db.Floors.Single(f => f.Id == RoomA.FloorId).BuildingId,
            IsPrimary = false,
        });
        Db.SaveChanges();
    }

    /// <summary>The watch job runs with nobody signed in (nobody is skipped as "the actor").</summary>
    private async Task<int> RunWatchAsync()
    {
        Me.IsAuthenticated = false;
        var sent = await new EmergencyWatch(Db, Notifications, Clock).RunOnceAsync(default);
        Me.IsAuthenticated = true;
        return sent;
    }

    private bool Told(Account user, string text) => Db.Notifications.Any(n => n.UserId == user.Id && n.Details.Contains(text));

    /// <summary>A burst pipe reported by the customer of room 501, driven to <paramref name="target"/> by the staff and the technician.</summary>
    private async Task<long> EmergencyAtAsync(CaseStatus target)
    {
        var id = (await NewCaseAsync(BurstPipe)).Id;
        if (target == CaseStatus.Approved)
        {
            return id;
        }

        As(Staff);
        await Sut.AcknowledgeAsync(id, default);
        await Sut.AssignAsync(id, new AssignCaseRequest(Technician.Id), default);
        if (target == CaseStatus.Assigned)
        {
            return id;
        }

        As(Technician);
        await Sut.AcceptAsync(id, default);
        if (target == CaseStatus.Accepted)
        {
            return id;
        }

        await Sut.StartAsync(id, default);
        if (target == CaseStatus.InProgress)
        {
            return id;
        }

        await Sut.CompleteAsync(id, new CompleteWorkRequest("Pipe joint burst", null, null, null, Photo), default);
        if (target == CaseStatus.Completed)
        {
            return id;
        }

        As(Staff);
        await Sut.CloseAsync(id, default);
        return id;
    }

    // ----- intake + acknowledgement -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Create_Emergency_TellsOnlyThePrimaryStaffFirst()
    {
        await NewCaseAsync(BurstPipe);

        Assert.True(Told(Staff, "EMERGENCY"));
        Assert.False(Told(_backup, "EMERGENCY"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Acknowledge_Staff_TakesChargeAndTheButtonGoes()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved);
        Clock.Now = Clock.Now.AddMinutes(4);
        As(Staff);

        var result = await Sut.AcknowledgeAsync(id, default);

        Assert.Equal(Clock.Now, result.Emergency!.AcknowledgedAt);
        Assert.Equal(Staff.Id, result.Staff!.Id);
        Assert.DoesNotContain(CaseAction.Acknowledge, result.AllowedActions);
        Assert.Contains("after 4 min", result.History.Last().Reason);
        Assert.True(Told(Customer, "is handling your emergency"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Acknowledge_NormalCase_IsNotAllowed()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.AcknowledgeAsync(id, default)));
    }

    // ----- escalation clocks -----

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Watch_NotAcknowledged_BackupIsToldAtTenMinutesOnce()
    {
        await EmergencyAtAsync(CaseStatus.Approved);

        Clock.Now = Clock.Now.AddMinutes(9);
        Assert.Equal(0, await RunWatchAsync());

        Clock.Now = Clock.Now.AddMinutes(1);
        Assert.Equal(1, await RunWatchAsync());
        Assert.True(Told(_backup, "has not been acknowledged for 10 min"));
        Assert.Equal(0, await RunWatchAsync());
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Watch_NotAcknowledgedForThirtyMinutes_TellsTheManager()
    {
        await EmergencyAtAsync(CaseStatus.Approved);
        Clock.Now = Clock.Now.AddMinutes(30);

        await RunWatchAsync();

        Assert.True(Told(Manager, "nobody has acknowledged it"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Watch_AcknowledgedInTime_ChasesNobody()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved);
        Clock.Now = Clock.Now.AddMinutes(5);
        As(Staff);
        await Sut.AcknowledgeAsync(id, default);
        Clock.Now = Clock.Now.AddMinutes(30);

        Assert.Equal(0, await RunWatchAsync());
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Watch_TechnicianNotAcceptingForTenMinutes_IsReminded()
    {
        await EmergencyAtAsync(CaseStatus.Assigned);
        Clock.Now = Clock.Now.AddMinutes(10);

        await RunWatchAsync();

        Assert.True(Told(Technician, "still waits for you to accept it"));
        Assert.True(Told(Staff, "has not accepted EMERGENCY"));
    }

    // ----- downgrade -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Downgrade_BeforeAccepted_MovesTheCaseToF3WithoutADeadline()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved);
        As(Staff);

        var result = await Sut.DowngradeAsync(id, new DowngradeRequest(CasePriority.High, CostBearer.Customer, "Only a dripping tap, no flooding"), default);

        Assert.False(result.IsEmergencyFlow);
        Assert.Null(result.Emergency);
        Assert.Null(result.DueAt);
        Assert.Equal(CostBearer.Customer, result.CostBearer);
        Assert.Equal(CaseStatus.Approved, result.Status);
        Assert.Contains(CaseAction.Assign, result.AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Downgrade_AfterTheTechnicianAccepted_IsNotAllowed()
    {
        var id = await EmergencyAtAsync(CaseStatus.Accepted);
        As(Staff);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() =>
            Sut.DowngradeAsync(id, new DowngradeRequest(CasePriority.High, CostBearer.Building, "Not that bad after all"), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void DowngradeValidator_ToEmergencyOrShortReason_Fails()
    {
        var validator = new DowngradeRequestValidator();

        Assert.False(validator.Validate(new DowngradeRequest(CasePriority.Emergency, CostBearer.Building, "Long enough reason")).IsValid);
        Assert.False(validator.Validate(new DowngradeRequest(CasePriority.Low, CostBearer.Building, "too short")).IsValid);
        Assert.True(validator.Validate(new DowngradeRequest(CasePriority.Low, CostBearer.Building, "ten chars!")).IsValid);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task ChangeUrgency_OnAnEmergency_IsNotOffered()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved);
        As(Staff);

        var detail = await Sut.GetAsync(id, default);

        Assert.DoesNotContain(CaseAction.ChangeUrgency, detail.AllowedActions);
        Assert.Contains(CaseAction.Downgrade, detail.AllowedActions);
    }

    // ----- merge -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Merge_IntoAnOpenNormalCase_RaisesItAndTellsBothCustomersWhenFixed()
    {
        var main = await CaseAtAsync(CaseStatus.InProgress);
        As(Customer2);
        var report = (await Sut.CreateAsync(NewRequest(BurstPipe, RoomA2), default)).Id;
        As(Staff);

        var merged = await Sut.MergeAsync(report, new MergeCaseRequest(main, null), default);

        Assert.Equal(CaseStatus.Cancelled, merged.Status);
        Assert.Equal(main, merged.Emergency!.MergedInto!.Id);
        var target = await Sut.GetAsync(main, default);
        Assert.True(target.IsEmergencyFlow);
        Assert.Equal(report, Assert.Single(target.MergedCases).Id);
        Assert.True(Told(Customer2, "already being handled"));

        As(Technician);
        await Sut.CompleteAsync(main, new CompleteWorkRequest("Pipe joint burst", null, null, null, Photo), default);
        As(Staff);
        await Sut.CloseAsync(main, default);
        Assert.True(Told(Customer2, "was fixed under"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Merge_IntoACaseOfAnotherBuilding_Throws()
    {
        var other = (await NewCaseAsync(room: RoomB)).Id;
        var id = await EmergencyAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.invalid_merge_target",
            await ErrorCodeAsync(() => Sut.MergeAsync(id, new MergeCaseRequest(other, null), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Merge_IntoItself_Throws()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.invalid_merge_target",
            await ErrorCodeAsync(() => Sut.MergeAsync(id, new MergeCaseRequest(id, null), default)));
    }

    // ----- log, made safe, 24/7 clock -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task AddLog_ThirdPartyCalled_IsInTheHistoryAndTellsTheTechnician()
    {
        var id = await EmergencyAtAsync(CaseStatus.Assigned);
        As(Staff);

        var result = await Sut.AddLogAsync(id, new EmergencyLogRequest(EmergencyLogKind.ThirdParty, "Plumbing contractor, 0901 234 567, in 30 min"), default);

        Assert.StartsWith("Third party called:", result.History.Last().Reason);
        Assert.True(Told(Technician, "Plumbing contractor"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task MarkSafe_Technician_RecordsTheFirstMilestoneOnce()
    {
        var id = await EmergencyAtAsync(CaseStatus.InProgress);
        Clock.Now = Clock.Now.AddMinutes(25);

        var result = await Sut.MarkSafeAsync(id, default);

        Assert.Equal(Clock.Now, result.Emergency!.MadeSafeAt);
        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.MarkSafeAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task SlaReminder_EmergencyNotYetAssigned_IsOverdueTwoHoursAfterTheReport()
    {
        await EmergencyAtAsync(CaseStatus.Approved);
        var reminder = new SlaReminder(Db, Notifications, Clock);
        Clock.Now = Clock.Now.AddHours(2);

        await reminder.RunOnceAsync(default);

        Assert.True(Told(Manager, "is past its deadline"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Reopen_Emergency_RestartsTheTwentyFourSevenClock()
    {
        var id = await EmergencyAtAsync(CaseStatus.Closed);
        Clock.Now = Clock.Now.AddDays(1);
        As(Staff);

        var result = await Sut.ReopenAsync(id, new ReasonRequest("Water is leaking again from the joint"), default);

        Assert.Equal(Clock.Now.AddHours(2), result.DueAt);
        Assert.Null(result.Emergency!.MadeSafeAt);
    }

    // ----- post-incident report -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task IncidentReport_ClosedEmergency_IsSaved()
    {
        var id = await EmergencyAtAsync(CaseStatus.Closed);

        var result = await Sut.WriteIncidentReportAsync(id, new IncidentReportRequest(
            "Corridor floor wet for an hour", "Old pipe joints in restrooms", IncidentFollowUp.PreventivePlan), default);

        Assert.Equal(IncidentFollowUp.PreventivePlan, result.IncidentReport!.FollowUp);
        Assert.Contains(CaseAction.WriteIncidentReport, result.AllowedActions); // may be rewritten
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task IncidentReport_NormalCase_IsNotAllowed()
    {
        var id = await CaseAtAsync(CaseStatus.Closed);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.WriteIncidentReportAsync(id,
            new IncidentReportRequest("Nothing much happened", "Nothing to learn here", IncidentFollowUp.None), default)));
    }
}
