using FluentValidation;
using SmartMaint.Api.Common.Storage;
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
            .Must(p => p is null || p.All(x => x.Length <= MaxLength && DataUrl.IsImage(x)))
            .WithMessage("Photos must be JPEG, PNG or WebP images of reasonable size.");
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
        RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("Choose a room.");
        RuleFor(x => x.AssetId).NotNull().WithMessage("Choose the asset that has the problem.")
            .GreaterThan(0).WithMessage("Choose the asset that has the problem.");
        RuleFor(x => x.Photos).ValidPhotos();
        RuleFor(x => x.Urgency).IsInEnum().When(x => x.Urgency is not null).WithMessage("Choose how urgent it is.");
    }
}

internal static class WorkOrderRules
{
    public const int MaxTasks = 30;

    public static IRuleBuilderOptions<T, IReadOnlyList<TaskLineRequest>?> ValidTasks<T>(
        this IRuleBuilder<T, IReadOnlyList<TaskLineRequest>?> rule) =>
        rule
            .Must(l => l is null || l.Count <= MaxTasks).WithMessage($"At most {MaxTasks} tasks.")
            .Must(l => l is null || l.All(x => x.Name is not null && x.Name.Trim().Length is >= 2 and <= 200))
            .WithMessage("Each task needs a name of 2 to 200 characters.")
            .Must(l => l is null || l.All(x => (x.Description?.Length ?? 0) <= 500))
            .WithMessage("A task description has at most 500 characters.");

    /// <summary>The planned end comes after the planned start.</summary>
    public static bool ValidWindow(DateTimeOffset? start, DateTimeOffset? end) => start is null || end is null || end > start;
}

/// <summary>DLG-01: the work order form of an approval.</summary>
public sealed class ApproveCaseRequestValidator : AbstractValidator<ApproveCaseRequest>
{
    public ApproveCaseRequestValidator()
    {
        RuleFor(x => x.Urgency).IsInEnum();
        RuleFor(x => x.TechnicianId).GreaterThan(0).WithMessage("Choose a technician.");
        RuleFor(x => x.ServiceTypeId).GreaterThan(0).When(x => x.ServiceTypeId is not null).WithMessage("Choose a category.");
        RuleFor(x => x.Title).MaximumLength(CreateCaseRequestValidator.TitleMaxLength);
        RuleFor(x => x.Description).MaximumLength(CreateCaseRequestValidator.DescriptionMaxLength);
        RuleFor(x => x.PlannedStartAt).NotNull().When(x => x.Urgency != Entities.Cases.CasePriority.Emergency)
            .WithMessage("Choose the planned start.");
        RuleFor(x => x.PlannedEndAt).Must((x, end) => WorkOrderRules.ValidWindow(x.PlannedStartAt, end))
            .WithMessage("The planned end must be after the planned start.");
        RuleFor(x => x.Tasks).ValidTasks();
    }
}

/// <summary>The staff's Edit (meeting 07/11).</summary>
public sealed class EditCaseRequestValidator : AbstractValidator<EditCaseRequest>
{
    public EditCaseRequestValidator()
    {
        RuleFor(x => x.Urgency).IsInEnum();
        RuleFor(x => x.ServiceTypeId).GreaterThan(0).WithMessage("Choose a category.");
        RuleFor(x => x.Title).NotEmpty().Must(t => t.Trim().Length >= CreateCaseRequestValidator.TitleMinLength)
            .WithMessage($"At least {CreateCaseRequestValidator.TitleMinLength} characters.")
            .MaximumLength(CreateCaseRequestValidator.TitleMaxLength);
        RuleFor(x => x.Description).NotEmpty().Must(t => t.Trim().Length >= CreateCaseRequestValidator.DescriptionMinLength)
            .WithMessage($"At least {CreateCaseRequestValidator.DescriptionMinLength} characters.")
            .MaximumLength(CreateCaseRequestValidator.DescriptionMaxLength);
        RuleFor(x => x.PlannedEndAt).Must((x, end) => WorkOrderRules.ValidWindow(x.PlannedStartAt, end))
            .WithMessage("The planned end must be after the planned start.");
        RuleFor(x => (IReadOnlyList<TaskLineRequest>?)x.Tasks).NotNull().ValidTasks();
        RuleFor(x => x.StatusChange).IsInEnum().When(x => x.StatusChange is not null);
        RuleFor(x => x.Reason!).NotEmpty().Must(r => r is not null && r.Trim().Length >= ReasonRequestValidator.MinLength)
            .WithMessage($"Say what is not OK (at least {ReasonRequestValidator.MinLength} characters).")
            .MaximumLength(ReasonRequestValidator.MaxLength)
            .When(x => x.StatusChange == EditStatusChange.Reopen);
    }
}

