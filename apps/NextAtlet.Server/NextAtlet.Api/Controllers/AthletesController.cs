using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Athletes.Queries;
using NextAtlet.Application.Features.Invitations.Commands;

// ClaimsPrincipalExtensions (User.GetAuthProviderId()/GetEmail()) live in the NextAtlet.Api namespace.

namespace NextAtlet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AthletesController : ControllerBase
{
    private readonly ISender _sender;

    public AthletesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Self-registration: the authenticated caller registers their own profile (becomes AthleteOwner;
    /// a guardian is invited if the caller is a minor).
    /// </summary>
    [HttpPost("self-register")]
    public async Task<ActionResult<AthleteProfileDto>> SelfRegister([FromBody] RegisterOwnAthleteRequest request)
    {
        var dto = await _sender.Send(new SelfRegisterAthleteCommand(
            User.GetAuthProviderId(),
            User.GetEmail(),
            request.DisplayName,
            request.Slug,
            request.DateOfBirth,
            request.DefaultLocaleId,
            request.GuardianEmail));

        return Created($"/api/athletes/{dto.Id}", dto);
    }

    /// <summary>
    /// Guardian registers a profile for their child: the authenticated caller becomes the Guardian.
    /// </summary>
    [HttpPost("guardian-register")]
    public async Task<ActionResult<AthleteProfileDto>> GuardianRegister([FromBody] RegisterChildAthleteRequest request)
    {
        var dto = await _sender.Send(new GuardianRegisterAthleteCommand(
            User.GetAuthProviderId(),
            User.GetEmail(),
            request.ChildDisplayName,
            request.Slug,
            request.ChildDateOfBirth,
            request.DefaultLocaleId));

        return Created($"/api/athletes/{dto.Id}", dto);
    }

    /// <summary>
    /// Invite a person (by email) to this profile in a given role. Only a caller holding an active
    /// login on the profile may invite to it. The invited person claims it at /invitations/{id}/accept.
    /// </summary>
    [HttpPost("{id:guid}/invite")]
    public async Task<ActionResult<InvitationDto>> Invite(Guid id, [FromBody] InviteToProfileRequest request)
    {
        var dto = await _sender.Send(new InviteToProfileCommand(
            id,
            User.GetAuthProviderId(),
            User.GetEmail(),
            request.Email,
            request.Role));

        return Created($"/api/invitations/{dto.Id}/accept", dto);
    }

    /// <summary>
    /// Gets the draft SiteConfig for an athlete profile.
    /// </summary>
    [HttpGet("{id:guid}/config/draft")]
    public async Task<ActionResult<SiteConfigDto>> GetDraftConfig(Guid id)
        => Ok(await _sender.Send(new GetDraftSiteConfigQuery(id)));

    /// <summary>
    /// Updates the draft SiteConfig for an athlete profile.
    /// Runs validation, sanitization, and optimistic concurrency checks.
    /// </summary>
    [HttpPut("{id:guid}/config/draft")]
    public async Task<ActionResult<SiteConfigDto>> UpdateDraftConfig(Guid id, [FromBody] UpdateSiteConfigRequest request)
        => Ok(await _sender.Send(new EditDraftSiteConfigCommand(
            id,
            request.Layout,
            request.GlobalSettings,
            request.ExpectedVersion)));
}
