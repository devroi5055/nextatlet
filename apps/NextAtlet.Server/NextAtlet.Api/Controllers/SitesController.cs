using MediatR;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.Models;
using NextAtlet.Application.Contracts.Sites.Request;
using NextAtlet.Application.Contracts.Sites.Response;
using NextAtlet.Application.Features.Sites;

namespace NextAtlet.Api.Controllers;

[ApiController]
[Route("api/sites")]
public class SitesController : ControllerBase
{
    private readonly ISender _sender;

    public SitesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Lists sites as a paged result. All paging/sorting/filtering comes from the query string
    /// (<c>?page=&amp;pageSize=&amp;sortBy=&amp;sortDescending=&amp;search=&amp;siteType=&amp;visibility=</c>),
    /// so a given list view round-trips through the URL and is shareable/bookmarkable.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] SiteListRequest request)
        => Ok(await _sender.Send(new GetSitesQuery(request)));
}