public sealed class ReassignRequestValidator : AbstractValidator<ReassignRequest>
{
    public ReassignRequestValidator() => RuleFor(x => x.TechnicianId).GreaterThan(0).WithMessage("Choose a technician.");
}

public sealed class SetFollowersRequestValidator : AbstractValidator<SetFollowersRequest>
{
    public const int MaxFollowers = 20;

    public SetFollowersRequestValidator() =>
        RuleFor(x => x.CustomerIds).NotNull().Must(f => f.Count <= MaxFollowers).WithMessage($"At most {MaxFollowers} followers.");
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
        RuleFor(x => x.Duplicate).NotNull().When(x => x.Reason == RejectReason.Duplicate)
            .WithMessage("Choose the case this request repeats.");
        RuleFor(x => x.Duplicate!.CaseId).GreaterThan(0).WithMessage("Choose the case this request repeats.")
            .When(x => x.Reason == RejectReason.Duplicate && x.Duplicate is not null);
    }
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

public sealed class RateCaseRequestValidator : AbstractValidator<RateCaseRequest>
{
    public RateCaseRequestValidator()
    {
        RuleFor(x => x.Stars).InclusiveBetween(1, 5).WithMessage("Choose 1 to 5 stars.");
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}

internal static class CostRules
{
    public const int MaxLines = 20;
    public const decimal MaxAmount = 1_000_000_000m;

    public static IRuleBuilderOptions<T, IReadOnlyList<CostLineRequest>?> ValidCostLines<T>(
        this IRuleBuilder<T, IReadOnlyList<CostLineRequest>?> rule) =>
        rule
            .Must(l => l is null || l.Count <= MaxLines).WithMessage($"At most {MaxLines} cost lines.")
            .Must(l => l is null || l.All(x => x.Name.Trim().Length is >= 2 and <= 150))
            .WithMessage("Each cost line needs a name of 2 to 150 characters.")
            .Must(l => l is null || l.All(x => (x.Description?.Length ?? 0) <= 300))
            .WithMessage("A description has at most 300 characters.")
            .Must(l => l is null || l.All(x => x.Amount is >= 0 and <= MaxAmount))
            .WithMessage("An amount is between 0 and 1,000,000,000 VND.");
}

public sealed class ExtendDueRequestValidator : AbstractValidator<ExtendDueRequest>
{
    /// <summary>A deadline is moved by days, not months.</summary>
    public const int MaxDaysAhead = 30;

    public ExtendDueRequestValidator(TimeProvider clock)
    {
        RuleFor(x => x.Reason).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(300);
        RuleFor(x => x.Note).NotEmpty().When(x => x.Reason == ExtendDueReason.Other).WithMessage("Say why.");
        RuleFor(x => x.NewDueAt)
            .Must(d => d > clock.GetUtcNow()).WithMessage("Choose a time in the future.")
            .Must(d => d <= clock.GetUtcNow().AddDays(MaxDaysAhead)).WithMessage($"At most {MaxDaysAhead} days from now.");
    }
}

public sealed class UpdateCostsRequestValidator : AbstractValidator<UpdateCostsRequest>
{
    public UpdateCostsRequestValidator() => RuleFor(x => (IReadOnlyList<CostLineRequest>?)x.Lines).NotNull().ValidCostLines();
}

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public const int MaxCaption = 200;

    public UpdateTaskRequestValidator()
    {
        // Meeting 07/11: "still to do" is allowed too, to add a note or a photo along the way.
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(1000);
        RuleFor(x => x.Note).NotEmpty().Must(n => n is not null && n.Trim().Length >= 5)
            .When(x => x.Status == Entities.Maintenance.CaseTaskStatus.Fail)
            .WithMessage("Say what is wrong (at least 5 characters).");
        RuleFor(x => x.FailAction).IsInEnum().When(x => x.FailAction is not null);
        RuleFor(x => x.Photos)
            .Must(p => p is null || p.Count <= PhotoRules.MaxPhotos).WithMessage($"At most {PhotoRules.MaxPhotos} photos.")
            .Must(p => p is null || p.All(x => x.Image.Length <= PhotoRules.MaxLength && DataUrl.IsImage(x.Image)))
            .WithMessage("Photos must be JPEG, PNG or WebP images of reasonable size.")
            .Must(p => p is null || p.All(x => (x.Caption?.Length ?? 0) <= MaxCaption))
            .WithMessage($"A caption has at most {MaxCaption} characters.");
    }
}

