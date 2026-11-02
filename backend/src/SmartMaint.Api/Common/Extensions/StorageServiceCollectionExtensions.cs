using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SmartMaint.Api.Common.Storage;

namespace SmartMaint.Api.Common.Extensions;

/// <summary>File storage wiring: Supabase Storage when Supabase__Url + Supabase__ServiceRoleKey are set, else the local folder.</summary>
internal static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(StorageOptions.SectionName);
        services.AddOptions<StorageOptions>().Bind(section);

        if (section.Get<StorageOptions>()?.UseSupabase == true)
        {
            services.AddHttpClient<IFileStorage, SupabaseFileStorage>();
        }
        else
        {
            services.AddSingleton<IFileStorage, LocalFileStorage>();
        }

        return services;
    }

    /// <summary>Serves the local folder at /api/files (only when Supabase is not configured).</summary>
    public static WebApplication UseLocalFileStorage(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<StorageOptions>>().Value;
        if (options.UseSupabase)
        {
            return app;
        }

        var root = Path.Combine(app.Environment.ContentRootPath, options.LocalFolder);
        Directory.CreateDirectory(root);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = LocalFileStorage.RequestPath,
        });
        return app;
    }
}
