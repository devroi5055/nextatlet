using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Mapping;
using NextAtlet.Application.Common.Models;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Contracts.Sites.Request;
using NextAtlet.Application.Contracts.Sites.Response;

namespace NextAtlet.Application.Features.Sites;

/// <summary>
/// Lists sites as a <see cref="PagedResult{T}"/>, filtered/sorted/paged by the URL-bound
/// <see cref="SiteListRequest"/>. The repository owns the EF query (via the reusable paged-query
/// builder); this handler just maps the page of entities onto the public <see cref="SiteResponse"/>.
/// </summary>
public record GetSitesQuery(SiteListRequest Filter) : IRequest<Result<PagedResult<SiteResponse>>>;

public class GetSitesQueryHandler : IRequestHandler<GetSitesQuery, Result<PagedResult<SiteResponse>>>
{
    private readonly ISiteRepository _sites;

    public GetSitesQueryHandler(ISiteRepository sites) => _sites = sites;

    public async Task<Result<PagedResult<SiteResponse>>> Handle(GetSitesQuery request, CancellationToken cancellationToken)
    {
        var page = await _sites.GetPagedAsync(request.Filter, cancellationToken);

        var items = page.Items.Select(SiteMapper.ToResponse).ToList();
        return new PagedResult<SiteResponse>(items, page.Page, page.PageSize, page.TotalCount);
    }
}
