using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Accounts;

/// <summary>Every error the Accounts module can return. Codes are part of the API contract.</summary>
internal static class AccountErrors
{
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "account.not_found", "Account not found.");

    public static AppException UsernameTaken() =>
        new(ErrorKind.Conflict, "account.username_taken", "This username is already used.");

    public static AppException PhoneTaken() =>
        new(ErrorKind.Conflict, "account.phone_taken", "Another account already has this phone number.");

    public static AppException EmailTaken() =>
        new(ErrorKind.Conflict, "account.email_taken", "This e-mail is already used by another account.");

    public static AppException UnknownSpecialty() =>
        new(ErrorKind.BadRequest, "account.unknown_specialty", "One of the specialties does not exist.");

    public static AppException UnknownBuilding() =>
        new(ErrorKind.BadRequest, "account.unknown_building", "One of the buildings does not exist.");

    public static AppException ActionNotAllowed() =>
        new(ErrorKind.Conflict, "account.action_not_allowed",
            "This action is not possible for this account any more. Reload the page.");

    public static AppException OpenWork(int count) =>
        new(ErrorKind.Conflict, "account.open_work",
            $"This technician still holds {count} open case(s). Reassign them first.");

    public static AppException LastAdmin() =>
        new(ErrorKind.Conflict, "account.last_admin", "The system must keep at least one active Administrator.");
}
