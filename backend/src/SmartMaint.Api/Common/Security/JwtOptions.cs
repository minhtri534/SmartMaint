using System.ComponentModel.DataAnnotations;

namespace SmartMaint.Api.Common.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = "smartmaint-api";

    [Required]
    public string Audience { get; init; } = "smartmaint-clients";

    /// <summary>HMAC-SHA256 key, at least 32 characters. Set via env var Jwt__SigningKey; never commit it.</summary>
    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>Session length (BR-18: 8 hours).</summary>
    [Range(1, 24 * 60)]
    public int AccessTokenMinutes { get; init; } = 480;
}
