using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Common.Storage;
using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.UnitTests.Common;

public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

public sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;
    public long Id { get; set; }
    public UserRole Role { get; set; }
}

public sealed class FakeTokenService : ITokenService
{
    public AccessToken CreateAccessToken(Account user) =>
        new($"token-for-{user.Id}", DateTimeOffset.UnixEpoch);
}

/// <summary>Keeps "uploaded" files in memory; URLs are fake but stable so tests can compare them.</summary>
public sealed class FakeFileStorage : IFileStorage
{
    public Dictionary<string, UploadedFile> Files { get; } = [];

    public Task<string> SaveAsync(string folder, UploadedFile file, CancellationToken cancellationToken)
    {
        var path = $"{folder}/{Files.Count + 1}.{file.Extension}";
        Files[path] = file;
        return Task.FromResult(path);
    }

    public Task<IReadOnlyDictionary<string, string>> GetUrlsAsync(
        IReadOnlyCollection<string> paths, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<string, string>>(paths.Distinct().ToDictionary(p => p, p => $"https://files.test/{p}"));
}

/// <summary>Stands in for Replicate: answers with <see cref="Reply"/> (null = the AI failed) and keeps the prompt.</summary>
public sealed class FakeAiGateway : SmartMaint.Api.Common.Ai.IAiGateway
{
    public bool IsAvailable { get; set; } = true;
    public string? Reply { get; set; }
    public int Calls { get; private set; }
    public string? LastPrompt { get; private set; }

    public string? LastSystemPrompt { get; private set; }

    public Task<string?> AskAsync(string systemPrompt, string prompt, int maxTokens, CancellationToken cancellationToken)
    {
        Calls++;
        LastPrompt = prompt;
        LastSystemPrompt = systemPrompt;
        return Task.FromResult(Reply);
    }
}
