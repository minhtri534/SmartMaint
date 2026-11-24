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
        new("Pham Minh Duc", "duc@test.local", "+84 901 234 567", UserRole.Technician, 80000m, null, null);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void Create_ValidTechnician_Passes() => Assert.True(_create.Validate(Valid()).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void Create_CustomerNeedsNoRoom_Passes() =>
        Assert.True(_create.Validate(Valid() with { Role = UserRole.Customer }).IsValid);

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void Create_BadEmail_Fails()
    {
        var result = _create.Validate(Valid() with { Email = "not-an-email" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAccountRequest.Email));
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a@b.c", true)]
    public void Create_EmailIsRequired(string? email, bool valid) =>
        Assert.Equal(valid, _create.Validate(Valid() with { Email = email }).IsValid);

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(720, true)]
    [InlineData(721, false)]
    public void UpdateSla_HoursMustBeBetweenOneAnd720(int hours, bool valid) =>
        Assert.Equal(valid, new UpdateSlaRequestValidator().Validate(new UpdateSlaRequest(hours)).IsValid);
}
