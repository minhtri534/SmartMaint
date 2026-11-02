namespace SmartMaint.Api.Common.Storage;

/// <summary>
/// Where uploaded files live (case photos, task photos). The database keeps only the path returned by
/// <see cref="SaveAsync"/>; the web gets short-lived URLs from <see cref="GetUrlsAsync"/>.
/// Two implementations: SupabaseFileStorage (when Supabase__Url is set) and LocalFileStorage (dev without a key).
/// </summary>
public interface IFileStorage
{
    /// <summary>Saves the file under <paramref name="folder"/> and returns its path, e.g. "cases/2026/11/3f2a….jpg".</summary>
    Task<string> SaveAsync(string folder, UploadedFile file, CancellationToken cancellationToken);

    /// <summary>A URL to show each path (signed for 1 hour on Supabase). Paths that cannot be signed are left out.</summary>
    Task<IReadOnlyDictionary<string, string>> GetUrlsAsync(IReadOnlyCollection<string> paths, CancellationToken cancellationToken);
}

/// <summary>A file received from the web, already decoded.</summary>
public sealed record UploadedFile(byte[] Content, string ContentType)
{
    public string Extension => ContentType switch
    {
        "image/png" => "png",
        "image/webp" => "webp",
        _ => "jpg",
    };
}
