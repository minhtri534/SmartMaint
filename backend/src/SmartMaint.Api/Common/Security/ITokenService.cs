using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Common.Security;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(Account user);
}
