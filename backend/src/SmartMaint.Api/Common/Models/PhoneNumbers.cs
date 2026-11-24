namespace SmartMaint.Api.Common.Models;

/// <summary>Phone numbers as they are compared and stored (meeting note 18/11: a phone number signs in).</summary>
public static class PhoneNumbers
{
    /// <summary>Digits only, with a leading "+" kept: "+84 90-123 4567" -> "+84901234567".</summary>
    public static string Normalize(string phone)
    {
        var trimmed = phone.Trim();
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        return trimmed.StartsWith('+') ? $"+{digits}" : digits;
    }
}
