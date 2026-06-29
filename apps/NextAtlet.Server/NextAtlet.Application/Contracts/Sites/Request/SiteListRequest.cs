using NextAtlet.Application.Common.Models;

namespace NextAtlet.Application.Contracts.Sites.Request;

/// <summary>
/// Query-string parameters for listing sites: paging/sorting/search from <see cref="PagedQuery"/> plus
/// the site-specific filters. Bound via <c>[FromQuery]</c> so the whole list view round-trips through
/// the URL. Sortable keys: <c>slug</c>, <c>displayName</c>, <c>createdUtc</c>, <c>updatedUtc</c>.
/// </summary>
public class SiteListRequest : PagedQuery
{
    /// <summary>Filter by <c>SiteType</c> id (e.g. "individual" | "organization"); null = any.</summary>
    public string? SiteType { get; set; }

    /// <summary>Filter by <c>VisibilityState</c> id (e.g. "public" | "private"); null = any.</summary>
    public string? Visibility { get; set; }
}
