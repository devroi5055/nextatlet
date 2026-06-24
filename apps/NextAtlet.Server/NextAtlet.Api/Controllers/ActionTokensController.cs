using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Features.ActionTokens.Commands;

// ClaimsPrincipalExtensions (User.GetAuthProviderId()/GetEmail()) live in the NextAtlet.Api namespace.

namespace NextAtlet.Api.Controllers;

[ApiController]
[Route("api/action-tokens")]
public class ActionTokensController : ControllerBase
{
    private readonly ISender _sender;

    public ActionTokensController(ISender sender) => _sender = sender;

    /// <summary>
    /// The single accept endpoint for every emailed-link flow (invite, guardian consent, org email
    /// verification). The token id in the URL is the secure link key; the action taken is selected by
    /// the token's type. Identity comes from the validated token, never the body.
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<Result>> Accept(Guid id)
        => Ok(await _sender.Send(new AcceptActionTokenCommand(id, User.TryGetAuthProviderId())));
}
