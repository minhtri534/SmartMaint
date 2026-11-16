using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace SmartMaint.Api.Common.Ai;

/// <summary>
/// Replicate through its HTTP API (no extra package). A prediction is asynchronous: we create it with
/// "Prefer: wait" (Replicate holds the request until the answer is ready, up to 10 s), then poll
/// GET predictions/{id} until it succeeds, fails or our own time limit runs out.
/// Docs: https://replicate.com/docs/reference/http (create a prediction using an official model, get a prediction).
/// </summary>
internal sealed partial class ReplicateAiGateway(
    HttpClient http, IOptions<ReplicateOptions> options, ILogger<ReplicateAiGateway> logger) : IAiGateway
{
    private static readonly TimeSpan PollEvery = TimeSpan.FromMilliseconds(800);

    /// <summary>A throttled request waits at most this long before its one retry.</summary>
    internal static TimeSpan MaxRetryWait { get; set; } = TimeSpan.FromSeconds(10);

    private readonly ReplicateOptions _options = options.Value;

    public bool IsAvailable => _options.IsConfigured;

    public async Task<string?> AskAsync(string systemPrompt, string prompt, int maxTokens, CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            return null;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        try
        {
            var prediction = await CreateAsync(systemPrompt, prompt, maxTokens, timeout.Token);
            while (prediction is { Status: "starting" or "processing" })
            {
                await Task.Delay(PollEvery, timeout.Token);
                prediction = await GetAsync(prediction.Id, timeout.Token);
            }

            if (prediction?.Status != "succeeded")
            {
                logger.LogWarning("Replicate prediction ended as {Status}: {Error}", prediction?.Status, prediction?.Error);
                return null;
            }

            return OutputText(prediction.Output);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException
            || (ex is OperationCanceledException && !cancellationToken.IsCancellationRequested))
        {
            // Timeout or network / API trouble: the caller carries on without AI.
            logger.LogWarning(ex, "Replicate question failed");
            return null;
        }
    }

    private Task<Prediction?> CreateAsync(string systemPrompt, string prompt, int maxTokens, CancellationToken cancellationToken) =>
        SendAsync(() =>
        {
            var request = NewRequest(HttpMethod.Post, $"models/{_options.Model}/predictions");
            request.Headers.Add("Prefer", "wait=10");
            // One input for both models we use; Replicate ignores the fields a model does not have.
            // Llama 3 (replicate.com/meta/meta-llama-3-70b-instruct/api): max_tokens, low temperature = steadier answers.
            // gpt-5-nano (replicate.com/openai/gpt-5-nano/api): minimal reasoning + low verbosity = quick, short answers.
            request.Content = JsonContent.Create(new
            {
                input = new
                {
                    system_prompt = systemPrompt,
                    prompt,
                    max_tokens = maxTokens,
                    temperature = 0.2,
                    reasoning_effort = "minimal",
                    verbosity = "low",
                    max_completion_tokens = maxTokens,
                },
            });
            return request;
        }, cancellationToken);

    private Task<Prediction?> GetAsync(string id, CancellationToken cancellationToken) =>
        SendAsync(() => NewRequest(HttpMethod.Get, $"predictions/{Uri.EscapeDataString(id)}"), cancellationToken);

    /// <summary>
    /// Sends the request; when Replicate throttles us (429: a small account may create 6 predictions a minute) it waits
    /// for the reset it names, at most <see cref="MaxRetryWait"/>, and tries once more.
    /// </summary>
    private async Task<Prediction?> SendAsync(Func<HttpRequestMessage> newRequest, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            using var request = newRequest();
            using var response = await http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Prediction>(cancellationToken);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt == 0)
            {
                await Task.Delay(RetryWait(response, body), cancellationToken);
                continue;
            }

            throw new HttpRequestException($"Replicate answered {(int)response.StatusCode}: {body}");
        }
    }

    /// <summary>Retry-After header, else "resets in ~3s" in the body, else 2 s; plus a second of margin.</summary>
    private static TimeSpan RetryWait(HttpResponseMessage response, string body)
    {
        var wait = response.Headers.RetryAfter?.Delta
            ?? (ResetsIn().Match(body) is { Success: true } m
                ? TimeSpan.FromSeconds(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
                : TimeSpan.FromSeconds(2));
        wait += TimeSpan.FromSeconds(1);
        return wait > MaxRetryWait ? MaxRetryWait : wait;
    }

    private HttpRequestMessage NewRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, new Uri(new Uri(_options.BaseUrl), path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
        return request;
    }

    /// <summary>Language models on Replicate stream their output as a list of pieces; some return one string.</summary>
    internal static string? OutputText(JsonElement output) => output.ValueKind switch
    {
        JsonValueKind.String => output.GetString(),
        JsonValueKind.Array => string.Concat(output.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())),
        _ => null,
    };

    [GeneratedRegex(@"resets in ~?(\d+)\s*s")]
    private static partial Regex ResetsIn();

    private sealed record Prediction(string Id, string Status, JsonElement Output, JsonElement Error);
}
