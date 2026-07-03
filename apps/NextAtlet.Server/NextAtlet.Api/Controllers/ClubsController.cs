using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.ClubRegistry.Commands;

namespace NextAtlet.Api.Controllers;

[ApiController]
[Route("api/clubs")]
public class ClubsController : ControllerBase
{
    private readonly ISender _sender;

    public ClubsController(ISender sender) => _sender = sender;

    /// <summary>
    /// Dev-only: runs the club-directory scraper(s) for a sport/country and upserts the results.
    /// Returns a short summary. AllowAnonymous for easy manual testing.
    /// </summary>
    [HttpPost("scrape")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Scrape([FromQuery] string sport = "judo", [FromQuery] string country = "denmark")
        => Ok(await _sender.Send(new ScrapeClubsCommand(sport, country)));

    /// <summary>

    /// </summary>
    [HttpPut("remove-sports")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveSports(Guid id, List<string> sportIds)
        => Ok(await _sender.Send(new RemoveSportsCommand(id, sportIds)));

    /// <summary>

    /// </summary>
    [HttpPut("add-sports")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddSports(Guid id, List<string> sportIds)
        => Ok(await _sender.Send(new AddSportsCommand(id, sportIds)));
}
