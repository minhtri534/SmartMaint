using FluentValidation;
using SmartMaint.Api.Modules.CatalogAdmin.Dtos;

namespace SmartMaint.Api.Modules.CatalogAdmin.Validators;

// Same rules as the web dialogs (frontend/src/features/catalog/schemas.ts).

internal static class CatalogRules
{
    public static IRuleBuilderOptions<T, string> ValidCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(20).Matches("^[A-Za-z0-9-]+$").WithMessage("Letters, digits and dash only, e.g. AC-SPLIT.");

    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule, int max) =>
        rule.NotEmpty().Must(n => n.Trim().Length >= 2).WithMessage("At least 2 characters.").MaximumLength(max);
}

public sealed class CaseCategoryRequestValidator : AbstractValidator<CaseCategoryRequest>
{
    public CaseCategoryRequestValidator()
    {
        RuleFor(x => x.GroupName).ValidName(60);
        RuleFor(x => x.Name).ValidName(100);
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.DefaultPriority).IsInEnum();
        RuleFor(x => x.DefaultCostBearer).IsInEnum();
    }
}

public sealed class AssetCategoryRequestValidator : AbstractValidator<AssetCategoryRequest>
{
    public AssetCategoryRequestValidator()
    {
        RuleFor(x => x.Code).ValidCode();
        RuleFor(x => x.CategoryName).ValidName(100);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public sealed class AssetTypeRequestValidator : AbstractValidator<AssetTypeRequest>
{
    public AssetTypeRequestValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Choose an asset category.");
        RuleFor(x => x.Code).ValidCode();
        RuleFor(x => x.Name).ValidName(100);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public sealed class ManufacturerRequestValidator : AbstractValidator<ManufacturerRequest>
{
    public ManufacturerRequestValidator()
    {
        RuleFor(x => x.Name).ValidName(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.Email).MaximumLength(254)
            .Must(e => string.IsNullOrWhiteSpace(e) || System.Net.Mail.MailAddress.TryCreate(e.Trim(), out _))
            .WithMessage("Enter a valid e-mail address.");
    }
}

public sealed class SpecialtyRequestValidator : AbstractValidator<SpecialtyRequest>
{
    public SpecialtyRequestValidator() => RuleFor(x => x.Name).ValidName(60);
}
