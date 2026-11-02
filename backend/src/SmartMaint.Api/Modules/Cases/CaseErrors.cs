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

    public static AppException RoomNotAllowed() =>
        new(ErrorKind.BadRequest, "case.room_not_allowed",
            "You can report problems in your own room or in the common areas of your building.");

    public static AppException AssetNotInRoom() =>
        new(ErrorKind.BadRequest, "case.asset_not_in_room", "That asset is not in the chosen room, or it was retired.");

    public static AppException PhotoRequired(string serviceType) =>
        new(ErrorKind.BadRequest, "case.photo_required", $"\"{serviceType}\" needs at least one photo.");

    public static AppException TaskNotFound() =>
        new(ErrorKind.NotFound, "case.task_not_found", "This checklist item does not belong to the case.");

    public static AppException SameTechnician() =>
        new(ErrorKind.BadRequest, "case.same_technician", "This technician already holds the case. Choose another one.");

    public static AppException SameUrgency() =>
        new(ErrorKind.BadRequest, "case.same_urgency", "The case already has this urgency.");

    public static AppException DueNotLater() =>
        new(ErrorKind.BadRequest, "case.due_not_later", "The new deadline must be later than the current one.");

    public static AppException InvalidTechnician() =>
        new(ErrorKind.BadRequest, "case.invalid_technician", "Choose an active technician.");
}
