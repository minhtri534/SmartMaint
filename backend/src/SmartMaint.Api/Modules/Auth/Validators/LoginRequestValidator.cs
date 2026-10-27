using FluentValidation;
using SmartMaint.Api.Modules.Auth.Dtos;

namespace SmartMaint.Api.Modules.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public const int EmailMaxLength = 254;

    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(EmailMaxLength).EmailAddress();
        // Only presence is checked here: the password policy applies when a password is set, not on sign-in.
        RuleFor(x => x.Password).NotEmpty();
    }
}
