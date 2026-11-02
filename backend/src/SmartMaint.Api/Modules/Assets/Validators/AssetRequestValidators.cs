using FluentValidation;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Assets.Dtos;

namespace SmartMaint.Api.Modules.Assets.Validators;

// Same rules as the web form (frontend/src/features/assets/schemas.ts).

public sealed class AssetRequestValidator : AbstractValidator<AssetRequest>
{
    public AssetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Must(n => n.Trim().Length >= 2).WithMessage("At least 2 characters.")
            .MaximumLength(150);
        RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("Choose the room of the asset.");
        RuleFor(x => x.TypeId).GreaterThan(0).WithMessage("Choose the asset type.");
        RuleFor(x => x.ManufacturerId).GreaterThan(0).When(x => x.ManufacturerId is not null);
    }
}

public sealed class AssetListQueryValidator : AbstractValidator<AssetListQuery>
{
    public AssetListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(100);
    }
}

public sealed class ImportAssetsRequestValidator : AbstractValidator<ImportAssetsRequest>
{
    /// <summary>About 1 MB of text — far more than 1000 rows need.</summary>
    public const int MaxLength = 1_000_000;

    public ImportAssetsRequestValidator() =>
        RuleFor(x => x.Csv).NotEmpty().WithMessage("Choose a CSV file.").MaximumLength(MaxLength).WithMessage("The file is too large.");
}
