using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Common.Security;

/// <summary>The signed-in caller, read from the JWT. Use this in services instead of HttpContext.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long Id { get; }
    UserRole Role { get; }
}
