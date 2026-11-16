namespace SmartMaint.Api.Common.Ai;

/// <summary>
/// The one door to the LLM (kernel, D-plan "AI gateway"). Implementations never throw for AI trouble: they return
/// null, and the caller shows the screen without the suggestion.
/// </summary>
public interface IAiGateway
{
    /// <summary>False when no token is set: callers skip the question (and do not log it).</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// The model's text answer (at most <paramref name="maxTokens"/> tokens long), or null when the AI failed, timed
    /// out or is off.
    /// </summary>
    Task<string?> AskAsync(string systemPrompt, string prompt, int maxTokens, CancellationToken cancellationToken);
}
