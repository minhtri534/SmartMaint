using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data;
using SmartMaint.Api.Data.Interceptors;

namespace SmartMaint.Api.Common.Extensions;

/// <summary>Database wiring: Postgres + snake_case names + created_at/updated_at interceptor + health check.</summary>
internal static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Default is not set. Copy .env.example to .env (local) or set ConnectionStrings__Default.");
        }

        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddSingleton<TimestampInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetRequiredService<TimestampInterceptor>()));

        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

        return services;
    }
}
