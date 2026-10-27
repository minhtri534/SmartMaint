using FluentValidation;
using SmartMaint.Api.Modules.Auth;

namespace SmartMaint.Api.Modules;

internal static class ModuleRegistration
{
    /// <summary>Registers every module service. New module = one line here.</summary>
    public static IServiceCollection AddModules(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly, includeInternalTypes: true);

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
