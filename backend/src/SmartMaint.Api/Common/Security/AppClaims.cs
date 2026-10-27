namespace SmartMaint.Api.Common.Security;

/// <summary>JWT claim names shared by the token issuer (Infrastructure) and readers (Api).</summary>
public static class AppClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string Role = "role";
    public const string PasswordChangeRequired = "pwd_change_required";
}
