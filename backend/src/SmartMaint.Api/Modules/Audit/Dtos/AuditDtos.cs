using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Entities.Core;

namespace SmartMaint.Api.Modules.Audit.Dtos;

/// <summary>ADM-08 filters: ?targetType=account&amp;operation=create&amp;from=2026-11-01&amp;to=2026-11-02&amp;page=1.</summary>
public sealed record AuditLogQuery : PageRequest
{
    public AuditTarget? TargetType { get; init; }
    public AuditOperation? Operation { get; init; }

    /// <summary>First day included (UTC date).</summary>
    public DateOnly? From { get; init; }

    /// <summary>Last day included (UTC date).</summary>
    public DateOnly? To { get; init; }
}

public sealed record AuditLogResponse(
    long Id,
    DateTimeOffset OccurredAt,
    long AdminId,
    string AdminName,
    AuditTarget TargetType,
    AuditOperation Operation,
    string? OldValue,
    string? NewValue);
