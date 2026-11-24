using ClosedXML.Excel;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Analytics;
using SmartMaint.Api.Modules.Analytics.Dtos;
using SmartMaint.Api.Modules.Analytics.Validators;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.UnitTests.Common;
using SmartMaint.UnitTests.Modules.Cases;

namespace SmartMaint.UnitTests.Modules.Analytics;

/// <summary>F6: KPI definitions (on time by both deadlines, exclusions), scope, preventive compliance, attention, export.</summary>
public sealed class AnalyticsTests : CaseTestBase
{
    private AnalyticsService Analytics => new(Db, Me, new BuildingScope(Db, Me), Clock);

    private static readonly AnalyticsQuery All = new();

    /// <summary>A normal case in room 501 (SLA 24 h from the start of work), reported done <paramref name="hoursAfterStart"/> after the start.</summary>
    private async Task<long> CompletedAsync(double hoursAfterStart)
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Clock.Now = Clock.Now.AddHours(hoursAfterStart);
        As(Technician);
        await Sut.LogHoursAsync(id, new LogHoursRequest(1), default);
        await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);
        return id;
    }

    // ----- ANA-01 -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Overview_OneOnTimeOneLate_HalfOnTime()
    {
        await CompletedAsync(2);
        await CompletedAsync(30);
        As(Manager);

        var kpis = (await Analytics.OverviewAsync(All, default)).Current;

        Assert.Equal(2, kpis.Reported);
        Assert.Equal(2, kpis.Completed);
        Assert.Equal(0.5, kpis.OnTimeRate);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Overview_OpenCasePastItsDeadline_CountsAsMissed()
    {
        await CompletedAsync(2);                       // done on time
        await CaseAtAsync(CaseStatus.InProgress);       // due in 24 h, still in work
        Clock.Now = Clock.Now.AddHours(25);
        As(Manager);

        var overview = await Analytics.OverviewAsync(All, default);

        Assert.Equal(1, overview.Current.Completed);
        Assert.Equal(0.5, overview.Current.OnTimeRate); // not 100 %: the open one already missed its deadline
        Assert.Equal(1, Assert.Single(overview.ByBuilding, b => b.OpenNow > 0).OverdueNow);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Overview_PendingAndWaitingForVerification_AreOpenNow()
    {
        await NewCaseAsync();                            // pending
        await CompletedAsync(2);                         // reported done, the staff have not verified it
        As(Manager);

        var building = Assert.Single((await Analytics.OverviewAsync(All, default)).ByBuilding, b => b.OpenNow > 0);

        Assert.Equal(2, building.OpenNow);
        Assert.Equal(0, building.OverdueNow);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Overview_DeadlineMoved_BothRatesAreShown()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);
        await Sut.ExtendDueAsync(id, new ExtendDueRequest(Clock.Now.AddHours(48), ExtendDueReason.WaitingParts, null), default);
        Clock.Now = Clock.Now.AddHours(30);
        As(Technician);
        await Sut.LogHoursAsync(id, new LogHoursRequest(1), default);
        await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);
        As(Manager);

        var kpis = (await Analytics.OverviewAsync(All, default)).Current;

        Assert.Equal(1.0, kpis.OnTimeRate);         // by the deadline in force
        Assert.Equal(0.0, kpis.OnTimeOriginalRate); // by the first SLA deadline (S02)
        Assert.Equal(1, kpis.DeadlineMoves);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Overview_SurveyAndRejected_AreNotInTheRates()
    {
        await CompletedAsync(2);
        var rejected = await CaseAtAsync(CaseStatus.Pending);
        As(Staff);
        await Sut.RejectAsync(rejected, new RejectCaseRequest(RejectReason.MissingInfo, "Which socket, please?"), default);
        await Sut.OpenSurveyAsync(new OpenSurveyRequest(AcInRoomA.Id, NotCooling.Id, null), default);

        var kpis = (await Analytics.OverviewAsync(All, default)).Current;

        Assert.Equal(2, kpis.Reported); // the survey is not a report
        Assert.Equal(1, kpis.Rejected);
        Assert.Equal(1, kpis.Completed);
        Assert.Equal(1.0, kpis.OnTimeRate);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Overview_Ratings_OnlyForTheManager()
    {
        var id = await CompletedAsync(2);
        As(Staff);
        await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Close }, default);
        As(Customer);
        await Sut.RateAsync(id, new RateCaseRequest(4, ""), default);

        As(Staff);
        Assert.Null((await Analytics.OverviewAsync(All, default)).Current.AvgRating);
        As(Manager);
        Assert.Equal(4, (await Analytics.OverviewAsync(All, default)).Current.AvgRating);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Overview_BuildingOfSomeoneElse_Throws()
    {
        var other = Db.Rooms.Single(r => r.Id == RoomB.Id).FloorId;
        var buildingB = Db.Floors.Single(f => f.Id == other).BuildingId;
        As(Staff);

        Assert.Equal("analytics.building_not_allowed",
            await ErrorCodeAsync(() => Analytics.OverviewAsync(new AnalyticsQuery { BuildingId = buildingB }, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Preventive_DoneExactlyAtTheDueTime_IsOnTime()
    {
        var onTime = await CaseAtAsync(CaseStatus.Completed);
        var late = await CaseAtAsync(CaseStatus.Completed);
        foreach (var (id, completedAfterDue) in new[] { (onTime, 0), (late, 1) })
        {
            var c = Db.Cases.Single(x => x.Id == id);
            c.TriggerWorkOrderId = 999; // a preventive work order (F2)
            c.DueAt = Clock.Now.AddHours(-2);
            c.CompletedAt = c.DueAt.Value.AddMinutes(completedAfterDue);
        }

        await Db.SaveChangesAsync();
        As(Manager);

        var preventive = (await Analytics.OverviewAsync(All, default)).Preventive;

        Assert.Equal(2, preventive.Due);
        Assert.Equal(1, preventive.OnTime);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task FirstResponse_ApprovedAfterThirtyMinutes()
    {
        var id = (await NewCaseAsync()).Id;
        Clock.Now = Clock.Now.AddMinutes(30);
        As(Staff);
        await Sut.ApproveAsync(id, Approval(), default);

        Assert.Equal(30, (await Analytics.OverviewAsync(All, default)).Current.AvgFirstResponseMinutes);
    }

    // ----- ANA-02 / ANA-03 -----

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Technicians_FewerThanFiveCases_AreFlagged()
    {
        await CompletedAsync(2);
        As(Manager);

        var row = Assert.Single(await Analytics.TechniciansAsync(All, default));

        Assert.Equal(Technician.Id, row.Technician.Id);
        Assert.Equal(1, row.Completed);
        Assert.True(row.FewCases);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Failures_CountsByServiceType()
    {
        await NewCaseAsync();
        await NewCaseAsync();
        As(Staff);

        var failures = await Analytics.FailuresAsync(All, default);

        Assert.Equal(2, Assert.Single(failures.ByServiceType).Cases);
    }

    // ----- attention -----

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Attention_OverdueCase_IsListedFromTheFirstMinutePastTheDeadline()
    {
        await CaseAtAsync(CaseStatus.InProgress); // due in 24 h
        As(Staff);

        Clock.Now = Clock.Now.AddHours(24);
        Assert.DoesNotContain(await Analytics.AttentionAsync(default), i => i.Kind == AttentionKind.OverdueCase);

        Clock.Now = Clock.Now.AddMinutes(1);
        Assert.Contains(await Analytics.AttentionAsync(default), i => i.Kind == AttentionKind.OverdueCase);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Attention_DeclinedAndNobodyChosenAgain_IsListedNewestFirst()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Technician);
        await Sut.DeclineAsync(id, new ReasonRequest("Fully booked with another job"), default);
        As(Manager);

        var first = (await Analytics.AttentionAsync(default))[0];

        Assert.Equal(AttentionKind.NoTechnician, first.Kind);
        Assert.Equal(id, first.CaseId);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Attention_AcknowledgedEmergency_IsNotListedAsWaiting()
    {
        var id = (await NewCaseAsync()).Id;
        As(Staff);
        await Sut.ApproveAsync(id, Approval(CasePriority.Emergency), default); // the staff decided it: already acknowledged

        Assert.DoesNotContain(await Analytics.AttentionAsync(default), i => i.Kind == AttentionKind.EmergencyNotAcknowledged);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Attention_OnHoldForMoreThanThreeDays_IsListed()
    {
        await CaseAtAsync(CaseStatus.OnHold);
        Clock.Now = Clock.Now.AddDays(3).AddMinutes(1);
        As(Manager);

        Assert.Contains(await Analytics.AttentionAsync(default), i => i.Kind == AttentionKind.LongHold);
    }

    // ----- ANA-04 -----

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Export_Overview_IsAWorkbookWithTheTabAndTheCases()
    {
        var id = (await NewCaseAsync()).Id;
        Db.Cases.Single(c => c.Id == id).Title = "AC, room 501";
        await Db.SaveChangesAsync();
        As(Staff);

        var (name, content) = await Analytics.ExportAsync(new ExportQuery { Tab = ExportTab.Overview }, default);
        using var book = new XLWorkbook(new MemoryStream(content));

        Assert.EndsWith(".xlsx", name);
        Assert.Equal(["Overview", "By building", "Time on hold", "Cases"], book.Worksheets.Select(w => w.Name));
        Assert.Equal("AC, room 501", book.Worksheet("Cases").Cell(2, 2).GetString());
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void SortTechnicians_ByOnTimeDescending_PutsEmptyRatesLast()
    {
        TechnicianRow Row(string name, double? onTime) =>
            new(new SmartMaint.Api.Modules.Maintenance.Dtos.PersonRef(name.Length, name), 1, null, onTime, null, null, 0, 0, 0, [], true);
        var rows = new[] { Row("A", 0.5), Row("BB", null), Row("CCC", 0.9) };

        var sorted = AnalyticsService.SortTechnicians(rows, "onTime", descending: true);

        Assert.Equal(["CCC", "A", "BB"], sorted.Select(r => r.Technician.FullName));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void SortTechnicians_UnknownColumn_KeepsTheServerOrder()
    {
        var rows = new[]
        {
            new TechnicianRow(new SmartMaint.Api.Modules.Maintenance.Dtos.PersonRef(1, "Z"), 3, null, null, null, null, 0, 0, 0, [], true),
            new TechnicianRow(new SmartMaint.Api.Modules.Maintenance.Dtos.PersonRef(2, "A"), 1, null, null, null, null, 0, 0, 0, [], true),
        };

        Assert.Equal(["Z", "A"], AnalyticsService.SortTechnicians(rows, "nonsense", true).Select(r => r.Technician.FullName));
    }

    // ----- charts -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chart_CasesReportedPerDay_CountsEachDayAndSplitsByBuilding()
    {
        await NewCaseAsync();
        await NewCaseAsync();
        As(Manager);
        var today = DateOnly.FromDateTime(Clock.Now.ToOffset(TimeSpan.FromHours(7)).DateTime);

        var chart = await Analytics.ChartAsync(new ChartQuery
        {
            From = today.AddDays(-2), To = today, Metric = ChartMetric.CasesReported, Step = ChartStep.Day,
        }, default);

        Assert.Equal([0d, 0d, 2d], chart.Points.Select(p => p.Value ?? -1));
        Assert.Equal("Building", chart.BreakdownBy);
        Assert.Equal(2, Assert.Single(chart.Breakdown).Value);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Chart_ByMonth_FirstAndLastMonthAreCutToThePeriod()
    {
        As(Manager);

        var chart = await Analytics.ChartAsync(new ChartQuery
        {
            From = new DateOnly(2026, 9, 20), To = new DateOnly(2026, 11, 3), Metric = ChartMetric.CasesCompleted, Step = ChartStep.Month,
        }, default);

        Assert.Equal([new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 1)], chart.Points.Select(p => p.Start));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Chart_BuildingOfSomeoneElse_Throws()
    {
        var buildingB = Db.Floors.Single(f => f.Id == Db.Rooms.Single(r => r.Id == RoomB.Id).FloorId).BuildingId;
        As(Manager);

        Assert.Equal("analytics.building_not_allowed", await ErrorCodeAsync(() =>
            Analytics.ChartAsync(new ChartQuery { BuildingId = buildingB, Metric = ChartMetric.CostTotal }, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Resolution_CountsFromTheStartOfWorkNotFromTheReport()
    {
        var id = (await NewCaseAsync()).Id;
        Clock.Now = Clock.Now.AddHours(10);              // waits 10 h for the staff
        As(Staff);
        await Sut.ApproveAsync(id, Approval(), default);
        As(Technician);
        await Sut.AcceptAsync(id, default);
        await Sut.StartAsync(id, default);
        Clock.Now = Clock.Now.AddHours(3);
        await Sut.CompleteAsync(id, new CompleteWorkRequest(), default);
        As(Manager);

        Assert.Equal(3, (await Analytics.OverviewAsync(All, default)).Current.AvgResolutionHours);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void QueryValidator_EndBeforeStart_Fails()
    {
        var result = new AnalyticsQueryValidator().Validate(
            new AnalyticsQuery { From = new DateOnly(2026, 10, 10), To = new DateOnly(2026, 10, 1) });

        Assert.False(result.IsValid);
    }
}
