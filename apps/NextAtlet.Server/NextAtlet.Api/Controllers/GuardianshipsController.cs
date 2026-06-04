using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Features.Account.Commands;

namespace NextAtlet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/guardianships")]
public class GuardianshipsController : ControllerBase
{
    private readonly ISender _sender;

    public GuardianshipsController(ISender sender) => _sender = sender;

    /// <summary>
    /// The invited guardian accepts guardianship: claims their account and activates their pending
    /// guardian logins. This is the explicit consent step that unlocks publishing a minor's profile.
    /// </summary>
    [HttpPost("accept")]
    public async Task<ActionResult<GuardianshipAcceptedDto>> Accept()
        => Ok(await _sender.Send(new AcceptGuardianInviteCommand(User.GetAuthProviderId(), User.GetEmail())));
}
