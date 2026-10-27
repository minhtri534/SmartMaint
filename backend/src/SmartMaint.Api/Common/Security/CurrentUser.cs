using System.Globalization;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Common.Security;

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private System.Security.Claims.ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public long Id => long.Parse(Require(AppClaims.UserId), CultureInfo.InvariantCulture);

    public UserRole Role => EnumText.FromText<UserRole>(Require(AppClaims.Role));

    private string Require(string claimType) =>
        Principal?.FindFirst(claimType)?.Value
        ?? throw new InvalidOperationException($"Claim '{claimType}' is missing; is the endpoint [AllowAnonymous]?");
}
