using ClosedXML.Excel;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Analytics.Dtos;
using SmartMaint.Api.Modules.Cases;

namespace SmartMaint.Api.Modules.Analytics;

/// <summary>
/// Meeting note 18/11: "Export" saves the tab on screen as an Excel workbook (ClosedXML), with its period, building,
/// chart choice and sort, and a sheet of every case reported in the period.
/// </summary>
internal sealed partial class AnalyticsService
{
    public async Task<(string FileName, byte[] Content)> ExportAsync(ExportQuery query, CancellationToken cancellationToken)
    {
        var (period, from, to) = Resolve(query);
        using var book = new XLWorkbook();

        switch (query.Tab)
        {
            case ExportTab.Overview:
                {
                    var o = await OverviewAsync(query, cancellationToken);
                    var k = o.Current;
                    Sheet(book, "Overview", ["Figure", "Value"],
                    [
                        ["Period", $"{period.From:yyyy-MM-dd} to {period.To:yyyy-MM-dd}"],
                        ["Reported", k.Reported],
                        ["Completed", k.Completed],
                        ["Rejected", k.Rejected],
                        ["Cancelled", k.Cancelled],
                        ["First response (minutes, report to first decision)", k.AvgFirstResponseMinutes],
                        ["Resolution (hours, start of work to done)", k.AvgResolutionHours],
                        ["On time (%)", Percent(k.OnTimeRate)],
                        ["Reopened (%)", Percent(k.ReopenRate)],
                        ["Average rating (stars)", k.AvgRating],
                        ["Ratings", k.Ratings],
                        ["Jobs declined", k.TechnicianDeclines],
                    ]);
                    Sheet(book, "By building", ["Building", "Reported", "Completed", "On time (%)", "Open now", "Overdue now"],
                        o.ByBuilding.Select(b => new object?[] { b.Name, b.Reported, b.Completed, Percent(b.OnTimeRate), b.OpenNow, b.OverdueNow }));
                    Sheet(book, "Time on hold", ["Reason", "Times", "Total hours", "Average hours"],
                        o.HoldTime.Select(h => new object?[] { h.Reason, h.Times, h.TotalHours, h.AvgHours }));
                    break;
                }

            case ExportTab.Charts:
                {
                    var chart = await ChartAsync(new ChartQuery
                    {
                        From = query.From, To = query.To, BuildingId = query.BuildingId, Metric = query.Metric, Step = query.Step,
                    }, cancellationToken);
                    Sheet(book, "Chart", [$"{chart.Step} starting", $"{chart.Metric} ({chart.Unit})"],
                        chart.Points.Select(p => new object?[] { p.Start.ToDateTime(TimeOnly.MinValue), p.Value }));
                    Sheet(book, "Breakdown", [chart.BreakdownBy, chart.Unit], chart.Breakdown.Select(p => new object?[] { p.Name, p.Value }));
                    break;
                }

            case ExportTab.Technicians:
                {
                    var rows = SortTechnicians(await TechniciansAsync(query, cancellationToken), query.Sort, query.Descending);
                    Sheet(book, "Technicians",
                        ["Technician", "Completed", "Avg hours", "On time (%)", "Reopened (%)", "Avg rating", "Labour hours", "Open now", "Declined"],
                        rows.Select(t => new object?[]
                        {
                            t.Technician.FullName, t.Completed, t.AvgCompletionHours, Percent(t.OnTimeRate), Percent(t.ReopenRate),
                            t.AvgRating, t.LaborHours, t.OpenNow, t.Declines,
                        }));
                    break;
                }

            case ExportTab.Failures:
                {
                    var f = await FailuresAsync(query, cancellationToken);
                    Sheet(book, "By fault category", ["Fault category", "Cases", "Avg resolution hours"],
                        f.ByServiceType.Select(s => new object?[] { s.Name, s.Cases, s.AvgResolutionHours }));
                    Sheet(book, "By asset type", ["Asset type", "Cases"], f.ByAssetType.Select(g => new object?[] { g.Group, g.Cases }));
                    Sheet(book, "Problem assets", ["Asset", "Location", "Type", "Repairs", "Repairs in 30 days", "Age (years)", "Under warranty"],
                        f.ProblemAssets.Select(a => new object?[] { a.Name, a.Location, a.TypeName, a.Repairs, a.RepairsLast30Days, a.AgeYears, a.UnderWarranty ? "yes" : "no" }));
                    break;
                }

            case ExportTab.Attention:
                {
                    var items = await AttentionAsync(cancellationToken);
                    Sheet(book, "Needs attention", ["Kind", "What", "Case", "Since"],
                        items.Select(i => new object?[] { i.Kind.ToString(), i.Text, i.CaseCode, Local(i.Since) }));
                    break;
                }
        }

        var cases = await LoadAsync(query.BuildingId, cancellationToken);
        Sheet(book, "Cases",
            ["Code", "Title", "Kind", "Building", "Room", "Fault category", "Urgency", "Status", "Reported", "Completed", "Deadline", "On time", "Technician", "Rating"],
            cases.Where(c => In(c.SubmittedAt, from, to)).OrderBy(c => c.SubmittedAt).Select(c => new object?[]
            {
                Case.CodeOf(c.Id), c.Title,
                c.TriggerWorkOrderId != null ? "Preventive" : c.IsSurvey ? "Survey" : c.MergedIntoCaseId != null ? "Duplicate"
                    : c.EmergencyAt != null ? "Emergency" : "Incident",
                c.Room.Floor.Building.Name, c.Room.RoomNumber, CaseMapper.ServiceTypeName(c.Category), c.Priority.Name,
                c.Status.ToString(), Local(c.SubmittedAt), Local(c.CompletedAt), Local(c.DueAt),
                c.CompletedAt is null || c.DueAt is null ? null : c.CompletedAt <= c.DueAt ? "yes" : "no",
                c.Technician?.FullName, c.RatingStars,
            }));

        using var stream = new MemoryStream();
        book.SaveAs(stream);
        var name = $"smartmaint-{query.Tab.ToString().ToLowerInvariant()}-{period.From:yyyyMMdd}-{period.To:yyyyMMdd}.xlsx";
        return (name, stream.ToArray());
    }

