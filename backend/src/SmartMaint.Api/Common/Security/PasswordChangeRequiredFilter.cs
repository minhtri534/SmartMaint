using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Common.Security;

/// <summary>Marks the few endpoints a user may call while still holding a temporary password (AUTH-04).</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowPendingPasswordChangeAttribute : Attribute;

/// <summary>Blocks every other endpoint with 403 "auth.password_change_required" until the password is changed.</summary>
internal sealed class PasswordChangeRequiredFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var mustChange = context.HttpContext.User.HasClaim(AppClaims.PasswordChangeRequired, "true");
        var allowed = context.ActionDescriptor.EndpointMetadata.OfType<AllowPendingPasswordChangeAttribute>().Any();
        if (mustChange && !allowed)
        {
            var problem = ProblemFactory.Create(
                context.HttpContext, StatusCodes.Status403Forbidden, "auth.password_change_required",
                "You must change your temporary password first.");
            context.Result = new ObjectResult(problem) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
