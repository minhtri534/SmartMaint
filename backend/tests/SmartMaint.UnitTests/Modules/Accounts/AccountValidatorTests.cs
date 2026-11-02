using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Accounts.Dtos;
using SmartMaint.Api.Modules.Accounts.Validators;
using SmartMaint.Api.Modules.PriorityLevels.Dtos;
using SmartMaint.Api.Modules.PriorityLevels.Validators;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Accounts;

public sealed class AccountValidatorTests
{
    private readonly CreateAccountRequestValidator _create = new();

    private static CreateAccountRequest Valid() =>
        new("duc", "Pham Minh Duc", "duc@test.local", "+84 901 234 567", UserRole.Technician, null, 80000m, null, null);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void Create_ValidTechnician_Passes() => Assert.True(_create.Validate(Valid()).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void Create_CustomerWithoutRoom_Fails()
    {
        var result = _create.Validate(Valid() with { Role = UserRole.Customer });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAccountRequest.RoomId));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void Create_UsernameWithSpaceAndBadEmail_Fails()
    {
        var result = _create.Validate(Valid() with { Username = "pham duc", Email = "not-an-email" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAccountRequest.Username));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAccountRequest.Email));
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData("ab", false)]
    [InlineData("abc", true)]
    public void Create_UsernameLength_ThreeIsTheMinimum(string username, bool valid) =>
        Assert.Equal(valid, _create.Validate(Valid() with { Username = username }).IsValid);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(720, true)]
    [InlineData(721, false)]
    public void UpdateSla_HoursMustBeBetweenOneAnd720(int hours, bool valid) =>
        Assert.Equal(valid, new UpdateSlaRequestValidator().Validate(new UpdateSlaRequest(hours)).IsValid);
}
