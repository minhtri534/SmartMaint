using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Cases.Validators;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>The request validators: each limit is tested one step inside and one step outside (Boundary).</summary>
public sealed class CaseValidatorTests
{
    private static readonly FixedTimeProvider Clock = new(new DateTimeOffset(2026, 10, 28, 8, 0, 0, TimeSpan.Zero));
    private static readonly IReadOnlyList<string> Photo = ["data:image/jpeg;base64,AAAA"];

    private static CreateCaseRequest Create(string title = "Air conditioner is warm", string? description = null, IReadOnlyList<string>? photos = null) =>
        new(title, description ?? new string('d', 20), 1, 1, null, photos);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(120, true)]
    [InlineData(121, false)]
    public void CreateCase_TitleLength_BoundaryAtFiveAndOneHundredTwenty(int length, bool valid) =>
        Assert.Equal(valid, new CreateCaseRequestValidator().Validate(Create(title: new string('t', length))).IsValid);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(14, false)]
    [InlineData(15, true)]
    [InlineData(2000, true)]
    [InlineData(2001, false)]
    public void CreateCase_DescriptionLength_BoundaryAtFifteenAndTwoThousand(int length, bool valid) =>
        Assert.Equal(valid, new CreateCaseRequestValidator().Validate(Create(description: new string('d', length))).IsValid);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void CreateCase_PhotoCount_AtMostFive(int count, bool valid) =>
        Assert.Equal(valid, new CreateCaseRequestValidator().Validate(Create(photos: Enumerable.Repeat(Photo[0], count).ToList())).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void CreateCase_PhotoThatIsNotAnImage_IsInvalid() =>
        Assert.False(new CreateCaseRequestValidator().Validate(Create(photos: ["javascript:alert(1)"])).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void CreateCase_UrgencyOutsideTheList_IsInvalid() =>
        Assert.False(new CreateCaseRequestValidator().Validate(Create() with { Urgency = (CasePriority)99 }).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void ApproveCase_CategoryZero_IsInvalidButEmptyKeepsTheOldOne()
    {
        var validator = new ApproveCaseRequestValidator();
        Assert.False(validator.Validate(new ApproveCaseRequest(CasePriority.Normal, 1, ServiceTypeId: 0)).IsValid);
        Assert.True(validator.Validate(new ApproveCaseRequest(CasePriority.Normal, 1)).IsValid);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void ApproveCase_WithoutTechnician_IsInvalid() =>
        Assert.False(new ApproveCaseRequestValidator().Validate(new ApproveCaseRequest(CasePriority.Normal, 0)).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void ApproveCase_PlannedEndEqualToStart_IsInvalid()
    {
        var start = Clock.Now.AddHours(1);
        var validator = new ApproveCaseRequestValidator();
        Assert.False(validator.Validate(new ApproveCaseRequest(CasePriority.Normal, 1, PlannedStartAt: start, PlannedEndAt: start)).IsValid);
        Assert.True(validator.Validate(new ApproveCaseRequest(CasePriority.Normal, 1, PlannedStartAt: start, PlannedEndAt: start.AddMinutes(1))).IsValid);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void ApproveCase_TaskNames_TwoTo200Characters()
    {
        var validator = new ApproveCaseRequestValidator();
        Assert.False(validator.Validate(new ApproveCaseRequest(CasePriority.Normal, 1, Tasks: [new TaskLineRequest(null, "x", null)])).IsValid);
        Assert.True(validator.Validate(new ApproveCaseRequest(CasePriority.Normal, 1, Tasks: [new TaskLineRequest(null, "xy", null)])).IsValid);
        Assert.False(validator.Validate(new ApproveCaseRequest(CasePriority.Normal, 1, Tasks: [new TaskLineRequest(null, new string('t', 201), null)])).IsValid);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void EditCase_ReopenWithoutReason_IsInvalid()
    {
        var edit = new EditCaseRequest(CasePriority.Normal, 1, 1, null, null, [], "Air conditioner is warm",
            "The AC blows warm air since this morning.", StatusChange: EditStatusChange.Reopen);
        var validator = new EditCaseRequestValidator();
        Assert.False(validator.Validate(edit).IsValid);
        Assert.True(validator.Validate(edit with { Reason = "Still drips water onto the desk" }).IsValid);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(9, false)]
    [InlineData(10, true)]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void Reason_Length_BoundaryAtTenAndFiveHundred(int length, bool valid) =>
        Assert.Equal(valid, new ReasonRequestValidator().Validate(new ReasonRequest(new string('r', length))).IsValid);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(9, false)]
    [InlineData(10, true)]
    public void RejectNote_Length_BoundaryAtTen(int length, bool valid) =>
        Assert.Equal(valid, new RejectCaseRequestValidator().Validate(new RejectCaseRequest(RejectReason.MissingInfo, new string('n', length))).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void Reject_UnknownReasonValue_IsInvalid() =>
        Assert.False(new RejectCaseRequestValidator().Validate(new RejectCaseRequest((RejectReason)99, new string('n', 20))).IsValid);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData("0", false)]
    [InlineData("0.01", true)]
    [InlineData("200", true)]
    [InlineData("200.01", false)]
    public void LogHours_TypedTotal_MoreThanZeroAndAtMost200(string hours, bool valid) =>
        Assert.Equal(valid, new LogHoursRequestValidator().Validate(
            new LogHoursRequest(decimal.Parse(hours, System.Globalization.CultureInfo.InvariantCulture))).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void AddCost_WithReceiptNumberOnly_IsValid() =>
        Assert.True(new AddCostRequestValidator().Validate(new AddCostRequest("Capacitor", null, 150_000m, "R-102", null)).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void AddCost_WithoutAnyReceipt_IsInvalid() =>
        Assert.False(new AddCostRequestValidator().Validate(new AddCostRequest("Capacitor", null, 150_000m, " ", null)).IsValid);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void Rate_Stars_OneToFive(int stars, bool valid) =>
        Assert.Equal(valid, new RateCaseRequestValidator().Validate(new RateCaseRequest(stars, null)).IsValid);
}
