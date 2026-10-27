namespace SmartMaint.Api.Entities.Users;

/// <summary>
/// Account state managed by the Administrator. A temporary lock after failed sign-ins is NOT a status;
/// it is tracked by <see cref="User.LockedUntil"/> (BR-18).
/// </summary>
public enum UserStatus
{
    Active,
    Deactivated,
}
