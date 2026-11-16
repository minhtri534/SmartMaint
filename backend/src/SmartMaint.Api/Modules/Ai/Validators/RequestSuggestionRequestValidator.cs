using FluentValidation;
using SmartMaint.Api.Modules.Ai.Dtos;
using SmartMaint.Api.Modules.Cases.Validators;

namespace SmartMaint.Api.Modules.Ai.Validators;

/// <summary>Same length limits as the request's description, so whatever the form accepts can be asked about.</summary>
public sealed class RequestSuggestionRequestValidator : AbstractValidator<RequestSuggestionRequest>
{
    public RequestSuggestionRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty()
            .Must(d => d.Trim().Length >= CreateCaseRequestValidator.DescriptionMinLength)
            .WithMessage($"Describe the problem in at least {CreateCaseRequestValidator.DescriptionMinLength} characters first.")
            .MaximumLength(CreateCaseRequestValidator.DescriptionMaxLength)
            .WithMessage($"At most {CreateCaseRequestValidator.DescriptionMaxLength} characters.");
    }
}
