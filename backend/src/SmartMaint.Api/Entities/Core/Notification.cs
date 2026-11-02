using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Entities.Core;

/// <summary>
/// ERD: Notifications(ID, UserID, Details, TimeSent). In-app only. As drawn in the ERD there is no "read" flag and no link
/// to the case, so Details carries the case code in its text (e.g. "CS-000012 was approved.").
/// </summary>
public class Notification : Entity
{
    public long UserId { get; set; }
    public Account User { get; set; } = null!;

    public string Details { get; set; } = string.Empty;

    /// <summary>ERD: TimeSent.</summary>
    public DateTimeOffset SentAt { get; set; }
}
