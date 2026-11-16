using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Ai;
using SmartMaint.Api.Modules.Ai.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// The AI assistant (Replicate). Suggestions only: nothing here changes cases or accounts. Leader 16/11: every role but
/// Admin. When the AI is off or fails the answer is still 200 with available = false, so the screens never break
/// because of the AI.
/// </summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Customer, UserRole.Staff, UserRole.Technician, UserRole.Manager)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
public sealed class AiController(IAiService ai) : ControllerBase
{
    /// <summary>EMP-02: service type, urgency and missing details from the description being written.</summary>
    [HttpPost("request-suggestion")]
    [AuthorizeRoles(UserRole.Customer)]
    public Task<RequestSuggestionResponse> RequestSuggestion(
        RequestSuggestionRequest request, CancellationToken cancellationToken) =>
        ai.SuggestRequestAsync(request, cancellationToken);

    /// <summary>The caller's latest conversation with the assistant (shown again when the chat opens).</summary>
    [HttpGet("chat")]
    public Task<ChatThreadResponse> LatestChat(CancellationToken cancellationToken) =>
        ai.GetLatestChatAsync(cancellationToken);

    /// <summary>Ask the assistant. It answers from the caller's own data only and never acts for them.</summary>
    [HttpPost("chat")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ChatReplyResponse> Chat(ChatRequest request, CancellationToken cancellationToken) =>
        ai.ChatAsync(request, cancellationToken);
}
