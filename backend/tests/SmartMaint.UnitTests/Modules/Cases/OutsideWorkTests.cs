using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Assets.Dtos;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Cases.Validators;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>F5: assessment, decision board, outside party (warranty, contract, quote), check / redo, replacement, survey.</summary>
public sealed class OutsideWorkTests : CaseTestBase
{
    private static readonly DateTimeOffset Tomorrow = new(2026, 10, 29, 9, 0, 0, TimeSpan.FromHours(7));

    private ReplacementService Replacements => new(Db, Me, new BuildingScope(Db, Me), new AuditService(Db, Me, Clock), Notifications, Clock);

    private bool Told(Account user, string text) => Db.Notifications.Any(n => n.UserId == user.Id && n.Details.Contains(text));

    private static RequestDecisionRequest Assessment(TechnicianRecommendation recommendation = TechnicianRecommendation.Replace) =>
        new("Compressor bearing worn, trips after ten minutes", "Wear after years of use", recommendation, Photo);

    private static OutsourceRequest Contractor(OutsideWorkKind kind = OutsideWorkKind.Contract, OutsideWorkJob job = OutsideWorkJob.Repair) =>
        new(kind, job, "Cool Air Co.", "Mr Minh 0901 234 567", Tomorrow, null);

    /// <summary>A case on the AC of room 501, in progress, then waiting for the staff's decision.</summary>
    private async Task<long> WaitingDecisionAsync()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        Db.Cases.Single(c => c.Id == id).AssetId = AcInRoomA.Id;
        await Db.SaveChangesAsync();
        As(Technician);
        await Sut.RequestDecisionAsync(id, Assessment(), default);
        As(Staff);
        return id;
    }

    /// <summary>The outside party is working on the case.</summary>
    private async Task<long> WithOutsidePartyAsync(OutsideWorkJob job = OutsideWorkJob.Repair)
    {
        var id = await WaitingDecisionAsync();
        await Sut.OutsourceAsync(id, Contractor(job: job), default);
        return id;
    }

    // ----- assessment -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task RequestDecision_Technician_WaitsForTheStaffWithTheAssessment()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);

        var result = await Sut.RequestDecisionAsync(id, Assessment(), default);

        Assert.Equal(CaseStatus.OnHold, result.Status);
        Assert.Equal(HoldReason.WaitingStaffDecision, result.HoldReason);
        Assert.Equal(TechnicianRecommendation.Replace, result.Assessment!.Recommendation);
        Assert.Contains(result.Photos, p => p.Kind == CaseImageKind.Assessment);
        Assert.DoesNotContain(CaseAction.Resume, result.AllowedActions); // the staff decide, not the technician
        Assert.True(Told(Staff, "needs your decision"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void RequestDecisionValidator_WithoutPhoto_Fails()
    {
        var result = new RequestDecisionRequestValidator().Validate(Assessment() with { Photos = [] });

        Assert.Contains(result.Errors, e => e.PropertyName == "Photos");
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void HoldValidator_WaitingForStaffDecision_IsNotAPlainHold()
    {
        var result = new HoldCaseRequestValidator(Clock).Validate(
            new HoldCaseRequest(HoldReason.WaitingStaffDecision, DateOnly.FromDateTime(Clock.Now.UtcDateTime)));

        Assert.False(result.IsValid);
    }

    // ----- decision board -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task DecisionInfo_ThirdRepairInAYearOutOfWarranty_SuggestsReplacing()
    {
        for (var i = 0; i < 2; i++)
        {
            Db.Cases.Single(c => c.Id == (CaseAtAsync(CaseStatus.Closed).GetAwaiter().GetResult())).AssetId = AcInRoomA.Id;
        }

        await Db.SaveChangesAsync();
        var id = await WaitingDecisionAsync();

        var info = (await Sut.GetAsync(id, default)).DecisionInfo!;

        Assert.Equal(3, info.RepairsLast12Months);
        Assert.True(info.SuggestReplace);
        Assert.Contains(info.Reasons, r => r.Contains("3 repairs"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task DecisionInfo_WarrantyEndsToday_SendsToTheManufacturerInstead()
    {
        AcInRoomA.WarrantyEndDate = DateOnly.FromDateTime(Clock.Now.ToOffset(TimeSpan.FromHours(7)).DateTime);
        await Db.SaveChangesAsync();
        var id = await WaitingDecisionAsync();

        var info = (await Sut.GetAsync(id, default)).DecisionInfo!;

        Assert.True(info.UnderWarranty);
        Assert.False(info.SuggestReplace);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task DecisionInfo_Customer_IsNotShown()
    {
        var id = await WaitingDecisionAsync();
        As(Customer);

        Assert.Null((await Sut.GetAsync(id, default)).DecisionInfo);
    }

    // ----- outside party -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Outsource_Contract_WaitsForTheOutsidePartyAndTellsTheTechnician()
    {
        var id = await WaitingDecisionAsync();

        var result = await Sut.OutsourceAsync(id, Contractor(), default);

        Assert.Equal(HoldReason.WaitingThirdParty, result.HoldReason);
        Assert.Equal(OutsideWorkStatus.Working, Assert.Single(result.ThirdParties).Status);
        Assert.True(Told(Technician, "You are the contact person"));
        Assert.DoesNotContain(CaseAction.Outsource, result.AllowedActions); // one party at a time
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Quote_CustomerPaysAndAccepts_PartyWorksAndTheCustomerPays()
    {
        var id = await WaitingDecisionAsync();
        await Sut.OutsourceAsync(id, Contractor(OutsideWorkKind.Quote), default);
        await Sut.RecordQuoteAsync(id, new RecordQuoteRequest(CostBearer.Customer, "Ms Lan, office manager", null), default);

        var result = await Sut.DecideQuoteAsync(id, new QuoteDecisionRequest(true, "Ms Lan, office manager", "Email of 29/10", Photo[0]), default);

        var party = Assert.Single(result.ThirdParties);
        Assert.Equal(OutsideWorkStatus.Working, party.Status);
        Assert.NotNull(party.EvidenceUrl);
        Assert.Equal(CostBearer.Customer, result.CostBearer);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Quote_Declined_TheStaffChooseAgain()
    {
        var id = await WaitingDecisionAsync();
        await Sut.OutsourceAsync(id, Contractor(OutsideWorkKind.Quote), default);
        await Sut.RecordQuoteAsync(id, new RecordQuoteRequest(CostBearer.Building, null, null), default);

        var result = await Sut.DecideQuoteAsync(id, new QuoteDecisionRequest(false, "Building director", "Too expensive", null), default);

        Assert.Equal(OutsideWorkStatus.Declined, Assert.Single(result.ThirdParties).Status);
        Assert.Equal(HoldReason.WaitingStaffDecision, result.HoldReason);
        Assert.Contains(CaseAction.Outsource, result.AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void RecordQuoteValidator_CustomerPaysWithoutRecipient_Fails()
    {
        Assert.False(new RecordQuoteRequestValidator().Validate(new RecordQuoteRequest(CostBearer.Customer, null, null)).IsValid);
        Assert.True(new RecordQuoteRequestValidator().Validate(new RecordQuoteRequest(CostBearer.Building, null, null)).IsValid);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task QuoteReminder_ThreeDaysWithoutAnswer_RemindsTheStaffOnce()
    {
        var id = await WaitingDecisionAsync();
        await Sut.OutsourceAsync(id, Contractor(OutsideWorkKind.Quote), default);
        await Sut.RecordQuoteAsync(id, new RecordQuoteRequest(CostBearer.Customer, "Ms Lan", null), default);
        var reminder = new SlaReminder(Db, Notifications, Clock);
        Me.IsAuthenticated = false;

        Clock.Now = Clock.Now.AddDays(3).AddMinutes(-1);
        await reminder.RunOnceAsync(default);
        Assert.False(Told(Staff, "has not answered the quote"));

        Clock.Now = Clock.Now.AddMinutes(1);
        await reminder.RunOnceAsync(default);
        await reminder.RunOnceAsync(default);
        Assert.Single(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("has not answered the quote"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Check_FailedThenRedoneThenOk_BackToTheTechnician()
    {
        var id = await WithOutsidePartyAsync();
        As(Technician);
        await Sut.CheckOutsideWorkAsync(id, new CheckOutsideWorkRequest(false, "Still trips after ten minutes"), default);
        As(Staff);
        var redo = await Sut.RedoOutsideWorkAsync(id, new RedoOutsideWorkRequest(null, "Check the capacitor too"), default);
        Assert.Equal(1, Assert.Single(redo.ThirdParties).RedoCount);
        As(Technician);

        var result = await Sut.CheckOutsideWorkAsync(id, new CheckOutsideWorkRequest(true, null), default);

        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Null(result.HoldReason);
        Assert.Equal(OutsideWorkStatus.Done, Assert.Single(result.ThirdParties).Status);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task StaffResume_WhileAPartyWorks_StopsTheParty()
    {
        var id = await WithOutsidePartyAsync();

        var result = await Sut.ResumeAsync(id, default);

        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Equal(OutsideWorkStatus.Cancelled, Assert.Single(result.ThirdParties).Status);
    }

    // ----- replacement -----

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Complete_AfterAReplacementWithoutTheNewModel_Throws()
    {
        var id = await WithOutsidePartyAsync(OutsideWorkJob.Replace);
        As(Technician);
        await Sut.CheckOutsideWorkAsync(id, new CheckOutsideWorkRequest(true, null), default);

        Assert.Equal("case.replacement_model_required", await ErrorCodeAsync(() =>
            Sut.CompleteAsync(id, new CompleteWorkRequest("Compressor worn out", null, null, null, Photo), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Replacement_ClosedCase_AdminRegistersTheNewAssetAndRetiresTheOld()
    {
        var id = await WithOutsidePartyAsync(OutsideWorkJob.Replace);
        As(Technician);
        await Sut.CheckOutsideWorkAsync(id, new CheckOutsideWorkRequest(true, null), default);
        await Sut.CompleteAsync(id, new CompleteWorkRequest("Compressor worn out", null, null, null, Photo,
            ReplacementModel: "Daikin FTKF35", ReplacementSerial: "SN-123"), default);
        As(Staff);
        await Sut.CloseAsync(id, default);
        As(Admin);
        var request = Assert.Single(await Replacements.ListAsync(default));
        Assert.Null(request.CaseId); // BR-11: no link into the case for the Administrator

        var done = await Replacements.CompleteAsync(request.Id,
            new CompleteReplacementRequest("AC 501 Daikin FTKF35", AcInRoomA.TypeId, null, false, null), default);

        Assert.Equal(ReplacementStatus.Done, done.Status);
        var created = Db.Assets.Single(a => a.Id == done.NewAsset!.Id);
        Assert.Equal(AcInRoomA.Id, created.ReplacesAssetId);
        Assert.NotNull(Db.Assets.Single(a => a.Id == AcInRoomA.Id).RetiredAt);
        Assert.True(Told(Staff, "is registered in place of"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task ReplacementComplete_NameAlreadyInTheRoom_Throws()
    {
        var id = await WithOutsidePartyAsync(OutsideWorkJob.Replace);
        As(Technician);
        await Sut.CheckOutsideWorkAsync(id, new CheckOutsideWorkRequest(true, null), default);
        await Sut.CompleteAsync(id, new CompleteWorkRequest("Compressor worn out", null, null, null, Photo, ReplacementModel: "Daikin"), default);
        As(Staff);
        await Sut.CloseAsync(id, default);
        As(Admin);
        var request = (await Replacements.ListAsync(default)).Single();

        Assert.Equal("asset.name_taken", await ErrorCodeAsync(() => Replacements.CompleteAsync(request.Id,
            new CompleteReplacementRequest(AcInRoomA.Name, AcInRoomA.TypeId, null, false, null), default)));
    }

    // ----- survey (way 2) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Survey_Staff_OpensAnApprovedCaseWithoutCustomer()
    {
        As(Staff);

        var result = await Sut.OpenSurveyAsync(new OpenSurveyRequest(AcInRoomA.Id, NotCooling.Id, null), default);

        Assert.True(result.IsSurvey);
        Assert.Equal(CaseStatus.Approved, result.Status);
        Assert.Null(result.Requester);
        Assert.Contains(CaseAction.Assign, result.AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Survey_SecondOneOrOtherBuilding_Throws()
    {
        As(Staff);
        await Sut.OpenSurveyAsync(new OpenSurveyRequest(AcInRoomA.Id, NotCooling.Id, null), default);

        Assert.Equal("case.survey_not_allowed", await ErrorCodeAsync(() =>
            Sut.OpenSurveyAsync(new OpenSurveyRequest(AcInRoomA.Id, NotCooling.Id, null), default)));
        Assert.Equal("case.survey_not_allowed", await ErrorCodeAsync(() =>
            Sut.OpenSurveyAsync(new OpenSurveyRequest(AssetInRoomB.Id, NotCooling.Id, null), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Survey_IsNotCountedAsARepair()
    {
        As(Staff);
        await Sut.OpenSurveyAsync(new OpenSurveyRequest(AcInRoomA.Id, NotCooling.Id, null), default);

        Assert.Equal(0, await AssetRepairStats.RepairsSinceAsync(Db, AcInRoomA.Id, Clock.Now.AddYears(-1), default));
    }
}
