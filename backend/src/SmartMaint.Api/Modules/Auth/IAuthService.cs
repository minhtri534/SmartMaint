using SmartMaint.Api.Modules.Auth.Dtos;

namespace SmartMaint.Api.Modules.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken);

    /// <summary>Changes the caller's password and returns a fresh token (clears the AUTH-04 flag).</summary>
    Task<LoginResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
}
