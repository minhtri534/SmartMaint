using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Auth.Dtos;

namespace SmartMaint.Api.Modules.Auth;

internal sealed class AuthService(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUser currentUser,
    IAuditService audit,
    TimeProvider clock,
    IOptions<AuthOptions> options) : IAuthService
{
    private readonly AuthOptions _options = options.Value;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await FindByLoginAsync(request.Identifier, cancellationToken) ?? throw AuthErrors.InvalidCredentials();

        var now = clock.GetUtcNow();
        if (user.Status == UserStatus.Deactivated)
        {
            throw AuthErrors.AccountDeactivated();
        }

        if (user.IsLockedOut(now))
        {
            throw AuthErrors.AccountLocked(user.LockedUntil!.Value);
        }

        if (!passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            user.RecordFailedLogin(now, _options.MaxFailedAttempts, TimeSpan.FromMinutes(_options.LockoutMinutes));
            await db.SaveChangesAsync(cancellationToken);
            throw user.IsLockedOut(now) ? AuthErrors.AccountLocked(user.LockedUntil!.Value) : AuthErrors.InvalidCredentials();
        }

        user.RecordSuccessfulLogin(now);
        // BR-16 as far as the ERD allows: AuditLog only references Admins, so only administrator sign-ins are kept.
        if (user is Admin)
        {
            audit.Record(user.Id, AuditTarget.Account, AuditOperation.SignIn, null, new { user.Id, user.Username });
        }

        await db.SaveChangesAsync(cancellationToken);
        return IssueToken(user);
    }

    /// <summary>
    /// The account of an e-mail address (anything with "@") or a phone number (compared by its digits, so "090 123 4567"
    /// and "0901234567" are the same). Meeting note 18/11: the username no longer signs in.
    /// </summary>
    private async Task<Account?> FindByLoginAsync(string login, CancellationToken cancellationToken)
    {
        if (login.Contains('@'))
        {
            var email = login.Trim().ToLowerInvariant();
            return await db.Accounts.SingleOrDefaultAsync(a => a.Email == email, cancellationToken);
        }

        var phone = PhoneNumbers.Normalize(login);
        if (phone.Length < 8) return null;
        // Few accounts have a phone number: compared here so numbers saved with spaces before 18/11 still match.
        var candidates = await db.Accounts.Where(a => a.PhoneNumber != null).ToListAsync(cancellationToken);
        var found = candidates.Where(a => PhoneNumbers.Normalize(a.PhoneNumber!) == phone).ToList();
        return found.Count == 1 ? found[0] : null;
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await FindCurrentUserAsync(cancellationToken);
        return CurrentUserResponse.From(user);
    }

    public async Task<LoginResponse> ChangePasswordAsync(
        ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await FindCurrentUserAsync(cancellationToken);
        if (!passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            throw AuthErrors.WrongCurrentPassword();
        }

        user.ChangePassword(passwordHasher.Hash(request.NewPassword));
        await db.SaveChangesAsync(cancellationToken);
        return IssueToken(user);
    }

    private async Task<Account> FindCurrentUserAsync(CancellationToken cancellationToken) =>
        await db.Accounts.SingleOrDefaultAsync(u => u.Id == currentUser.Id, cancellationToken)
        ?? throw AuthErrors.UserNotFound();

    private LoginResponse IssueToken(Account user)
    {
        var token = tokenService.CreateAccessToken(user);
        return new LoginResponse(token.Token, token.ExpiresAt, CurrentUserResponse.From(user));
    }
}
