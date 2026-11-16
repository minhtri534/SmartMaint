namespace SmartMaint.Api.Common.Ai;

/// <summary>
/// Section "Replicate" (.env: Replicate__ApiToken, optional Replicate__Model). Without a token the AI is simply off:
/// every AI endpoint answers "not available" and the screens work as if there were no AI (F2/F3 section 4).
/// </summary>
public sealed class ReplicateOptions
{
    public const string SectionName = "Replicate";

    /// <summary>Server-side token from replicate.com/account/api-tokens; never sent to the web.</summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// "owner/name" of an official model. Llama 3 70B Instruct (Leader 16/11/2026): answers our JSON in under a second.
    /// openai/gpt-5-nano was the first pick (cheaper) but Replicate answers 500 for every OpenAI / Google / Anthropic
    /// model on our account; set Replicate__Model=openai/gpt-5-nano to go back once Replicate fixes it.
    /// </summary>
    public string Model { get; set; } = "meta/meta-llama-3-70b-instruct";

    public string BaseUrl { get; set; } = "https://api.replicate.com/v1/";

    /// <summary>How long one question may take in total (create + wait + poll) before we give up.</summary>
    public int TimeoutSeconds { get; set; } = 25;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiToken);
}
