using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>Every error the Cases module can return. Codes are part of the API contract.</summary>
internal static class CaseErrors
{
    // Also returned for a case the caller may not see: never reveal that it exists (B3).
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "case.not_found", "Case not found.");

    public static AppException InvalidTransition() =>
        new(ErrorKind.Conflict, "case.invalid_transition",
            "This action is no longer possible. The case may have been changed by someone else. Reload the page.");

    public static AppException UnknownServiceType() =>
        new(ErrorKind.BadRequest, "case.unknown_service_type", "Choose a service type.");

    public static AppException UnknownRoom() =>
        new(ErrorKind.BadRequest, "case.unknown_room", "Choose a room or area.");

    public static AppException AssetNotInRoom() =>
        new(ErrorKind.BadRequest, "case.asset_not_in_room", "That asset is not in the chosen room.");

    public static AppException PhotoRequired(string serviceType) =>
        new(ErrorKind.BadRequest, "case.photo_required", $"\"{serviceType}\" needs at least one photo.");

    public static AppException InvalidTechnician() =>
        new(ErrorKind.BadRequest, "case.invalid_technician", "Choose an active technician.");

    public static AppException NoOpenAssignment() =>
        new(ErrorKind.Conflict, "case.no_open_assignment", "There is no assignment waiting for your answer.");
}
