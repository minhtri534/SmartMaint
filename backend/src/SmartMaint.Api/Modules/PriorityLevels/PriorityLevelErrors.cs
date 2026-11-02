using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.PriorityLevels;

internal static class PriorityLevelErrors
{
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "priority_level.not_found", "Urgency level not found.");
}
