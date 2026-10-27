using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Users;

namespace SmartMaint.UnitTests.Common;

public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

public sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;
    public long Id { get; set; }
    public UserRole Role { get; set; }
}

public sealed class FakeTokenService : ITokenService
{
    public AccessToken CreateAccessToken(User user) =>
        new($"token-for-{user.Id}", DateTimeOffset.UnixEpoch);
}
