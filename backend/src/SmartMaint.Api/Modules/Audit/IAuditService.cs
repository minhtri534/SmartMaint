using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Audit.Dtos;

namespace SmartMaint.Api.Modules.Audit;

/// <summary>
/// The audit trail (ERD: AuditLog, BR-16). As in the ERD only Administrator actions are recorded.
/// <see cref="Record"/> only adds the row: it is saved by the caller's SaveChanges, together with the change itself,
/// so a change and its audit row are never split.
/// </summary>
public interface IAuditService
{
    /// <summary>An action of the signed-in Administrator.</summary>
    void Record(AuditTarget target, AuditOperation operation, object? oldValue, object? newValue);

    /// <summary>An action of an Administrator who is not the signed-in user yet (sign-in).</summary>
    void Record(long adminId, AuditTarget target, AuditOperation operation, object? oldValue, object? newValue);

    /// <summary>ADM-08: newest first, filtered.</summary>
    Task<PagedResult<AuditLogResponse>> ListAsync(AuditLogQuery query, CancellationToken cancellationToken);
}
