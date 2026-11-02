using FluentValidation;
using SmartMaint.Api.Modules.Auth.Dtos;

namespace SmartMaint.Api.Modules.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public const int UsernameMaxLength = 50;

    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(UsernameMaxLength);
        // Only presence is checked here: the password policy applies when a password is set, not on sign-in.
        RuleFor(x => x.Password).NotEmpty();
    }
}
