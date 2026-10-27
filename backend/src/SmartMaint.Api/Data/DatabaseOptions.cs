namespace SmartMaint.Api.Data;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Apply pending migrations and run Data.sql on startup. Development only.</summary>
    public bool MigrateAndSeedOnStartup { get; init; }
}
