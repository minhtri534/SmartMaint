namespace SmartMaint.Api.Entities.Accounts;

/// <summary>
/// ERD: Account(ID, Username, Password, PhoneNumber, Role). Shared by the 5 actor tables (Customers, Staff, Technicians,
/// Manager, Admins), which reuse the same ID: EF maps this with one table per class (TPT), so <c>db.Accounts</c> is every
/// user and <c>db.Technicians</c> only technicians. The role column and the child table always agree (the constructor
/// of each child sets the role).
/// Columns after the ERD ones are NOT IN ERD v5 — see docs/database/ERD_v5_Thay_Doi_Khi_Code.md.
/// </summary>
public abstract class Account : AuditableEntity
{
    protected Account()
    {
    }

    protected Account(string username, string fullName, UserRole role, string passwordHash, bool mustChangePassword)
    {
        Username = NormalizeUsername(username);
        FullName = fullName.Trim();
        Role = role;
        PasswordHash = passwordHash;
        MustChangePassword = mustChangePassword;
        Status = UserStatus.Active;
    }

    /// <summary>Sign-in name, stored lower case, unique.</summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>ERD: Password. Only the hash is stored (PBKDF2, ASP.NET Core Identity v3).</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    /// <summary>ERD: Role -> Roles.ID (column role_id).</summary>
    public UserRole Role { get; private set; }

    // ----- NOT IN ERD v5 -----------------------------------------------------------------------------------------

    /// <summary>Shown on every screen instead of the username.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Contact only (sign-in uses the username). Unique when present.</summary>
    public string? Email { get; set; }

    public UserStatus Status { get; set; }

    /// <summary>AUTH-04: an account created by the Administrator must change its temporary password first.</summary>
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

    /// <summary>DLG-16 "Reset password": a new temporary password that must be changed at the next sign-in.</summary>
    public void ResetPassword(string temporaryPasswordHash)
    {
        PasswordHash = temporaryPasswordHash;
        MustChangePassword = true;
        Unlock();
    }

    /// <summary>DLG-16 "Unlock": ends a BR-18 lock before its time.</summary>
    public void Unlock()
    {
        FailedLoginCount = 0;
        LockedUntil = null;
    }

    public static string NormalizeUsername(string username) => username.Trim().ToLowerInvariant();

    /// <summary>
    /// A new account of the given role (the row in the role's own table comes with it).
    /// <paramref name="roomId"/> is required for a customer and ignored for the other roles.
    /// </summary>
    public static Account New(
        UserRole role, string username, string fullName, string passwordHash, bool mustChangePassword, long? roomId) =>
        role switch
        {
            UserRole.Customer => new Customer(username, fullName, passwordHash, mustChangePassword,
                roomId ?? throw new ArgumentException("A customer needs a room.", nameof(roomId))),
            UserRole.Staff => new Staff(username, fullName, passwordHash, mustChangePassword),
            UserRole.Technician => new Technician(username, fullName, passwordHash, mustChangePassword),
            UserRole.Manager => new Manager(username, fullName, passwordHash, mustChangePassword),
            UserRole.Admin => new Admin(username, fullName, passwordHash, mustChangePassword),
            _ => throw new ArgumentOutOfRangeException(nameof(role)),
        };
}
