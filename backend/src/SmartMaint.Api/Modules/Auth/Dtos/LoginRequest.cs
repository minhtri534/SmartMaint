namespace SmartMaint.Api.Modules.Auth.Dtos;

/// <summary>
/// Meeting note 18/11 (decision 1): sign in with the e-mail address or the phone number, no longer the username.
/// <see cref="Username"/> is the old field name, still read so the mobile app keeps working until it sends
/// <see cref="Login"/>; either one may hold the e-mail or the phone number.
/// </summary>
public sealed record LoginRequest(string? Login, string Password, string? Username = null)
{
    /// <summary>What the person typed: the e-mail address or the phone number.</summary>
    public string Identifier => (string.IsNullOrWhiteSpace(Login) ? Username : Login)?.Trim() ?? string.Empty;
}
