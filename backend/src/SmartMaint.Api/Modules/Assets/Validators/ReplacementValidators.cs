using FluentValidation;
using SmartMaint.Api.Modules.Assets.Dtos;

namespace SmartMaint.Api.Modules.Assets.Validators;

// Same rules as the web forms (frontend/src/features/assets/schemas.ts).

public sealed class CompleteReplacementRequestValidator : AbstractValidator<CompleteReplacementRequest>
{
    public CompleteReplacementRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Must(n => n.Trim().Length >= 2).WithMessage("At least 2 characters.")
            .MaximumLength(150);
        RuleFor(x => x.TypeId).GreaterThan(0).WithMessage("Choose the asset type.");
        RuleFor(x => x.ManufacturerId).GreaterThan(0).When(x => x.ManufacturerId is not null);
    }
}

public sealed class DismissReplacementRequestValidator : AbstractValidator<DismissReplacementRequest>
{
    public DismissReplacementRequestValidator() =>
        RuleFor(x => x.Reason).NotEmpty().Must(r => r.Trim().Length >= 10).WithMessage("At least 10 characters.")
            .MaximumLength(500);
}
