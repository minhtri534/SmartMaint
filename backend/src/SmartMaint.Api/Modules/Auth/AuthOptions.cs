namespace SmartMaint.Api.Modules.Auth;

/// <summary>
/// Sign-in rule values (BR-18). Bound from the "Auth" config section; will move to SystemSetting
/// (ADM-07) once that table exists.
/// </summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public int MaxFailedAttempts { get; init; } = 5;
    public int LockoutMinutes { get; init; } = 15;
}
