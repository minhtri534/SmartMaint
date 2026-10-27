using SmartMaint.Api.Common.Errors;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Users;
using SmartMaint.Api.Modules.Auth;
using SmartMaint.Api.Modules.Auth.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Auth;

/// <summary>Sample test class: one class per service, Arrange / Act / Assert, one Type trait per test.</summary>
public sealed class AuthServiceTests : IDisposable
{
    private const string Password = "Correct@123";
    private static readonly AuthOptions Options = new() { MaxFailedAttempts = 5, LockoutMinutes = 15 };

    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 10, 19, 8, 0, 0, TimeSpan.Zero));
    private readonly PasswordHasher _hasher = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly AppDbContext _db;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _db = TestDbContextFactory.Create(_clock);
        _sut = new AuthService(
            _db, _hasher, new FakeTokenService(), _currentUser, _clock, Microsoft.Extensions.Options.Options.Create(Options));
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Login_WithValidCredentials_ReturnsTokenAndResetsFailedCount()
    {
        var user = await SeedUserAsync();

        var result = await _sut.LoginAsync(new LoginRequest("  USER@Smartmaint.test ", Password), default);

        Assert.Equal($"token-for-{user.Id}", result.AccessToken);
        Assert.Equal(UserRole.Officer, result.User.Role);
        Assert.Equal(0, user.FailedLoginCount);
        Assert.Equal(_clock.Now, user.LastLoginAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Login_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var user = await SeedUserAsync();

        var error = await Assert.ThrowsAsync<AppException>(
            () => _sut.LoginAsync(new LoginRequest(user.Email, "Wrong@123"), default));

        Assert.Equal("auth.invalid_credentials", error.Code);
        Assert.Equal(1, user.FailedLoginCount);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Login_WithUnknownEmail_ThrowsSameErrorAsWrongPassword()
    {
        var error = await Assert.ThrowsAsync<AppException>(
            () => _sut.LoginAsync(new LoginRequest("nobody@smartmaint.test", Password), default));

        Assert.Equal("auth.invalid_credentials", error.Code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Login_FourthWrongAttempt_DoesNotLockAccount()
    {
        var user = await SeedUserAsync();
        await FailLoginAsync(user, times: Options.MaxFailedAttempts - 1);

        Assert.False(user.IsLockedOut(_clock.Now));
        Assert.Equal(Options.MaxFailedAttempts - 1, user.FailedLoginCount);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Login_FifthWrongAttempt_LocksAccountFor15Minutes()
    {
        var user = await SeedUserAsync();
        await FailLoginAsync(user, times: Options.MaxFailedAttempts - 1);

        var error = await Assert.ThrowsAsync<AppException>(
            () => _sut.LoginAsync(new LoginRequest(user.Email, "Wrong@123"), default));

        Assert.Equal("auth.account_locked", error.Code);
        Assert.Equal(_clock.Now.AddMinutes(Options.LockoutMinutes), user.LockedUntil);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Login_ExactlyWhenLockExpires_Succeeds()
    {
        var user = await SeedUserAsync();
        await FailLoginAsync(user, times: Options.MaxFailedAttempts);
        _clock.Now = user.LockedUntil!.Value;

        var result = await _sut.LoginAsync(new LoginRequest(user.Email, Password), default);

        Assert.Equal(user.Id, result.User.Id);
        Assert.Null(user.LockedUntil);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ChangePassword_WithCorrectCurrentPassword_ClearsMustChangeFlag()
    {
        var user = await SeedUserAsync(mustChangePassword: true);
        _currentUser.Id = user.Id;

        var result = await _sut.ChangePasswordAsync(new ChangePasswordRequest(Password, "NewPass@456"), default);

        Assert.False(result.User.MustChangePassword);
        Assert.True(_hasher.Verify(user.PasswordHash, "NewPass@456"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task ChangePassword_WithWrongCurrentPassword_Throws()
    {
        var user = await SeedUserAsync();
        _currentUser.Id = user.Id;

        var error = await Assert.ThrowsAsync<AppException>(
            () => _sut.ChangePasswordAsync(new ChangePasswordRequest("Wrong@123", "NewPass@456"), default));

        Assert.Equal("auth.wrong_current_password", error.Code);
    }

    private async Task<User> SeedUserAsync(bool mustChangePassword = false)
    {
        var user = new User("user@smartmaint.test", "Test User", UserRole.Officer, _hasher.Hash(Password), mustChangePassword);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private async Task FailLoginAsync(User user, int times)
    {
        for (var i = 0; i < times; i++)
        {
            await Assert.ThrowsAsync<AppException>(
                () => _sut.LoginAsync(new LoginRequest(user.Email, "Wrong@123"), default));
        }
    }
}
