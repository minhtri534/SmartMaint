using FluentValidation;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases.Validators;

// Same rules as the web forms (frontend/src/features/cases/schemas.ts); the web check is for comfort, this one is the real one.

internal static class PhotoRules
{
    public const int MaxPhotos = 5;

    /// <summary>The web shrinks photos to 960 px JPEG data URLs (about 100 to 300 kB of text).</summary>
    public const int MaxLength = 1_500_000;

    public static IRuleBuilderOptions<T, IReadOnlyList<string>?> ValidPhotos<T>(
        this IRuleBuilder<T, IReadOnlyList<string>?> rule) =>
        rule
            .Must(p => p is null || p.Count <= MaxPhotos).WithMessage($"At most {MaxPhotos} photos.")
            .Must(p => p is null || p.All(x => x.StartsWith("data:image/", StringComparison.Ordinal) && x.Length <= MaxLength))
            .WithMessage("Photos must be images of reasonable size.");
}

public sealed class CreateCaseRequestValidator : AbstractValidator<CreateCaseRequest>
{
    public const int TitleMinLength = 5;
    public const int TitleMaxLength = 120;
    public const int DescriptionMinLength = 15;
    public const int DescriptionMaxLength = 2000;

    public CreateCaseRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Must(t => t.Trim().Length >= TitleMinLength).WithMessage($"At least {TitleMinLength} characters.")
            .MaximumLength(TitleMaxLength).WithMessage($"At most {TitleMaxLength} characters.");
        RuleFor(x => x.Description).NotEmpty().Must(t => t.Trim().Length >= DescriptionMinLength)
            .WithMessage($"Describe the problem in at least {DescriptionMinLength} characters.")
            .MaximumLength(DescriptionMaxLength).WithMessage($"At most {DescriptionMaxLength} characters.");
        RuleFor(x => x.ServiceTypeId).GreaterThan(0).WithMessage("Choose what is wrong.");
        RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("Choose a room or area.");
        RuleFor(x => x.Photos).ValidPhotos();
    }
}

public sealed class ApproveCaseRequestValidator : AbstractValidator<ApproveCaseRequest>
{
    public ApproveCaseRequestValidator()
    {
        RuleFor(x => x.Urgency).IsInEnum();
        RuleFor(x => x.CostBearer).IsInEnum();
    }
}

public sealed class RejectCaseRequestValidator : AbstractValidator<RejectCaseRequest>
{
    public const int NoteMinLength = 10;

    public RejectCaseRequestValidator()
    {
        RuleFor(x => x.Reason).IsInEnum();
        RuleFor(x => x.Note).NotEmpty().Must(n => n.Trim().Length >= NoteMinLength)
            .WithMessage($"Tell the customer why (at least {NoteMinLength} characters).")
            .MaximumLength(500);
    }
}

public sealed class AssignCaseRequestValidator : AbstractValidator<AssignCaseRequest>
{
    public AssignCaseRequestValidator() => RuleFor(x => x.TechnicianId).GreaterThan(0).WithMessage("Choose a technician.");
}

public sealed class ReasonRequestValidator : AbstractValidator<ReasonRequest>
{
    public const int MinLength = 10;
    public const int MaxLength = 500;

    public ReasonRequestValidator() =>
        RuleFor(x => x.Reason).NotEmpty().Must(r => r.Trim().Length >= MinLength)
            .WithMessage($"At least {MinLength} characters.")
            .MaximumLength(MaxLength).WithMessage($"At most {MaxLength} characters.");
}

public sealed class HoldCaseRequestValidator : AbstractValidator<HoldCaseRequest>
{
    public HoldCaseRequestValidator(TimeProvider clock)
    {
        RuleFor(x => x.Reason).IsInEnum();
        // The day it should continue is required (N13); today is fine, the past is not.
        RuleFor(x => x.ExpectedResumeOn)
            .Must(d => d >= DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime))
            .WithMessage("Choose today or a later day.");
    }
}

public sealed class CompleteWorkRequestValidator : AbstractValidator<CompleteWorkRequest>
{
    public const decimal MinHours = 0.25m;
    public const decimal MaxHours = 24m;

    public CompleteWorkRequestValidator()
    {
        RuleFor(x => x.RootCause).NotEmpty().Must(r => r.Trim().Length >= 3).WithMessage("Enter the cause.").MaximumLength(1000);
        RuleFor(x => x.PartsUsed).MaximumLength(500);
        RuleFor(x => x.LaborHours)
            .InclusiveBetween(MinHours, MaxHours).When(x => x.LaborHours is not null)
            .WithMessage($"Between {MinHours} and {MaxHours} hours.");
        RuleFor(x => (IReadOnlyList<string>?)x.BeforePhotos).ValidPhotos();
        RuleFor(x => (IReadOnlyList<string>?)x.AfterPhotos).NotEmpty().WithMessage("Add at least one photo after the repair.").ValidPhotos();
    }
}

public sealed class RateCaseRequestValidator : AbstractValidator<RateCaseRequest>
{
    public RateCaseRequestValidator()
    {
        RuleFor(x => x.Stars).InclusiveBetween(1, 5).WithMessage("Choose 1 to 5 stars.");
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
