using Microsoft.EntityFrameworkCore;

namespace SmartMaint.Api.Common.Models;

/// <summary>Response shape of every list endpoint.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public static async Task<PagedResult<T>> CreateAsync(
        IQueryable<T> query, PageRequest request, CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, request.Page, request.PageSize, total);
    }
}
