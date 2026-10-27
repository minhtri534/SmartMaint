namespace SmartMaint.Api.Modules.Auth.Dtos;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
