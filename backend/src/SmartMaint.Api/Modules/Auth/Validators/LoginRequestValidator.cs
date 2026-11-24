using FluentValidation;
using SmartMaint.Api.Modules.Auth.Dtos;

namespace SmartMaint.Api.Modules.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public const int LoginMaxLength = 254;

    public LoginRequestValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty().WithMessage("Enter your e-mail or phone number.")
            .MaximumLength(LoginMaxLength).OverridePropertyName(nameof(LoginRequest.Login));
        // Only presence is checked here: the password policy applies when a password is set, not on sign-in.
        RuleFor(x => x.Password).NotEmpty();
    }
}
