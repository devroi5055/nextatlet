using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Contracts.Sites.Request;
using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Infrastructure.Persistence.Querying;

/// <summary>
/// The <see cref="Site"/> listing's filter + sort rules for <see cref="PagedQueryBuilder{TEntity,TQuery}"/>.
/// Search matches slug/display-name (case-insensitive); optional type/visibility filters narrow further.
/// </summary>
public sealed class SiteListQueryBuilder : PagedQueryBuilder<Site, SiteListRequest>
{
    protected override Expression<Func<Site, object>> DefaultSort => s => s.CreatedUtc;

    protected override IReadOnlyDictionary<string, Expression<Func<Site, object>>> SortMap { get; } =
        new Dictionary<string, Expression<Func<Site, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["slug"]        = s => s.Slug,
            ["displayName"] = s => s.DisplayName,
            ["createdUtc"]  = s => s.CreatedUtc,
            ["updatedUtc"]  = s => s.UpdatedUtc,
        };

    protected override IQueryable<Site> ApplyFilters(IQueryable<Site> query, SiteListRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Slug, pattern) ||
                EF.Functions.ILike(s.DisplayName, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.SiteType))
            query = query.Where(s => s.SiteTypeId == request.SiteType);

        if (!string.IsNullOrWhiteSpace(request.Visibility))
            query = query.Where(s => s.VisibilityStateId == request.Visibility);

        return query;
    }
}
