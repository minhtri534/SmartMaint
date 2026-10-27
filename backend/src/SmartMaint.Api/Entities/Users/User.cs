using SmartMaint.Api.Entities;

namespace SmartMaint.Api.Entities.Users;

/// <summary>
/// Minimal user for Auth (allowed before the ERD freeze). Columns that depend on F1 tables
/// (unit_id, is_away — D2, D5) are added in a later migration once the ERD is frozen.
/// </summary>
public class User : AuditableEntity
{
    private User()
    {
    }

    public User(string email, string fullName, UserRole role, string passwordHash, bool mustChangePassword)
    {
        Email = NormalizeEmail(email);
        FullName = fullName.Trim();
        Role = role;
        PasswordHash = passwordHash;
        MustChangePassword = mustChangePassword;
        Status = UserStatus.Active;
    }

    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public bool MustChangePassword { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }

    public bool IsLockedOut(DateTimeOffset now) => LockedUntil is { } until && until > now;

    /// <summary>Counts a wrong password; locks the account when the limit is reached (BR-18).</summary>
    public void RecordFailedLogin(DateTimeOffset now, int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= maxAttempts)
        {
            LockedUntil = now.Add(lockoutDuration);
            FailedLoginCount = 0;
        }
    }

    public void RecordSuccessfulLogin(DateTimeOffset now)
    {
        FailedLoginCount = 0;
        LockedUntil = null;
        LastLoginAt = now;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        MustChangePassword = false;
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
