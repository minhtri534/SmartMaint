using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Users;
using SmartMaint.Api.Modules.Auth.Dtos;

namespace SmartMaint.Api.Modules.Auth;

internal sealed class AuthService(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUser currentUser,
    TimeProvider clock,
    IOptions<AuthOptions> options) : IAuthService
{
    private readonly AuthOptions _options = options.Value;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(request.Email);
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw AuthErrors.InvalidCredentials();

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
        await db.SaveChangesAsync(cancellationToken);

        // TODO(audit): write every sign-in attempt to audit_logs (BR-16) once the table is frozen.
        return IssueToken(user);
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

    private async Task<User> FindCurrentUserAsync(CancellationToken cancellationToken) =>
        await db.Users.SingleOrDefaultAsync(u => u.Id == currentUser.Id, cancellationToken)
        ?? throw AuthErrors.UserNotFound();

    private LoginResponse IssueToken(User user)
    {
        var token = tokenService.CreateAccessToken(user);
        return new LoginResponse(token.Token, token.ExpiresAt, CurrentUserResponse.From(user));
    }
}
