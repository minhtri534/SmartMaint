using SmartMaint.Api.Common.Ai;

namespace SmartMaint.Api.Common.Extensions;

/// <summary>
/// AI gateway wiring (Replicate). Always registered: without Replicate__ApiToken the gateway reports "not available"
/// and every AI endpoint answers available = false, so the screens work as if there were no AI.
/// </summary>
internal static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAiGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ReplicateOptions>().Bind(configuration.GetSection(ReplicateOptions.SectionName));
        services.AddHttpClient<IAiGateway, ReplicateAiGateway>();
        return services;
    }
}
