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
        RuleFor(x => x.Reason).IsInEnum()
            // F5: these come from "Needs the staff's decision" and the staff's hand-over, not from a plain hold.
            .Must(r => r is not (Entities.Cases.HoldReason.WaitingStaffDecision or Entities.Cases.HoldReason.WaitingThirdParty))
            .WithMessage("Use \"Needs the staff's decision\" for this.");
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
        RuleFor(x => x.Parts).ValidCostLines();
        RuleFor(x => x.ReplacementModel).MaximumLength(150);
        RuleFor(x => x.ReplacementSerial).MaximumLength(100);
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

public sealed class ReassignCaseRequestValidator : AbstractValidator<ReassignCaseRequest>
{
    public ReassignCaseRequestValidator()
    {
        RuleFor(x => x.TechnicianId).GreaterThan(0).WithMessage("Choose a technician.");
        RuleFor(x => x.Reason).NotEmpty().Must(r => r.Trim().Length >= ReasonRequestValidator.MinLength)
            .WithMessage($"At least {ReasonRequestValidator.MinLength} characters.").MaximumLength(ReasonRequestValidator.MaxLength);
    }
}

public sealed class ChangeUrgencyRequestValidator : AbstractValidator<ChangeUrgencyRequest>
{
    public ChangeUrgencyRequestValidator()
    {
        RuleFor(x => x.Urgency).IsInEnum();
        RuleFor(x => x.Reason).NotEmpty().Must(r => r.Trim().Length >= ReasonRequestValidator.MinLength)
            .WithMessage($"At least {ReasonRequestValidator.MinLength} characters.").MaximumLength(ReasonRequestValidator.MaxLength);
    }
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
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum()
            .NotEqual(Entities.Maintenance.CaseTaskStatus.Pending).WithMessage("Choose Pass, Fail or N/A.");
        RuleFor(x => x.Note).MaximumLength(1000);
        RuleFor(x => x.Note).NotEmpty().Must(n => n!.Trim().Length >= 5)
            .When(x => x.Status == Entities.Maintenance.CaseTaskStatus.Fail)
            .WithMessage("Say what is wrong (at least 5 characters).");
        RuleFor(x => x.FailAction).NotNull().IsInEnum()
            .When(x => x.Status == Entities.Maintenance.CaseTaskStatus.Fail)
            .WithMessage("Choose: fixed on the spot, or make a repair case.");
        RuleFor(x => x.Photos).ValidPhotos();
    }
}

public sealed class CompleteChecklistRequestValidator : AbstractValidator<CompleteChecklistRequest>
{
    public CompleteChecklistRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(1000);
        RuleFor(x => x.LaborHours)
            .InclusiveBetween(CompleteWorkRequestValidator.MinHours, CompleteWorkRequestValidator.MaxHours)
            .When(x => x.LaborHours is not null);
        RuleFor(x => x.Parts).ValidCostLines();
    }
}

// ----- F4 emergency -------------------------------------------------------------------------------------------------

public sealed class DowngradeRequestValidator : AbstractValidator<DowngradeRequest>
{
    public DowngradeRequestValidator()
    {
        RuleFor(x => x.Urgency).IsInEnum()
            .NotEqual(Entities.Cases.CasePriority.Emergency).WithMessage("Choose High, Normal or Low.");
        RuleFor(x => x.CostBearer).IsInEnum();
        RuleFor(x => x.Reason).NotEmpty().Must(r => r.Trim().Length >= ReasonRequestValidator.MinLength)
            .WithMessage($"At least {ReasonRequestValidator.MinLength} characters.").MaximumLength(ReasonRequestValidator.MaxLength);
    }
}

public sealed class MergeCaseRequestValidator : AbstractValidator<MergeCaseRequest>
{
    public MergeCaseRequestValidator()
    {
        RuleFor(x => x.TargetCaseId).GreaterThan(0).WithMessage("Choose the case to merge into.");
        RuleFor(x => x.Note).MaximumLength(ReasonRequestValidator.MaxLength);
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
        RuleFor(x => x.Condition).Text(10, 1000);
        RuleFor(x => x.Cause).Text(5, 1000);
        RuleFor(x => x.Recommendation).IsInEnum();
        RuleFor(x => x.Photos).NotEmpty().WithMessage("Add at least one photo.");
        RuleFor(x => x.Photos).ValidPhotos();
    }
}

public sealed class OutsourceRequestValidator : AbstractValidator<OutsourceRequest>
{
    public OutsourceRequestValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Job).IsInEnum();
        RuleFor(x => x.CompanyName).Text(2, 150);
        RuleFor(x => x.Contact).Text(5, 150);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}

public sealed class RecordQuoteRequestValidator : AbstractValidator<RecordQuoteRequest>
{
    public RecordQuoteRequestValidator()
    {
        RuleFor(x => x.Bearer).IsInEnum();
        RuleFor(x => x.SentTo!).Text(3, 150).When(x => x.Bearer == Entities.Cases.CostBearer.Customer)
            .WithName("Sent to");
        RuleFor(x => x.Note).MaximumLength(500);
    }
}

public sealed class QuoteDecisionRequestValidator : AbstractValidator<QuoteDecisionRequest>
{
    public QuoteDecisionRequestValidator()
    {
        RuleFor(x => x.DecidedBy).Text(3, 150);
        RuleFor(x => x.Note).MaximumLength(500);
        RuleFor(x => x.Evidence)
            .Must(e => e is null || (e.Length <= PhotoRules.MaxLength && DataUrl.IsImage(e)))
            .WithMessage("The evidence must be a JPEG, PNG or WebP image of reasonable size.");
    }
}

public sealed class CheckOutsideWorkRequestValidator : AbstractValidator<CheckOutsideWorkRequest>
{
    public CheckOutsideWorkRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(500);
        RuleFor(x => x.Note!).Text(5, 500).When(x => !x.Passed).WithMessage("Say what is not OK (at least 5 characters).");
    }
}

public sealed class RedoOutsideWorkRequestValidator : AbstractValidator<RedoOutsideWorkRequest>
{
    public RedoOutsideWorkRequestValidator() => RuleFor(x => x.Reason).Text(5, 500);
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
