using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Notifications.Dtos;

namespace SmartMaint.Api.Modules.Notifications;

/// <summary>
/// In-app notifications (ERD: Notifications, SRS 5.4.2). <see cref="Send"/> only adds the rows: they are saved by the
/// caller's SaveChanges together with the change that caused them. No e-mail (Leader 02/11).
/// </summary>
public interface INotificationService
{
    /// <summary>One row per distinct user; the signed-in user never notifies themself.</summary>
    void Send(IEnumerable<long> userIds, string details);

    /// <summary>COM-05: the caller's notifications of the last 90 days, newest first.</summary>
    Task<PagedResult<NotificationResponse>> ListMineAsync(PageRequest page, CancellationToken cancellationToken);
}
