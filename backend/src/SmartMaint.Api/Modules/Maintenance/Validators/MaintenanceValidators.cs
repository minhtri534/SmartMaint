using FluentValidation;
using SmartMaint.Api.Modules.Maintenance.Dtos;

namespace SmartMaint.Api.Modules.Maintenance.Validators;

// Same rules as the web forms (frontend/src/features/maintenance/schemas.ts).

public sealed class ChecklistRequestValidator : AbstractValidator<ChecklistRequest>
{
    public const int MaxItems = 50;

    public ChecklistRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Must(n => n.Trim().Length >= 3).WithMessage("At least 3 characters.").MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Tasks).NotEmpty().WithMessage("Add at least one item.")
            .Must(t => t.Count <= MaxItems).WithMessage($"At most {MaxItems} items.");
        RuleForEach(x => x.Tasks).ChildRules(t =>
        {
            t.RuleFor(x => x.Name).NotEmpty().Must(n => n.Trim().Length >= 3).WithMessage("At least 3 characters.").MaximumLength(200);
            t.RuleFor(x => x.Description).MaximumLength(500);
        });
    }
}

public sealed class PlanRequestValidator : AbstractValidator<PlanRequest>
{
    public const int MaxAssets = 100;
    public const int MaxTriggers = 10;

    /// <summary>"First due day not in the past" is checked by the service: an unchanged day of a running plan is fine.</summary>
    public PlanRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Must(n => n.Trim().Length >= 3).WithMessage("At least 3 characters.").MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.ChecklistId).GreaterThan(0).WithMessage("Choose a checklist.");
        RuleFor(x => x.EstimatedHours).InclusiveBetween(0.25m, 24m).When(x => x.EstimatedHours is not null)
            .WithMessage("Between 0.25 and 24 hours.");
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Triggers).NotEmpty().WithMessage("Add at least one trigger.")
            .Must(t => t.Count <= MaxTriggers).WithMessage($"At most {MaxTriggers} triggers.")
            .Must(t => t.Where(x => x.Id is not null).Select(x => x.Id).Distinct().Count() == t.Count(x => x.Id is not null))
            .WithMessage("A trigger is listed twice.")
            .Must(t => t.Sum(x => x.Assets.Count) <= MaxAssets).WithMessage($"At most {MaxAssets} assets in a plan.")
            // one template for every trigger: the same asset twice would get the same work order twice
            .Must(t => t.SelectMany(x => x.Assets).Select(a => a.AssetId).Distinct().Count() == t.Sum(x => x.Assets.Count))
            .WithMessage("An asset is in two triggers, or listed twice.");
        RuleForEach(x => x.Triggers).ChildRules(t =>
        {
            t.RuleFor(x => x.Frequency).InclusiveBetween(1, 365).WithMessage("Between 1 and 365.");
            t.RuleFor(x => x.FrequencyType).IsInEnum();
            t.RuleFor(x => x.LeadDays).InclusiveBetween(0, 30).WithMessage("Between 0 and 30 days.");
            t.RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate is not null)
                .WithMessage("The end day cannot be before the first due day.");
            t.RuleFor(x => x.Assets).NotEmpty().WithMessage("Choose at least one asset.");
        });
    }
}

public sealed class RejectPlanRequestValidator : AbstractValidator<RejectPlanRequest>
{
    public RejectPlanRequestValidator() =>
        RuleFor(x => x.Reason).NotEmpty().Must(r => r.Trim().Length >= 10).WithMessage("At least 10 characters.").MaximumLength(500);
}
