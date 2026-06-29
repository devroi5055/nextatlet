namespace NextAtlet.Application.Common.Models;

/// <summary>
/// Base for any URL-bound listing query: paging + sorting + free-text search. Bound from query-string
/// parameters (<c>[FromQuery]</c>) so a list view is fully expressible — and therefore shareable and
/// bookmarkable — as a URL. Values are clamped to safe ranges so a hand-edited URL can't request an
/// unbounded page. Derive a per-listing request from this and add its own filter fields.
/// </summary>
public abstract class PagedQuery
{
    public const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>1-based page number (clamped to a minimum of 1).</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Items per page (clamped to 1..<see cref="MaxPageSize"/>).</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    /// <summary>Sort key — a whitelisted field name; unknown/absent falls back to the listing's default order.</summary>
    public string? SortBy { get; set; }

    /// <summary>Sort descending when true; ascending otherwise.</summary>
    public bool SortDescending { get; set; }

    /// <summary>Optional free-text filter; the matched fields are defined per listing.</summary>
    public string? Search { get; set; }
}
