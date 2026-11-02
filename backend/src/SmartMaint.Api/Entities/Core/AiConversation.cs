using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Entities.Core;

// AI assistant tables. Created now (Leader 02/11) but not used yet: the AI feature comes later, for Technician and
// Customer only. Cardinalities are 1-1 exactly as drawn in the ERD (one conversation per account, one message per
// conversation); ERD review #5 asks for 1-M, to fix at the ERD meeting before the feature is built.

/// <summary>ERD: AIConversation(ID, ContextType, ContextID, UserID).</summary>
public class AiConversation : Entity
{
    /// <summary>What the conversation is about, e.g. "case".</summary>
    public string ContextType { get; set; } = string.Empty;

    public long? ContextId { get; set; }

    public long UserId { get; set; }
    public Account User { get; set; } = null!;

    public AiMessage? Message { get; set; }
}

/// <summary>ERD: AIMessage(ID, ConversationID, Message, Reply, TimeSent).</summary>
public class AiMessage : Entity
{
    public long ConversationId { get; set; }
    public AiConversation Conversation { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
    public string? Reply { get; set; }

    /// <summary>ERD: TimeSent.</summary>
    public DateTimeOffset SentAt { get; set; }
}
