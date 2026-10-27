namespace SmartMaint.Api.Modules.Auth.Dtos;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, CurrentUserResponse User);
