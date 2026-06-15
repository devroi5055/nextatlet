using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Features.Invitations.Commands;

namespace NextAtlet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly ISender _sender;

    public InvitationsController(ISender sender) => _sender = sender;

    /// <summary>
    /// The invited person claims their login. Role-agnostic — the invitation id carries who was
    /// invited, to which profile, and in what role. Replaces the old /guardianships/accept.
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<InvitationAcceptedDto>> Accept(Guid id)
        => Ok(await _sender.Send(new AcceptInvitationCommand(id, User.GetAuthProviderId(), User.GetEmail())));
}
