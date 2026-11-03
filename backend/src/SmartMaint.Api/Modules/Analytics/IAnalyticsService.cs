using SmartMaint.Api.Modules.Analytics.Dtos;

namespace SmartMaint.Api.Modules.Analytics;

/// <summary>
/// F6 (Hùng) — monitoring. Staff see their buildings (no technician ranking, no ratings), managers their buildings in
/// full (S01). Read only: the Manager directs outside the system (BR-12, S04 / S05). The Administrator has no analytics.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>ANA-01 (and the MGR-01 tiles): this period against the one before.</summary>
    Task<OverviewResponse> OverviewAsync(AnalyticsQuery query, CancellationToken cancellationToken);

    /// <summary>ANA-02, Manager only.</summary>
    Task<IReadOnlyList<TechnicianRow>> TechniciansAsync(AnalyticsQuery query, CancellationToken cancellationToken);

    /// <summary>ANA-03.</summary>
    Task<FailuresResponse> FailuresAsync(AnalyticsQuery query, CancellationToken cancellationToken);

    /// <summary>F6 step ④ "Needs attention", now.</summary>
    Task<IReadOnlyList<AttentionItem>> AttentionAsync(CancellationToken cancellationToken);

    /// <summary>ANA-04: one report as CSV (UTF-8 with BOM).</summary>
    Task<(string FileName, byte[] Content)> ExportAsync(ExportReport report, AnalyticsQuery query, CancellationToken cancellationToken);
}
