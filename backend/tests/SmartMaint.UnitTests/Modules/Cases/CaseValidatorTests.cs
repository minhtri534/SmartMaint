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

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void Hold_ExpectedDay_TodayIsFineYesterdayIsNot()
    {
        var validator = new HoldCaseRequestValidator(Clock);
        var today = DateOnly.FromDateTime(Clock.Now.UtcDateTime);

        Assert.True(validator.Validate(new HoldCaseRequest(HoldReason.WaitingParts, today)).IsValid);
        Assert.False(validator.Validate(new HoldCaseRequest(HoldReason.WaitingParts, today.AddDays(-1))).IsValid);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData("0.24", false)]
    [InlineData("0.25", true)]
    [InlineData("24", true)]
    [InlineData("24.01", false)]
    public void CompleteWork_TypedHours_BetweenQuarterHourAndTwentyFour(string hours, bool valid) =>
        Assert.Equal(valid, new CompleteWorkRequestValidator().Validate(
            new CompleteWorkRequest("Worn part", null, decimal.Parse(hours, System.Globalization.CultureInfo.InvariantCulture), null, Photo)).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void CompleteWork_HoursLeftEmpty_IsValid_TheSystemMeasuresThem() =>
        Assert.True(new CompleteWorkRequestValidator().Validate(new CompleteWorkRequest("Worn part", null, null, null, Photo)).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void CompleteWork_NoPhotoAfterRepair_IsInvalid() =>
        Assert.False(new CompleteWorkRequestValidator().Validate(new CompleteWorkRequest("Worn part", null, null, null, [])).IsValid);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void Rate_Stars_OneToFive(int stars, bool valid) =>
        Assert.Equal(valid, new RateCaseRequestValidator().Validate(new RateCaseRequest(stars, null)).IsValid);
}
