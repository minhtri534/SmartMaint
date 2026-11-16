using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Ai;

/// <summary>Every error the AI module can return. Codes are part of the API contract.</summary>
internal static class AiErrors
{
    // Also returned for someone else's conversation: never reveal that it exists.
    public static AppException ConversationNotFound() =>
        new(ErrorKind.NotFound, "ai.conversation_not_found", "This conversation is gone. Start a new chat.");
}
