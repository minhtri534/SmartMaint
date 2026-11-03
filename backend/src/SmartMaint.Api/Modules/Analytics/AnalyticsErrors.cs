using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Analytics;

/// <summary>Every error the Analytics module (F6) can return. Codes are part of the API contract.</summary>
internal static class AnalyticsErrors
{
    public static AppException BuildingNotAllowed() =>
        new(ErrorKind.BadRequest, "analytics.building_not_allowed", "Choose one of the buildings you look after.");

    public static AppException TechniciansManagerOnly() =>
        new(ErrorKind.BadRequest, "analytics.managers_only", "Only managers see this report.");
}
