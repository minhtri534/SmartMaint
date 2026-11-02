using System.Security.Cryptography;

namespace SmartMaint.Api.Common.Security;

/// <summary>
/// Temporary passwords the Administrator hands to a user (create account, reset password). Always meets BR-18
/// (upper + lower + digit, 12 characters) and skips look-alike characters (0/O, 1/l/I) so it can be read out loud.
/// </summary>
public static class TemporaryPassword
{
    public const int Length = 12;

    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";

    public static string Generate()
    {
        var all = Upper + Lower + Digits;
        var chars = new List<char>
        {
            Pick(Upper),
            Pick(Lower),
            Pick(Digits),
        };
        while (chars.Count < Length)
        {
            chars.Add(Pick(all));
        }

        // Shuffle so the first three characters are not always upper / lower / digit.
        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }

    private static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];
}
