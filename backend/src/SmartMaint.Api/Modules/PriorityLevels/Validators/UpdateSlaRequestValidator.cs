using FluentValidation;
using SmartMaint.Api.Modules.PriorityLevels.Dtos;

namespace SmartMaint.Api.Modules.PriorityLevels.Validators;

public sealed class UpdateSlaRequestValidator : AbstractValidator<UpdateSlaRequest>
{
    public const int MinHours = 1;

    /// <summary>30 days.</summary>
    public const int MaxHours = 720;

    public UpdateSlaRequestValidator() =>
        RuleFor(x => x.SlaHours).InclusiveBetween(MinHours, MaxHours).WithMessage($"Between {MinHours} and {MaxHours} hours.");
}
