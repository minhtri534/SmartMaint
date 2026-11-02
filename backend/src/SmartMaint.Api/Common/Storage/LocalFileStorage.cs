using Microsoft.Extensions.Options;

namespace SmartMaint.Api.Common.Storage;

/// <summary>
/// Dev fallback when Supabase is not configured: files go to the "uploads" folder next to the API and are served
/// at /api/files/... (see StorageServiceCollectionExtensions.UseLocalFileStorage). Not for deployment: no access check.
/// </summary>
internal sealed class LocalFileStorage(IOptions<StorageOptions> options, IWebHostEnvironment environment, TimeProvider clock)
    : IFileStorage
{
    public const string RequestPath = "/api/files";

    private readonly string _root = Path.Combine(environment.ContentRootPath, options.Value.LocalFolder);

    public async Task<string> SaveAsync(string folder, UploadedFile file, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var path = $"{folder}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}.{file.Extension}";
        var fullPath = Path.Combine(_root, path.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, file.Content, cancellationToken);
        return path;
    }

    public Task<IReadOnlyDictionary<string, string>> GetUrlsAsync(
        IReadOnlyCollection<string> paths, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<string, string>>(
            paths.Distinct().ToDictionary(p => p, p => $"{RequestPath}/{p}"));
}
