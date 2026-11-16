using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Assets.Dtos;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Cases.Validators;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>
/// F5: assessment, decision board, the hand-over to an outside party and its tasks (meeting 07/11), replacement, survey.
/// </summary>
public sealed class OutsideWorkTests : CaseTestBase
{
    private ReplacementService Replacements => new(Db, Me, new BuildingScope(Db, Me), new AuditService(Db, Me, Clock), Notifications, Clock);

    private bool Told(Account user, string text) => Db.Notifications.Any(n => n.UserId == user.Id && n.Details.Contains(text));

    private static RequestDecisionRequest Assessment() =>
        new("Compressor bearing worn, trips after ten minutes", "Needs a new compressor from the maker", Photo);

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

    // ----- assessment -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task RequestDecision_Technician_WaitsForTheStaffWithTheAssessment()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);

        var result = await Sut.RequestDecisionAsync(id, Assessment(), default);

        Assert.Equal(CaseStatus.OnHold, result.Status);
        Assert.Equal(HoldReason.WaitingStaffDecision, result.HoldReason);
        Assert.Equal("Needs a new compressor from the maker", result.Assessment!.Cause); // meeting 07/11: description + reason
        Assert.Null(result.Assessment.Recommendation);
        Assert.Contains(result.Photos, p => p.Kind == CaseImageKind.Assessment);
        Assert.DoesNotContain(CaseAction.Resume, result.AllowedActions); // the staff decide, not the technician
        Assert.True(Told(Staff, "asks to hand"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void RequestDecisionValidator_WithoutPhoto_Fails()
    {
        var result = new RequestDecisionRequestValidator().Validate(Assessment() with { Photos = [] });

        Assert.Contains(result.Errors, e => e.PropertyName == "Photos");
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void RequestDecisionValidator_DescriptionOfTenCharacters_IsTheMinimum()
    {
        var validator = new RequestDecisionRequestValidator();

        Assert.False(validator.Validate(Assessment() with { Description = new string('d', 9) }).IsValid);
        Assert.True(validator.Validate(Assessment() with { Description = new string('d', 10) }).IsValid);
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

        var result = await Sut.GetAsync(id, default);

        Assert.Null(result.DecisionInfo);
        Assert.Null(result.Assessment); // meeting 07/11: how the work is done is not for the customer
    }

    // ----- outside party (meeting 07/11: only the hand-over is recorded, the party's tasks are marked) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Outsource_AfterTheAssessment_WaitsForTheOutsidePartyAndTellsTheTechnician()
    {
        var id = await WaitingDecisionAsync();

        var result = await Sut.OutsourceAsync(id, default);

        Assert.Equal(CaseStatus.OnHold, result.Status);
        Assert.Equal(HoldReason.WaitingThirdParty, result.HoldReason);
        Assert.Empty(result.ThirdParties); // nothing about the party itself is recorded
        Assert.True(Told(Technician, "handed to an outside party"));
        Assert.DoesNotContain(CaseAction.Outsource, result.AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Outsource_WhileInProgress_PutsTheCaseOnHold()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);

        var result = await Sut.OutsourceAsync(id, default);

        Assert.Equal(CaseStatus.OnHold, result.Status);
        Assert.Equal(HoldReason.WaitingThirdParty, result.HoldReason);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Outsource_BeforeTheTechnicianAccepted_Throws()
    {
        var id = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.invalid_transition", await ErrorCodeAsync(() => Sut.OutsourceAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SetTaskThirdParty_Staff_MarksItDoneByTheOutsideParty()
    {
        var id = await WaitingDecisionAsync();
        await Sut.OutsourceAsync(id, default);
        var taskId = (await Sut.GetAsync(id, default)).Tasks.Single().Id;

        var result = await Sut.SetTaskThirdPartyAsync(id, taskId, new SetTaskThirdPartyRequest(true), default);

        var task = Assert.Single(result.Tasks);
        Assert.Equal(TaskDoneBy.ThirdParty, task.DoneBy);
        Assert.Equal(CaseTaskStatus.Pass, task.Status);
        Assert.Equal(Clock.Now, task.DoneAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task UpdateTask_DoneByTheOutsideParty_TheTechnicianCannotChangeIt()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);
        var taskId = (await Sut.GetAsync(id, default)).Tasks.Single().Id;
        await Sut.SetTaskThirdPartyAsync(id, taskId, new SetTaskThirdPartyRequest(true), default);
        As(Technician);

        Assert.Equal("case.task_third_party", await ErrorCodeAsync(() => Sut.UpdateTaskAsync(id, taskId,
            new UpdateTaskRequest(CaseTaskStatus.Pass, null, null, null, false), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task SetTaskThirdParty_Unmarked_GoesBackToTheTechnician()
    {
        var id = await CaseAtAsync(CaseStatus.InProgress);
        As(Staff);
        var taskId = (await Sut.GetAsync(id, default)).Tasks.Single().Id;
        await Sut.SetTaskThirdPartyAsync(id, taskId, new SetTaskThirdPartyRequest(true), default);

        var result = await Sut.SetTaskThirdPartyAsync(id, taskId, new SetTaskThirdPartyRequest(false), default);

        var task = Assert.Single(result.Tasks);
        Assert.Null(task.DoneBy);
        Assert.Equal(CaseTaskStatus.Pending, task.Status);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task EditContinue_AfterTheOutsideParty_TheTechnicianGoesOn()
    {
        var id = await WaitingDecisionAsync();
        await Sut.OutsourceAsync(id, default);

        var result = await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Continue }, default);

        Assert.Equal(CaseStatus.InProgress, result.Status);
        Assert.Null(result.HoldReason);
    }

    // ----- replacement (an outside party of the old flow replaced the asset) -----

    /// <summary>The asset of an in-progress case was replaced by an outside party (a row of the old F5 flow).</summary>
    private async Task<long> ReplacedByOutsidePartyAsync()
    {
        var id = await WaitingDecisionAsync();
        Db.CaseThirdParties.Add(new CaseThirdParty
        {
            CaseId = id,
            Kind = OutsideWorkKind.Contract,
            Job = OutsideWorkJob.Replace,
            Status = OutsideWorkStatus.Done,
            CompanyName = "Cool Air Co.",
            Contact = "Mr Minh 0901 234 567",
            FinishedAt = Clock.Now,
        });
        await Db.SaveChangesAsync();
        await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Continue }, default);
        As(Technician);
        return id;
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Complete_AfterAReplacementWithoutTheNewModel_Throws()
    {
        var id = await ReplacedByOutsidePartyAsync();

        Assert.Equal("case.replacement_model_required", await ErrorCodeAsync(() =>
            Sut.CompleteAsync(id, new CompleteWorkRequest(), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Replacement_ClosedCase_AdminRegistersTheNewAssetAndRetiresTheOld()
    {
        var id = await ReplacedByOutsidePartyAsync();
        await Sut.CompleteAsync(id, new CompleteWorkRequest(
            ReplacementModel: "Daikin FTKF35", ReplacementSerial: "SN-123"), default);
        await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Close }, default);
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
        var id = await ReplacedByOutsidePartyAsync();
        await Sut.CompleteAsync(id, new CompleteWorkRequest(ReplacementModel: "Daikin"), default);
        await Sut.EditAsync(id, await EditOfAsync(id) with { StatusChange = EditStatusChange.Close }, default);
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
        Assert.Contains(CaseAction.Edit, result.AllowedActions); // the staff names the technician with Edit
        Assert.Single(result.Tasks);
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
