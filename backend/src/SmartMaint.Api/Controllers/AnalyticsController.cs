using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Analytics;
using SmartMaint.Api.Modules.Analytics.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>F6 analytics (ANA-01..04) and "Needs attention". Staff and managers, of their buildings; read only.</summary>
[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
public sealed class AnalyticsController(IAnalyticsService analytics) : ControllerBase
{
    /// <summary>ANA-01 / MGR-01.</summary>
    [HttpGet("overview")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    public Task<OverviewResponse> Overview([FromQuery] AnalyticsQuery query, CancellationToken cancellationToken) =>
        analytics.OverviewAsync(query, cancellationToken);

    /// <summary>ANA-02: technician performance, managers only (S01).</summary>
    [HttpGet("technicians")]
    [AuthorizeRoles(UserRole.Manager)]
    public Task<IReadOnlyList<TechnicianRow>> Technicians([FromQuery] AnalyticsQuery query, CancellationToken cancellationToken) =>
        analytics.TechniciansAsync(query, cancellationToken);

    /// <summary>ANA-03.</summary>
    [HttpGet("failures")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    public Task<FailuresResponse> Failures([FromQuery] AnalyticsQuery query, CancellationToken cancellationToken) =>
        analytics.FailuresAsync(query, cancellationToken);

    /// <summary>F6 step ④.</summary>
    [HttpGet("attention")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    public Task<IReadOnlyList<AttentionItem>> Attention(CancellationToken cancellationToken) =>
        analytics.AttentionAsync(cancellationToken);

    /// <summary>ANA-04: CSV download. The technician report is for managers only.</summary>
    [HttpGet("export/{report}")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    [Produces("text/csv")]
    public async Task<FileContentResult> Export(
        ExportReport report, [FromQuery] AnalyticsQuery query, [FromServices] ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (report == ExportReport.Technicians && currentUser.Role != UserRole.Manager)
        {
            throw AnalyticsErrors.TechniciansManagerOnly();
        }

        var (fileName, content) = await analytics.ExportAsync(report, query, cancellationToken);
        return File(content, "text/csv; charset=utf-8", fileName);
    }
}
