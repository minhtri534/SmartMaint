using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Ai.Dtos;

/// <summary>EMP-02 helper: what the customer has written so far.</summary>
public sealed record RequestSuggestionRequest(string Description);

/// <summary>
/// The AI's suggestion while a customer writes a request (F3 section 4, point 1). Only a suggestion: the customer
/// picks, the staff confirm the urgency. Available = false means "no AI right now"; the web then falls back to its own
/// keyword rules. ServiceTypeId is always one of the catalogue's types (or null when nothing fits).
/// </summary>
public sealed record RequestSuggestionResponse(
    bool Available,
    long? ServiceTypeId,
    CasePriority? Urgency,
    IReadOnlyList<string> Tips)
{
    public static RequestSuggestionResponse Unavailable { get; } = new(false, null, null, []);
}

/// <summary>The staff asks who should take this case (meeting note 18/11).</summary>
public sealed record TechnicianSuggestionRequest(long CaseId);

public enum SuggestionSource
{
    /// <summary>The AI answered.</summary>
    Ai,

    /// <summary>The AI is off or failed: the first of the ranked list.</summary>
    Rules,
}

/// <summary>Only a suggestion: nothing changes until the staff member chooses. TechnicianId is empty when nobody is active.</summary>
public sealed record TechnicianSuggestionResponse(long? TechnicianId, string Reason, SuggestionSource Source);

/// <summary>
/// One chat message to the assistant (Leader 16/11: every role but Admin, on every page).
/// ConversationId = null starts a new conversation. CaseId = the case open on the screen, if any (read only when the
/// caller may see it). Page = the web path the user is on, so the assistant can say which button to use.
/// </summary>
public sealed record ChatRequest(string Message, long? ConversationId = null, long? CaseId = null, string? Page = null);

/// <summary>One turn: the user's question and the assistant's answer (null when the AI failed or timed out).</summary>
public sealed record ChatMessageResponse(long Id, string Question, string? Answer, DateTimeOffset SentAt);

/// <summary>Available = false: no answer this time (AI off, busy or failed); the question is still kept when asked.</summary>
public sealed record ChatReplyResponse(bool Available, long? ConversationId, ChatMessageResponse Message);

/// <summary>The caller's latest conversation (empty when they never chatted).</summary>
public sealed record ChatThreadResponse(long? ConversationId, IReadOnlyList<ChatMessageResponse> Messages);
