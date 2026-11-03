using FluentValidation;
using SmartMaint.Api.Modules.Analytics.Dtos;

namespace SmartMaint.Api.Modules.Analytics.Validators;

public sealed class AnalyticsQueryValidator : AbstractValidator<AnalyticsQuery>
{
    public const int MaxDays = 366;

    public AnalyticsQueryValidator()
    {
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).When(x => x.From is not null && x.To is not null)
            .WithMessage("The end day must not be before the start day.");
        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.To.Value.DayNumber - x.From.Value.DayNumber < MaxDays)
            .WithName("To").WithMessage($"At most {MaxDays} days.");
        RuleFor(x => x.BuildingId).GreaterThan(0).When(x => x.BuildingId is not null);
    }
}
