using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Audit.Dtos;

namespace SmartMaint.Api.Modules.Accounts.Dtos;

/// <summary>Everything the Administrator can do to an account (DLG-16), sent as snake_case ("reset_password").</summary>
public enum AccountAction
{
    Edit,
    Deactivate,
    Reactivate,
    ResetPassword,
    Unlock,

    /// <summary>Meeting note 18/11: only an account that left no trace (no case, history, cost, plan, list, chat...).</summary>
    Delete,
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
    decimal? HourlyRate,
    IReadOnlyList<SpecialtyResponse> Specialties,
    IReadOnlyList<AssignedBuildingResponse> Buildings,
    bool HasHistory,
    IReadOnlyList<AccountAction> AllowedActions)
    : AccountSummaryResponse(Id, Username, FullName, Email, PhoneNumber, Role, Status, IsLocked, LastLoginAt, CreatedAt);

/// <summary>
/// Create: the account, and whether its temporary password went out by e-mail (meeting note 18/11). Only when it did
/// not (e-mail not set up, or refused) is the password in the reply, shown once to the Administrator.
/// </summary>
public sealed record AccountWithPasswordResponse(AccountDetailResponse Account, bool EmailSent, string? TemporaryPassword);

/// <summary>Reset password: the same rule as <see cref="AccountWithPasswordResponse"/>.</summary>
public sealed record TemporaryPasswordResponse(bool EmailSent, string? TemporaryPassword);

public sealed record RoleCountResponse(UserRole Role, int Active, int Deactivated);

/// <summary>ADM-01.</summary>
public sealed record AdminOverviewResponse(
    IReadOnlyList<RoleCountResponse> Accounts,
    int LockedNow,
    int MustChangePassword,
    IReadOnlyList<AuditLogResponse> RecentActivity);
