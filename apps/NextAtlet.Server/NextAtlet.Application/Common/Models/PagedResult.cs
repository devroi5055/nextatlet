namespace NextAtlet.Application.Common.Models;

/// <summary>
/// One page of a larger result set plus the metadata a client needs to render paging controls. The
/// single shape every paged listing returns, so the frontend handles them uniformly.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
