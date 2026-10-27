using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Modules.Auth;
using SmartMaint.Api.Modules.Auth.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>
/// Sample controller — every controller follows this shape: thin, one service call per action,
/// no try/catch, no role checks by hand, no DbContext.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>AUTH-01: sign in with email and password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken) =>
        authService.LoginAsync(request, cancellationToken);

    /// <summary>The signed-in user's own account.</summary>
    [HttpGet("me")]
    [AllowPendingPasswordChange]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    public Task<CurrentUserResponse> Me(CancellationToken cancellationToken) =>
        authService.GetCurrentUserAsync(cancellationToken);

    /// <summary>AUTH-04 / COM-03: change own password. Returns a new token.</summary>
    [HttpPost("change-password")]
    [AllowPendingPasswordChange]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<LoginResponse> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken) =>
        authService.ChangePasswordAsync(request, cancellationToken);
}
