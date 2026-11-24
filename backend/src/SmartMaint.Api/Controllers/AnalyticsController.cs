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

    /// <summary>The Charts tab (meeting note 18/11), managers only: it names technicians and staff.</summary>
    [HttpGet("chart")]
    [AuthorizeRoles(UserRole.Manager)]
    public Task<ChartResponse> Chart([FromQuery] ChartQuery query, CancellationToken cancellationToken) =>
        analytics.ChartAsync(query, cancellationToken);

    /// <summary>ANA-04 (meeting note 18/11): the tab on screen as an .xlsx file. Technicians and charts: managers only.</summary>
    [HttpGet("export")]
    [AuthorizeRoles(UserRole.Staff, UserRole.Manager)]
    [Produces(ExcelType)]
    public async Task<FileContentResult> Export(
        [FromQuery] ExportQuery query, [FromServices] ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (query.Tab is ExportTab.Technicians or ExportTab.Charts && currentUser.Role != UserRole.Manager)
        {
            throw AnalyticsErrors.TechniciansManagerOnly();
        }

        var (fileName, content) = await analytics.ExportAsync(query, cancellationToken);
        return File(content, ExcelType, fileName);
    }

    private const string ExcelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
