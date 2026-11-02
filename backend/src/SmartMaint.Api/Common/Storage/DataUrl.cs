namespace SmartMaint.Api.Common.Storage;

/// <summary>
/// The web sends photos inside the JSON body as data URLs ("data:image/jpeg;base64,...", already shrunk to 960 px).
/// This turns one into bytes before it goes to <see cref="IFileStorage"/>.
/// </summary>
public static class DataUrl
{
    public static readonly string[] ImageTypes = ["image/jpeg", "image/png", "image/webp"];

    public static bool IsImage(string value) => TryParseImage(value, out _);

    public static bool TryParseImage(string value, out UploadedFile file)
    {
        file = null!;
        const string prefix = "data:";
        const string marker = ";base64,";
        if (!value.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var markerAt = value.IndexOf(marker, StringComparison.Ordinal);
        if (markerAt < 0)
        {
            return false;
        }

        var contentType = value[prefix.Length..markerAt];
        if (!ImageTypes.Contains(contentType))
        {
            return false;
        }

        var bytes = new byte[value.Length];
        if (!Convert.TryFromBase64String(value[(markerAt + marker.Length)..], bytes, out var written) || written == 0)
        {
            return false;
        }

        file = new UploadedFile(bytes[..written], contentType);
        return true;
    }

    public static UploadedFile ParseImage(string value) =>
        TryParseImage(value, out var file) ? file : throw new FormatException("Not an image data URL.");
}