/// <summary>Meeting 07/11: the technician types the total work hours.</summary>
public sealed class LogHoursRequestValidator : AbstractValidator<LogHoursRequest>
{
    public const decimal MaxHours = 200m;

    public LogHoursRequestValidator() =>
        RuleFor(x => x.Hours).GreaterThan(0).LessThanOrEqualTo(MaxHours)
            .WithMessage($"More than 0 and at most {MaxHours} hours.");
}

/// <summary>Meeting 07/11: a cost the technician paid needs its receipt (number or photo).</summary>
public sealed class AddCostRequestValidator : AbstractValidator<AddCostRequest>
{
    public AddCostRequestValidator()
    {
        RuleFor(x => x.Name).Text(2, 150);
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.Amount).InclusiveBetween(0, CostRules.MaxAmount).WithMessage("An amount is between 0 and 1,000,000,000 VND.");
        RuleFor(x => x.ReceiptCode).MaximumLength(100);
        RuleFor(x => x.ReceiptPhoto)
            .Must(p => p is null || (p.Length <= PhotoRules.MaxLength && DataUrl.IsImage(p)))
            .WithMessage("The receipt photo must be a JPEG, PNG or WebP image of reasonable size.");
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.ReceiptCode) || !string.IsNullOrWhiteSpace(x.ReceiptPhoto))
            .WithName("ReceiptCode").WithMessage("Add the receipt number or a photo of the receipt.");
    }
}

/// <summary>Meeting note 18/11: the same rules as adding a cost; the receipt may already be on the line.</summary>
public sealed class EditCostRequestValidator : AbstractValidator<EditCostRequest>
{
    public EditCostRequestValidator()
    {
        RuleFor(x => x.Name).Text(2, 150);
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.Amount).InclusiveBetween(0, CostRules.MaxAmount).WithMessage("An amount is between 0 and 1,000,000,000 VND.");
        RuleFor(x => x.ReceiptCode).MaximumLength(100);
        RuleFor(x => x.ReceiptPhoto)
            .Must(p => p is null || (p.Length <= PhotoRules.MaxLength && DataUrl.IsImage(p)))
            .WithMessage("The receipt photo must be a JPEG, PNG or WebP image of reasonable size.");
    }
}

public sealed class EmergencyLogRequestValidator : AbstractValidator<EmergencyLogRequest>
{
    public const int MinLength = 5;

    public EmergencyLogRequestValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Note).NotEmpty().Must(n => n.Trim().Length >= MinLength)
            .WithMessage($"At least {MinLength} characters.").MaximumLength(ReasonRequestValidator.MaxLength);
    }
}

public sealed class IncidentReportRequestValidator : AbstractValidator<IncidentReportRequest>
{
    public const int MinLength = 10;
    public const int MaxLength = 1000;

    public IncidentReportRequestValidator()
    {
        RuleFor(x => x.Damage).NotEmpty().Must(n => n.Trim().Length >= MinLength)
            .WithMessage($"At least {MinLength} characters.").MaximumLength(MaxLength);
        RuleFor(x => x.Lessons).NotEmpty().Must(n => n.Trim().Length >= MinLength)
            .WithMessage($"At least {MinLength} characters.").MaximumLength(MaxLength);
        RuleFor(x => x.FollowUp).IsInEnum();
    }
}

// ----- F5 repair or replace -----------------------------------------------------------------------------------------

internal static class TextRules
{
    public static IRuleBuilderOptions<T, string> Text<T>(this IRuleBuilder<T, string> rule, int min, int max) =>
        rule.NotEmpty().Must(s => s is not null && s.Trim().Length >= min).WithMessage($"At least {min} characters.").MaximumLength(max);
}

public sealed class RequestDecisionRequestValidator : AbstractValidator<RequestDecisionRequest>
{
    public RequestDecisionRequestValidator()
    {
        RuleFor(x => x.Description).Text(10, 1000);
        RuleFor(x => x.Reason).Text(5, 1000);
        RuleFor(x => x.Photos).NotEmpty().WithMessage("Add at least one photo.");
        RuleFor(x => x.Photos).ValidPhotos();
    }
}


public sealed class OpenSurveyRequestValidator : AbstractValidator<OpenSurveyRequest>
{
    public OpenSurveyRequestValidator()
    {
        RuleFor(x => x.AssetId).GreaterThan(0);
        RuleFor(x => x.ServiceTypeId).GreaterThan(0).WithMessage("Choose the service type.");
        RuleFor(x => x.Note).MaximumLength(CreateCaseRequestValidator.DescriptionMaxLength);
    }
}
