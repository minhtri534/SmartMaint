using Microsoft.AspNetCore.Identity;

namespace SmartMaint.Api.Common.Security;

/// <summary>PBKDF2 (ASP.NET Core Identity v3 format). Only the hash is stored, never the password.</summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Subject = new();

    public string Hash(string password) => Hasher.HashPassword(Subject, password);

    public bool Verify(string hash, string password) =>
        Hasher.VerifyHashedPassword(Subject, hash, password) != PasswordVerificationResult.Failed;
}
