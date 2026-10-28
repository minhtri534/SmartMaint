using Microsoft.AspNetCore.Authorization;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Common.Security;

/// <summary>
/// The one way to restrict an endpoint by role. Never check roles by hand inside controllers or services.
/// <code>[AuthorizeRoles(UserRole.Admin)]</code>
/// Without this attribute an endpoint still requires a signed-in user (fallback policy);
/// use [AllowAnonymous] for public endpoints.
/// Data scope (e.g. a Staff member only sees buildings assigned to them) is enforced in the service, via ICurrentUser.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params UserRole[] roles)
    {
        Roles = string.Join(',', roles.Select(EnumText.ToText));
    }
}
