using FluentValidation;
using SmartMaint.Api.Modules.Accounts;
using SmartMaint.Api.Modules.Ai;
using SmartMaint.Api.Modules.Analytics;
using SmartMaint.Api.Modules.AssetChanges;
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
        var email = configuration.GetSection(EmailOptions.SectionName);
        services.AddOptions<EmailOptions>().Bind(email);
        // Railway's Hobby plan blocks SMTP: with a Brevo key the API sends over HTTPS instead.
        if (email.Get<EmailOptions>()?.UseBrevo == true)
        {
            services.AddHttpClient<IEmailSender, BrevoEmailSender>(c => c.Timeout = TimeSpan.FromSeconds(15));
        }
        else
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPriorityLevelService, PriorityLevelService>();
        services.AddScoped<IBuildingScope, BuildingScope>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAssetChangeService, AssetChangeService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ICatalogAdminService, CatalogAdminService>();
        services.AddScoped<IChecklistService, ChecklistService>();
        services.AddScoped<IMaintenancePlanService, MaintenancePlanService>();

        services.AddScoped<IAiService, AiService>();

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
