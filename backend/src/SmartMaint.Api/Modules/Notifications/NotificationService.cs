using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Notifications.Dtos;

namespace SmartMaint.Api.Modules.Notifications;

internal sealed class NotificationService(AppDbContext db, ICurrentUser currentUser, TimeProvider clock) : INotificationService
{
    /// <summary>Notifications are kept on screen for 90 days (SRS 3.4.2).</summary>
    private static readonly TimeSpan KeepFor = TimeSpan.FromDays(90);

    public void Send(IEnumerable<long> userIds, string details)
    {
        var now = clock.GetUtcNow();
        foreach (var userId in userIds.Distinct().Where(id => id != currentUser.Id))
        {
            db.Notifications.Add(new Notification { UserId = userId, Details = details, SentAt = now });
        }
    }

    public async Task<PagedResult<NotificationResponse>> ListMineAsync(PageRequest page, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        var since = clock.GetUtcNow() - KeepFor;
        var rows = db.Notifications.AsNoTracking().Where(n => n.UserId == userId && n.SentAt >= since);

        var total = await rows.CountAsync(cancellationToken);
        var items = await rows
            .OrderByDescending(n => n.SentAt).ThenByDescending(n => n.Id)
            .Skip(page.Skip).Take(page.PageSize)
            .Select(n => new NotificationResponse(n.Id, n.Details, n.SentAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationResponse>(items, page.Page, page.PageSize, total);
    }
}
