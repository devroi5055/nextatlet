using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Features.Account.Queries;

namespace NextAtlet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public class MeController : ControllerBase
{
    private readonly ISender _sender;

    public MeController(ISender sender) => _sender = sender;

    /// <summary>
    /// Domain-gate check for the authenticated caller: whether they've registered an athlete profile
    /// and what role they hold. Lets the frontend route new vs returning users.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<MeDto>> Get()
        => Ok(await _sender.Send(new GetCurrentUserQuery(User.GetAuthProviderId(), User.GetEmail())));
}
