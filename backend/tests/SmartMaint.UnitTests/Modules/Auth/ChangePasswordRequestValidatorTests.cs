using SmartMaint.Api.Modules.Auth.Dtos;
using SmartMaint.Api.Modules.Auth.Validators;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Auth;

public sealed class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _sut = new();

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void Validate_StrongNewPassword_IsValid()
    {
        var result = _sut.Validate(new ChangePasswordRequest("Old@1234", "NewPass@456"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Abnormal)]
    [InlineData("alllowercase1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("NoDigitsHere")]
    public void Validate_PasswordMissingCharacterClass_IsInvalid(string newPassword)
    {
        var result = _sut.Validate(new ChangePasswordRequest("Old@1234", newPassword));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void Validate_NewPasswordSameAsCurrent_IsInvalid()
    {
        var result = _sut.Validate(new ChangePasswordRequest("Same@1234", "Same@1234"));

        Assert.False(result.IsValid);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData("Abcdef1", false)]  // 7 chars: one below the minimum
    [InlineData("Abcdef12", true)]  // 8 chars: exactly the minimum
    public void Validate_PasswordLengthAroundMinimum(string newPassword, bool expectedValid)
    {
        var result = _sut.Validate(new ChangePasswordRequest("Old@1234", newPassword));

        Assert.Equal(expectedValid, result.IsValid);
    }
}
