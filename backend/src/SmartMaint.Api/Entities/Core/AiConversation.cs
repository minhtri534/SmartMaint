using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Entities.Core;

// AI assistant tables (AI for Customer + Technician only, suggestions only). Every question asked to the AI is kept
// here (BR-16): who asked, about what, the question and the answer. The ERD draws both links as 1-1; since 16/11 they
// are 1-M as ERD review #5 asked (one account asks many times, a conversation may hold several messages).
// Logged in docs/database/ERD_v5_Thay_Doi_Khi_Code.md.

/// <summary>ERD: AIConversation(ID, ContextType, ContextID, UserID).</summary>
public class AiConversation : Entity
{
    /// <summary>What the conversation is about: "case_request" (a customer writing a request), later "case".</summary>
    public string ContextType { get; set; } = string.Empty;

    public long? ContextId { get; set; }

    public long UserId { get; set; }
    public Account User { get; set; } = null!;

    public List<AiMessage> Messages { get; } = [];
}

/// <summary>ERD: AIMessage(ID, ConversationID, Message, Reply, TimeSent).</summary>
public class AiMessage : Entity
{
    public long ConversationId { get; set; }
    public AiConversation Conversation { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    /// <summary>The model's raw answer; empty when the AI failed or timed out.</summary>
    public string? Reply { get; set; }

    /// <summary>ERD: TimeSent.</summary>
    public DateTimeOffset SentAt { get; set; }
}
