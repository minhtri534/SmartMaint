using SmartMaint.Api.Modules.Analytics.Dtos;

namespace SmartMaint.Api.Modules.Analytics;

/// <summary>
/// F6 (Hùng) — monitoring. Staff see their buildings (no technician ranking, no ratings), managers their buildings in
/// full (S01). Read only: the Manager directs outside the system (BR-12, S04 / S05). The Administrator has no analytics.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>ANA-01 (and the MGR-01 tiles): totals and averages of the period.</summary>
    Task<OverviewResponse> OverviewAsync(AnalyticsQuery query, CancellationToken cancellationToken);

    /// <summary>ANA-02, Manager only.</summary>
    Task<IReadOnlyList<TechnicianRow>> TechniciansAsync(AnalyticsQuery query, CancellationToken cancellationToken);

    /// <summary>ANA-03.</summary>
    Task<FailuresResponse> FailuresAsync(AnalyticsQuery query, CancellationToken cancellationToken);

    /// <summary>F6 step ④ "Needs attention", now.</summary>
    Task<IReadOnlyList<AttentionItem>> AttentionAsync(CancellationToken cancellationToken);

    /// <summary>The Charts tab: one metric over time and split by who or where (managers).</summary>
    Task<ChartResponse> ChartAsync(ChartQuery query, CancellationToken cancellationToken);

    /// <summary>The tab on screen as an Excel workbook, with a sheet of the cases of the period.</summary>
    Task<(string FileName, byte[] Content)> ExportAsync(ExportQuery query, CancellationToken cancellationToken);
}
