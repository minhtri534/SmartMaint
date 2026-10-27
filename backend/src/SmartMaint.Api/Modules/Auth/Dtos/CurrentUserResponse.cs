using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Modules.Auth.Dtos;

public sealed record CurrentUserResponse(
    long Id,
    string Email,
    string FullName,
    UserRole Role,
    bool MustChangePassword,
    DateTimeOffset? LastLoginAt)
{
    public static CurrentUserResponse From(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role, user.MustChangePassword, user.LastLoginAt);
}
