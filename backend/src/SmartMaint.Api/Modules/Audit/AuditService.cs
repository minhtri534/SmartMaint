using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Audit.Dtos;

namespace SmartMaint.Api.Modules.Audit;

internal sealed class AuditService(AppDbContext db, ICurrentUser currentUser, TimeProvider clock) : IAuditService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public void Record(AuditTarget target, AuditOperation operation, object? oldValue, object? newValue)
    {
        // The ERD's audit log holds Administrator actions only (AdminID -> Admins). Since the meeting note of 18/11 a
        // manager's approval of a staff change list also changes floors, rooms and assets: that list (who wrote it, who
        // approved it, when, every line) is the record of those changes, so nothing is written here for them.
        if (currentUser.Role != UserRole.Admin)
        {
            return;
        }

        Record(currentUser.Id, target, operation, oldValue, newValue);
    }

    public void Record(long adminId, AuditTarget target, AuditOperation operation, object? oldValue, object? newValue) =>
        db.AuditLogs.Add(new AuditLog
        {
            AdminId = adminId,
            TargetType = target,
            Operation = operation,
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue, Json),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue, Json),
            OccurredAt = clock.GetUtcNow(),
        });

    public async Task<PagedResult<AuditLogResponse>> ListAsync(AuditLogQuery query, CancellationToken cancellationToken)
    {
        var rows = db.AuditLogs.AsNoTracking();
        if (query.TargetType is { } target)
        {
            rows = rows.Where(a => a.TargetType == target);
        }

        if (query.Operation is { } operation)
        {
            rows = rows.Where(a => a.Operation == operation);
        }

        if (query.From is { } from)
        {
            var start = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            rows = rows.Where(a => a.OccurredAt >= start);
        }

        if (query.To is { } to)
        {
            var end = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            rows = rows.Where(a => a.OccurredAt < end);
        }

        var total = await rows.CountAsync(cancellationToken);
        var ordered = query.OldestFirst
            ? rows.OrderBy(a => a.OccurredAt).ThenBy(a => a.Id)
            : rows.OrderByDescending(a => a.OccurredAt).ThenByDescending(a => a.Id);
        var items = await ordered
            .Skip(query.Skip).Take(query.PageSize)
            .Select(a => new AuditLogResponse(
                a.Id, a.OccurredAt, a.AdminId, a.Admin.FullName, a.TargetType, a.Operation, a.OldValue, a.NewValue))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogResponse>(items, query.Page, query.PageSize, total);
    }
}
