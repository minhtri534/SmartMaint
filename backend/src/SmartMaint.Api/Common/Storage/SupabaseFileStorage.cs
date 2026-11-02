using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace SmartMaint.Api.Common.Storage;

/// <summary>
/// Supabase Storage through its REST API (no extra package). The bucket is private: the API uploads with the
/// service-role key and hands out signed URLs that expire after StorageOptions.SignedUrlMinutes.
/// Docs: https://supabase.com/docs/reference/api/storage (object upload, create signed URLs).
/// </summary>
internal sealed class SupabaseFileStorage(HttpClient http, IOptions<StorageOptions> options, TimeProvider clock) : IFileStorage
{
    private readonly StorageOptions _options = options.Value;

    public async Task<string> SaveAsync(string folder, UploadedFile file, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var path = $"{folder}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}.{file.Extension}";

        using var request = NewRequest(HttpMethod.Post, $"object/{_options.StorageBucket}/{path}");
        request.Content = new ByteArrayContent(file.Content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Supabase Storage upload failed ({(int)response.StatusCode}): {body}");
        }

        return path;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetUrlsAsync(
        IReadOnlyCollection<string> paths, CancellationToken cancellationToken)
    {
        if (paths.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        using var request = NewRequest(HttpMethod.Post, $"object/sign/{_options.StorageBucket}");
        request.Content = JsonContent.Create(new SignRequest(_options.SignedUrlMinutes * 60, paths.Distinct().ToList()));

        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var signed = await response.Content.ReadFromJsonAsync<List<SignedUrl>>(cancellationToken) ?? [];

        // signedURL is relative to /storage/v1, e.g. "/object/sign/bucket/cases/...jpg?token=..."
        return signed
            .Where(s => s.Error is null && s.Path is not null && s.SignedUrlPath is not null)
            .ToDictionary(s => s.Path!, s => $"{BaseUrl}{s.SignedUrlPath}");
    }

    private string BaseUrl => $"{_options.Url.TrimEnd('/')}/storage/v1";

    private HttpRequestMessage NewRequest(HttpMethod method, string relativePath)
    {
        var request = new HttpRequestMessage(method, $"{BaseUrl}/{relativePath}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);
        request.Headers.Add("apikey", _options.ServiceRoleKey);
        return request;
    }

    private sealed record SignRequest(
        [property: JsonPropertyName("expiresIn")] int ExpiresIn,
        [property: JsonPropertyName("paths")] List<string> Paths);

    private sealed record SignedUrl(
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("path")] string? Path,
        [property: JsonPropertyName("signedURL")] string? SignedUrlPath);
}
