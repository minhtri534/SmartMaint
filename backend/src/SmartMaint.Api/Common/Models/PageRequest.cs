namespace SmartMaint.Api.Common.Models;

/// <summary>Query-string paging for every list endpoint: ?page=1&amp;pageSize=20.</summary>
public record PageRequest
{
    public const int MaxPageSize = 100;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public int Skip => (Page - 1) * PageSize;
}
