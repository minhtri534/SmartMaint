using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Cases.Validators;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>F4 emergency: intake, acknowledgement and escalation, lowering it with Edit, milestones, 24/7 clock, report.</summary>
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

    /// <summary>
    /// A burst pipe reported by the customer of room 501, driven to <paramref name="target"/> by the staff and the
    /// technician. At Approved, <paramref name="named"/> = the staff already named the technician (with Edit).
    /// </summary>
    private async Task<long> EmergencyAtAsync(CaseStatus target, bool named = false)
    {
        var id = (await NewCaseAsync(BurstPipe)).Id;
        if (target == CaseStatus.Approved && !named)
        {
            return id;
        }

        As(Staff);
        await Sut.AcknowledgeAsync(id, default);
        await Sut.EditAsync(id, await EditOfAsync(id) with { TechnicianId = Technician.Id }, default);
        if (target == CaseStatus.Approved)
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

        await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);
        if (target == CaseStatus.Completed)
        {
            return id;
        }

        await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Close }, default);
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
        Assert.Single(result.History); // meeting 07/11: no history row, the time is on the case
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
        await EmergencyAtAsync(CaseStatus.Approved, named: true);
        Clock.Now = Clock.Now.AddMinutes(10);

        await RunWatchAsync();

        Assert.True(Told(Technician, "still waits for you to accept it"));
        Assert.True(Told(Staff, "has not accepted EMERGENCY"));
    }

    // ----- not an emergency after all: the staff lowers the urgency with Edit (meeting 07/11) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Edit_LowerUrgencyOfAnEmergency_MovesTheCaseToF3WithoutADeadline()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved);

        var result = await Sut.EditAsync(id, await EditOfAsync(id) with { Urgency = CasePriority.High }, default);

        Assert.False(result.IsEmergencyFlow);
        Assert.Null(result.Emergency);
        Assert.Null(result.DueAt);
        Assert.Equal(CaseStatus.Approved, result.Status);
        Assert.Contains(CaseAction.Edit, result.AllowedActions);
        Assert.DoesNotContain(CaseAction.Acknowledge, result.AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Edit_EmergencyOfAnotherBuilding_IsNotFound()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved);
        var edit = await EditOfAsync(id);
        As(StaffB);

        Assert.Equal("case.not_found", await ErrorCodeAsync(() => Sut.EditAsync(id, edit with { Urgency = CasePriority.Low }, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Edit_EmergencyKeptAsEmergency_KeepsTheTwentyFourSevenClock()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved);
        var due = (await Sut.GetAsync(id, default)).DueAt;
        Clock.Now = Clock.Now.AddMinutes(20);

        var result = await Sut.EditAsync(id, await EditOfAsync(id) with { TechnicianId = Technician.Id }, default);

        Assert.True(result.IsEmergencyFlow);
        Assert.Equal(due, result.DueAt);
    }

    // ----- log, made safe, 24/7 clock -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task AddLog_ThirdPartyCalled_IsInTheHistoryAndTellsTheTechnician()
    {
        var id = await EmergencyAtAsync(CaseStatus.Approved, named: true);
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
    public async Task EditReopen_Emergency_RestartsTheTwentyFourSevenClock()
    {
        var id = await EmergencyAtAsync(CaseStatus.Closed);
        Clock.Now = Clock.Now.AddDays(1);

        var result = await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Reopen, Reason = "Water is leaking again from the joint" }, default);

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
