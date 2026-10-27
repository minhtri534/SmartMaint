using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Common.Security;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
}
