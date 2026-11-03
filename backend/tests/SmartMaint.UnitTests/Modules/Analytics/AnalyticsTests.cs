using System.Text;
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
        await Sut.CompleteAsync(id, new CompleteWorkRequest("Worn part", null, 1, null, Photo), default);
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
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Overview_DeadlineMoved_BothRatesAreShown()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);
        await Sut.ExtendDueAsync(id, new ExtendDueRequest(Clock.Now.AddHours(48), ExtendDueReason.WaitingParts, null), default);
        Clock.Now = Clock.Now.AddHours(30);
        As(Technician);
        await Sut.CompleteAsync(id, new CompleteWorkRequest("Worn part", null, 1, null, Photo), default);
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
        await Sut.CloseAsync(id, default);
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
        await Sut.ApproveAsync(id, new ApproveCaseRequest(CasePriority.Normal, CostBearer.Building), default);

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
    public async Task Attention_OverdueMoreThanADay_IsListedOnlyAfter24Hours()
    {
        await CaseAtAsync(CaseStatus.InProgress); // due in 24 h
        As(Staff);

        Clock.Now = Clock.Now.AddHours(48);
        Assert.DoesNotContain(await Analytics.AttentionAsync(default), i => i.Kind == AttentionKind.OverdueCase);

        Clock.Now = Clock.Now.AddMinutes(1);
        Assert.Contains(await Analytics.AttentionAsync(default), i => i.Kind == AttentionKind.OverdueCase);
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
    public async Task Export_Cases_IsCsvWithBomAndQuotedCommas()
    {
        var id = (await NewCaseAsync()).Id;
        Db.Cases.Single(c => c.Id == id).Title = "AC, room 501";
        await Db.SaveChangesAsync();
        As(Staff);

        var (name, content) = await Analytics.ExportAsync(ExportReport.Cases, All, default);
        var text = Encoding.UTF8.GetString(content);

        Assert.EndsWith(".csv", name);
        Assert.Equal(Encoding.UTF8.GetPreamble(), content.Take(3).ToArray());
        Assert.Contains("\"AC, room 501\"", text);
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
