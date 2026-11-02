using FluentValidation;
using SmartMaint.Api.Modules.Accounts;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Auth;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Catalog;
using SmartMaint.Api.Modules.CatalogAdmin;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Maintenance;
using SmartMaint.Api.Modules.Notifications;
using SmartMaint.Api.Modules.PriorityLevels;

namespace SmartMaint.Api.Modules;

internal static class ModuleRegistration
{
    /// <summary>Registers every module service. New module = one line here.</summary>
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly, includeInternalTypes: true);
        services.AddOptions<JobOptions>().Bind(configuration.GetSection(JobOptions.SectionName));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICaseService, CaseService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPriorityLevelService, PriorityLevelService>();
        services.AddScoped<IBuildingScope, BuildingScope>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IReplacementService, ReplacementService>();
        services.AddScoped<ICatalogAdminService, CatalogAdminService>();
        services.AddScoped<IChecklistService, ChecklistService>();
        services.AddScoped<IMaintenancePlanService, MaintenancePlanService>();

        // Background jobs (inside the API)
        services.AddScoped<ISlaReminder, SlaReminder>();
        services.AddHostedService<SlaReminderJob>();
        services.AddScoped<IMaintenanceGenerator, MaintenanceGenerator>();
        services.AddHostedService<MaintenanceGeneratorJob>();
        services.AddScoped<IEmergencyWatch, EmergencyWatch>();
        services.AddHostedService<EmergencyWatchJob>();

        return services;
    }
}
