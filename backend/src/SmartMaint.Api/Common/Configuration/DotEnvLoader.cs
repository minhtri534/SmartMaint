namespace SmartMaint.Api.Common.Configuration;

/// <summary>
/// Loads the repo-root <c>.env</c> (KEY=VALUE lines) into environment variables for local development,
/// so docker compose and the API read the same file. Variables already set in the real environment win.
/// Railway / production has no .env file and sets variables in its dashboard instead.
/// </summary>
internal static class DotEnvLoader
{
    private const string FileName = ".env";
    private const int MaxLevelsUp = 5;

    public static void Load()
    {
        var path = FindFile();
        if (path is null)
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"');
            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? FindFile()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var level = 0; directory is not null && level <= MaxLevelsUp; level++)
        {
            var candidate = Path.Combine(directory.FullName, FileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
