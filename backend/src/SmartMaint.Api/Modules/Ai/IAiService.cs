using SmartMaint.Api.Modules.Ai.Dtos;

namespace SmartMaint.Api.Modules.Ai;

/// <summary>
/// AI suggestions through Replicate. Leader 16/11: every role but Admin, as a chat on every page, suggestions only.
/// The AI never changes data, and it only ever receives what the caller may already see: the role scope is enforced
/// in code (the data handed to the model comes from the same scoped queries as the screens), the prompt rules only
/// add to it. Every question that reaches the model is kept in ai_conversations / ai_messages (BR-16).
/// </summary>
public interface IAiService
{
    /// <summary>Customer writing a request: service type, urgency and what to add (F3 section 4, point 1).</summary>
    Task<RequestSuggestionResponse> SuggestRequestAsync(RequestSuggestionRequest request, CancellationToken cancellationToken);

    /// <summary>The caller's latest chat, to show again when the assistant opens.</summary>
    Task<ChatThreadResponse> GetLatestChatAsync(CancellationToken cancellationToken);

    /// <summary>Ask the assistant; it answers from the caller's own data only.</summary>
    Task<ChatReplyResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken);
}
