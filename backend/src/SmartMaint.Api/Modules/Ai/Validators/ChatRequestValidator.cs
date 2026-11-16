using FluentValidation;
using SmartMaint.Api.Modules.Ai.Dtos;

namespace SmartMaint.Api.Modules.Ai.Validators;

public sealed class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public const int MessageMaxLength = 1000;
    public const int PageMaxLength = 200;

    public ChatRequestValidator()
    {
        RuleFor(x => x.Message).Must(m => !string.IsNullOrWhiteSpace(m)).WithMessage("Type a question first.")
            .MaximumLength(MessageMaxLength).WithMessage($"At most {MessageMaxLength} characters.");
        RuleFor(x => x.ConversationId).GreaterThan(0).When(x => x.ConversationId is not null);
        RuleFor(x => x.CaseId).GreaterThan(0).When(x => x.CaseId is not null);
        RuleFor(x => x.Page).MaximumLength(PageMaxLength);
    }
}
