using FluentValidation;
using SmartMaint.Api.Common.Validation;
using SmartMaint.Api.Modules.Auth.Dtos;

namespace SmartMaint.Api.Modules.Auth.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .StrongPassword()
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must differ from the current one.");
    }
}
