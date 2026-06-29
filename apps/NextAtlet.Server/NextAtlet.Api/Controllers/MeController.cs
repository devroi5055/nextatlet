using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Contracts.Identity.Response;
using NextAtlet.Application.Features.Identity;

namespace NextAtlet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MeController : ControllerBase
{
    private readonly ISender _sender;

    public MeController(ISender sender) => _sender = sender;

    /// <summary>
    /// Domain-gate check for the authenticated caller: whether they've registered an athlete profile
    /// and what role they hold. Lets the frontend route new vs returning users.
    /// </summary>
    [HttpGet(Name = "GetMe")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
        => Ok(await _sender.Send(new GetCurrentUserQuery(User.GetAuthProviderId(), User.GetEmail())));
}
