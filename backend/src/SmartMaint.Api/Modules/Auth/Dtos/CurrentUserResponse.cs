using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Modules.Auth.Dtos;

public sealed record CurrentUserResponse(
    long Id,
    string Username,
    string? Email,
    string FullName,
    UserRole Role,
    bool MustChangePassword,
    DateTimeOffset? LastLoginAt)
{
    public static CurrentUserResponse From(Account user) =>
        new(user.Id, user.Username, user.Email, user.FullName, user.Role, user.MustChangePassword, user.LastLoginAt);
}
