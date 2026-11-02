using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// How often an asset broke (F1 "frequent failure" flag, F5 decision board). A repair = an incident on the asset:
/// not a preventive work order, not a survey, not a report merged into another one, not rejected or cancelled.
/// </summary>
public static class AssetRepairStats
{
    /// <summary>From this many repairs in 12 months an asset "fails often" (F1, F5 way 2).</summary>
    public const int FrequentFailureThreshold = 3;

    public static readonly TimeSpan Window = TimeSpan.FromDays(365);

    public static Task<int> RepairsSinceAsync(AppDbContext db, long assetId, DateTimeOffset since, CancellationToken cancellationToken) =>
        db.Cases.AsNoTracking().CountAsync(c =>
            c.AssetId == assetId && c.TriggerWorkOrderId == null && !c.IsSurvey && c.MergedIntoCaseId == null
            && c.Status != CaseStatus.Rejected && c.Status != CaseStatus.Cancelled && c.SubmittedAt >= since,
            cancellationToken);
}
