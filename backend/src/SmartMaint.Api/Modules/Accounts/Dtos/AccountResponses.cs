using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Audit.Dtos;

namespace SmartMaint.Api.Modules.Accounts.Dtos;

/// <summary>Everything the Administrator can do to an account (DLG-16), sent as snake_case ("reset_password").</summary>
public enum AccountAction
{
    Edit,
    ChangeRole,
    Deactivate,
    Reactivate,
    ResetPassword,
    Unlock,
}

/// <summary>One row of ADM-02.</summary>
public record AccountSummaryResponse(
    long Id,
    string Username,
    string FullName,
    string? Email,
    string? PhoneNumber,
    UserRole Role,
    UserStatus Status,
    bool IsLocked,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt);

public sealed record RoomRefResponse(long Id, string BuildingName, string FloorName, string RoomName);

public sealed record SpecialtyResponse(long Id, string Name);

public sealed record AssignedBuildingResponse(long BuildingId, string BuildingName, bool IsPrimary);

/// <summary>
/// ADM-04. <see cref="AllowedActions"/> is decided by the server (status, lock, history, last admin, yourself),
/// the web only draws those buttons — same pattern as the case detail.
/// </summary>
public sealed record AccountDetailResponse(
    long Id,
    string Username,
    string FullName,
    string? Email,
    string? PhoneNumber,
    UserRole Role,
    UserStatus Status,
    bool IsLocked,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    bool MustChangePassword,
    DateTimeOffset? LockedUntil,
    RoomRefResponse? Room,
    decimal? HourlyRate,
    IReadOnlyList<SpecialtyResponse> Specialties,
    IReadOnlyList<AssignedBuildingResponse> Buildings,
    bool HasHistory,
    IReadOnlyList<AccountAction> AllowedActions)
    : AccountSummaryResponse(Id, Username, FullName, Email, PhoneNumber, Role, Status, IsLocked, LastLoginAt, CreatedAt);

/// <summary>Create / change role: the account plus its temporary password, shown once and never stored readable.</summary>
public sealed record AccountWithPasswordResponse(AccountDetailResponse Account, string TemporaryPassword);

public sealed record TemporaryPasswordResponse(string TemporaryPassword);

public sealed record RoleCountResponse(UserRole Role, int Active, int Deactivated);

/// <summary>ADM-01.</summary>
public sealed record AdminOverviewResponse(
    IReadOnlyList<RoleCountResponse> Accounts,
    int LockedNow,
    int MustChangePassword,
    IReadOnlyList<AuditLogResponse> RecentActivity);
