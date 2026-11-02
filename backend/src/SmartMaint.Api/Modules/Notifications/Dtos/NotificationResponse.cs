namespace SmartMaint.Api.Modules.Notifications.Dtos;

public sealed record NotificationResponse(long Id, string Details, DateTimeOffset SentAt);
