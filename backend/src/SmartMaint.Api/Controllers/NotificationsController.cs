using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Notifications;
using SmartMaint.Api.Modules.Notifications.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>COM-05: the signed-in user's own notifications (every role).</summary>
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public sealed class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<NotificationResponse>> List([FromQuery] PageRequest page, CancellationToken cancellationToken) =>
        notifications.ListMineAsync(page, cancellationToken);
}
