using SmartMaint.Api.Modules.PriorityLevels.Dtos;

namespace SmartMaint.Api.Modules.PriorityLevels;

/// <summary>
/// ADM-07 "SLA settings" (Leader 02/11: only the SLA hours of each urgency level). A new value applies from the next
/// time a technician starts work: due dates already set do not move.
/// </summary>
public interface IPriorityLevelService
{
    Task<IReadOnlyList<PriorityLevelResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Admin only; written to the audit log with the old and new value.</summary>
    Task<PriorityLevelResponse> UpdateSlaAsync(long id, UpdateSlaRequest request, CancellationToken cancellationToken);
}
