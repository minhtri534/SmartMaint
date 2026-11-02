namespace SmartMaint.Api.Common.Storage;

/// <summary>Section "Supabase" (.env: Supabase__Url, Supabase__ServiceRoleKey, Supabase__StorageBucket).</summary>
public sealed class StorageOptions
{
    public const string SectionName = "Supabase";

    /// <summary>https://&lt;project&gt;.supabase.co — empty means "use the local folder" (dev only).</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Server-side key; never sent to the web.</summary>
    public string ServiceRoleKey { get; set; } = string.Empty;

    /// <summary>A private bucket: files are only reachable through signed URLs.</summary>
    public string StorageBucket { get; set; } = "smartmaint";

    public int SignedUrlMinutes { get; set; } = 60;

    /// <summary>Local folder used when Url is empty, relative to the API's content root.</summary>
    public string LocalFolder { get; set; } = "uploads";

    public bool UseSupabase => !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(ServiceRoleKey);
}
