using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Models;

namespace NextAtlet.Infrastructure.Persistence.Querying;

/// <summary>
/// Reusable base for paged listing queries: applies whitelisted sorting and paging to an
/// <see cref="IQueryable{T}"/> and materializes a <see cref="PagedResult{T}"/> (one COUNT + one page
/// query). A concrete listing supplies a <see cref="DefaultSort"/>, a <see cref="SortMap"/> of allowed
/// sort keys, and (optionally) <see cref="ApplyFilters"/> — everything else (clamped paging, asc/desc,
/// total count) is shared. To add a new listing: subclass with the entity and its query type.
/// </summary>
/// <typeparam name="TEntity">The entity being listed.</typeparam>
/// <typeparam name="TQuery">The listing's URL-bound query (paging/sort/filters).</typeparam>
public abstract class PagedQueryBuilder<TEntity, TQuery>
    where TEntity : class
    where TQuery : PagedQuery
{
    /// <summary>Order applied when the request names no (or an unknown) sort key.</summary>
    protected abstract Expression<Func<TEntity, object>> DefaultSort { get; }

    /// <summary>Allowed sort keys → their selectors. Keys are matched case-insensitively against <c>SortBy</c>.</summary>
    protected abstract IReadOnlyDictionary<string, Expression<Func<TEntity, object>>> SortMap { get; }

    /// <summary>Entity-specific filtering (search + any typed filters). Default: no filtering.</summary>
    protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, TQuery request) => query;

    public async Task<PagedResult<TEntity>> BuildAsync(IQueryable<TEntity> source, TQuery request, CancellationToken cancellationToken)
    {
        var filtered = ApplyFilters(source, request);

        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await ApplySort(filtered, request)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>(items, request.Page, request.PageSize, totalCount);
    }

    private IQueryable<TEntity> ApplySort(IQueryable<TEntity> query, TQuery request)
    {
        var selector = request.SortBy is not null && SortMap.TryGetValue(request.SortBy, out var mapped)
            ? mapped
            : DefaultSort;

        return request.SortDescending ? query.OrderByDescending(selector) : query.OrderBy(selector);
    }
}