    /// <summary>The technician table in the order the manager chose on screen (the web's column names).</summary>
    internal static IReadOnlyList<TechnicianRow> SortTechnicians(IReadOnlyList<TechnicianRow> rows, string? sort, bool descending)
    {
        Func<TechnicianRow, IComparable?>? key = sort switch
        {
            "name" => t => t.Technician.FullName,
            "completed" => t => t.Completed,
            "avgTime" => t => t.AvgCompletionHours,
            "onTime" => t => t.OnTimeRate,
            "reopened" => t => t.ReopenRate,
            "rating" => t => t.AvgRating,
            "labour" => t => t.LaborHours,
            "open" => t => t.OpenNow,
            "declined" => t => t.Declines,
            _ => null,
        };
        if (key is null)
        {
            return rows;
        }

        // Empty values last either way, as on screen.
        var withValue = rows.Where(r => key(r) is not null);
        var ordered = descending ? withValue.OrderByDescending(key) : withValue.OrderBy(key);
        return [.. ordered, .. rows.Where(r => key(r) is null)];
    }

    /// <summary>One sheet: bold frozen header, then the rows; text, numbers and dates keep their type.</summary>
    private static void Sheet(XLWorkbook book, string name, string[] header, IEnumerable<object?[]> rows)
    {
        var sheet = book.Worksheets.Add(name);
        var widths = header.Select(h => h.Length).ToArray();
        for (var c = 0; c < header.Length; c++)
        {
            sheet.Cell(1, c + 1).Value = header[c];
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.SheetView.FreezeRows(1);

        var r = 2;
        foreach (var row in rows)
        {
            for (var c = 0; c < row.Length && c < header.Length; c++)
            {
                var cell = sheet.Cell(r, c + 1);
                switch (row[c])
                {
                    case null:
                        break;
                    case string text:
                        cell.Value = text;
                        break;
                    case DateTime at:
                        cell.Value = at;
                        cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                        break;
                    case int number:
                        cell.Value = number;
                        break;
                    case decimal number:
                        cell.Value = number;
                        break;
                    case double number:
                        cell.Value = number;
                        break;
                    default:
                        cell.Value = Convert.ToString(row[c], System.Globalization.CultureInfo.InvariantCulture);
                        break;
                }

                widths[c] = Math.Max(widths[c], row[c] is DateTime ? 16 : Convert.ToString(row[c], System.Globalization.CultureInfo.InvariantCulture)?.Length ?? 0);
            }

            r++;
        }

        // Fixed widths from the text length: no font measuring on the server.
        for (var c = 0; c < widths.Length; c++)
        {
            sheet.Column(c + 1).Width = Math.Min(60, widths[c] + 2);
        }
    }
}
