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
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Choose the service type.");
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

    /// <summary>"First due day not in the past" is checked by the service: an unchanged day of a running plan is fine.</summary>
    public PlanRequestValidator()
    {
        RuleFor(x => x.Frequency).InclusiveBetween(1, 365).WithMessage("Between 1 and 365.");
        RuleFor(x => x.FrequencyType).IsInEnum();
        RuleFor(x => x.LeadDays).InclusiveBetween(0, 30).WithMessage("Between 0 and 30 days.");
        RuleFor(x => x.ChecklistId).GreaterThan(0).WithMessage("Choose a checklist.");
        RuleFor(x => x.EstimatedHours).InclusiveBetween(0.25m, 24m).When(x => x.EstimatedHours is not null)
            .WithMessage("Between 0.25 and 24 hours.");
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Assets).NotEmpty().WithMessage("Choose at least one asset.")
            .Must(a => a.Count <= MaxAssets).WithMessage($"At most {MaxAssets} assets.")
            .Must(a => a.Select(x => x.AssetId).Distinct().Count() == a.Count).WithMessage("An asset is listed twice.");
        RuleForEach(x => x.Assets).ChildRules(a =>
        {
            a.RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).When(x => x.EndDate is not null)
                .WithMessage("The end day must be after the first due day.");
        });
    }
}

public sealed class RejectPlanRequestValidator : AbstractValidator<RejectPlanRequest>
{
    public RejectPlanRequestValidator() =>
        RuleFor(x => x.Reason).NotEmpty().Must(r => r.Trim().Length >= 10).WithMessage("At least 10 characters.").MaximumLength(500);
}
