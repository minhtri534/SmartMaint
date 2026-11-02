using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Accounts;

/// <summary>Every error the Accounts module can return. Codes are part of the API contract.</summary>
internal static class AccountErrors
{
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "account.not_found", "Account not found.");

    public static AppException UsernameTaken() =>
        new(ErrorKind.Conflict, "account.username_taken", "This username is already used.");

    public static AppException EmailTaken() =>
        new(ErrorKind.Conflict, "account.email_taken", "This e-mail is already used by another account.");

    public static AppException UnknownRoom() =>
        new(ErrorKind.BadRequest, "account.unknown_room", "Choose the customer's room.");

    public static AppException CommonAreaRoom() =>
        new(ErrorKind.BadRequest, "account.common_area_room", "A customer rents a room, not a common area.");

    public static AppException UnknownSpecialty() =>
        new(ErrorKind.BadRequest, "account.unknown_specialty", "One of the specialties does not exist.");

    public static AppException UnknownBuilding() =>
        new(ErrorKind.BadRequest, "account.unknown_building", "One of the buildings does not exist.");

    public static AppException ActionNotAllowed() =>
        new(ErrorKind.Conflict, "account.action_not_allowed",
            "This action is not possible for this account any more. Reload the page.");

    public static AppException HasHistory() =>
        new(ErrorKind.Conflict, "account.has_history",
            "This account already has history in its role. Deactivate it and create a new account instead.");

    public static AppException OpenWork(int count) =>
        new(ErrorKind.Conflict, "account.open_work",
            $"This technician still holds {count} open case(s). Reassign them first.");

    public static AppException LastAdmin() =>
        new(ErrorKind.Conflict, "account.last_admin", "The system must keep at least one active Administrator.");
}
