using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartMaint.Api.Data.Seed;

/// <summary>
/// Development helper: applies migrations, then runs database/Data.sql (idempotent, ON CONFLICT DO NOTHING).
/// Data.sql is the single source of seed data and is also the script submitted with Report 4/5.
/// Shared environments (Supabase) are migrated with database/Schema.sql, never on app startup.
/// </summary>
public static class DatabaseInitializer
{
    public const string SeedFileName = "Data.sql";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        if (!options.MigrateAndSeedOnStartup)
        {
            return;
        }

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrated");

        var seedFile = Path.Combine(AppContext.BaseDirectory, SeedFileName);
        if (!File.Exists(seedFile))
        {
            logger.LogWarning("Seed file not found at {SeedFile}; skipping seed", seedFile);
            return;
        }

        var sql = await File.ReadAllTextAsync(seedFile, cancellationToken);
        await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger.LogInformation("Seed data applied from {SeedFile}", seedFile);
    }
}
