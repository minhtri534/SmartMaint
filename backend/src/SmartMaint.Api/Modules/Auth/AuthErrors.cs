using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Auth;

/// <summary>Every error the Auth module can return. Codes are part of the API contract.</summary>
internal static class AuthErrors
{
    // Never say which of email / password was wrong (AUTH-01).
    public static AppException InvalidCredentials() =>
        new(ErrorKind.Unauthorized, "auth.invalid_credentials", "Email or password is incorrect.");

    public static AppException AccountLocked(DateTimeOffset until) =>
        new(ErrorKind.Forbidden, "auth.account_locked",
            $"Too many failed attempts. Try again after {until:HH:mm} UTC.");

    public static AppException AccountDeactivated() =>
        new(ErrorKind.Forbidden, "auth.account_deactivated",
            "This account has been deactivated. Please contact the Administrator.");

    public static AppException WrongCurrentPassword() =>
        new(ErrorKind.BadRequest, "auth.wrong_current_password", "Current password is incorrect.");

    public static AppException UserNotFound() =>
        new(ErrorKind.NotFound, "auth.user_not_found", "User not found.");
}
