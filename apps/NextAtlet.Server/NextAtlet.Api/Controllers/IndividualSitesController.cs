using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Contracts.Individuals.Request;
using NextAtlet.Application.Contracts.Invitations.Response;
using NextAtlet.Application.Contracts.Sites.Response;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Individuals.Control;
using NextAtlet.Application.Features.Individuals.Registration;
using NextAtlet.Application.Features.Invitations.Commands;
using NextAtlet.Application.Features.Sites;
using NextAtlet.Application.Contracts.Invitations.Request;

// ClaimsPrincipalExtensions (User.GetAuthProviderId()/GetEmail()) live in the NextAtlet.Api namespace.

namespace NextAtlet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IndividualSitesController : ControllerBase
{
    private readonly ISender _sender;

    public IndividualSitesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Self-registration: the authenticated caller registers their own profile (becomes AthleteOwner;
    /// a guardian is invited if the caller is a minor).
    /// </summary>
    [HttpPost("self-register")]
    [ProducesResponseType(typeof(SiteResponse), 200)]
    public async Task<IActionResult> SelfRegister([FromBody] RegisterIndividualSiteSelfRequest request)
        => Ok(await _sender.Send(new RegisterIndividualSiteSelfCommand(
            User.GetAuthProviderId(),
            User.GetEmail(),
            request.DisplayName,
            request.Slug,
            request.DateOfBirth,
            request.DefaultLocaleId,
            request.GuardianEmail)));

    /// <summary>
    /// Guardian registers a profile for their child: the authenticated caller becomes the Guardian.
    /// </summary>
    [HttpPost("guardian-register")]
    [ProducesResponseType(typeof(SiteResponse), 200)]
    public async Task<IActionResult> GuardianRegister([FromBody] RegisterIndividualSiteGuardianRequest request)
        => Ok(await _sender.Send(new RegisterIndividualSiteGuardianCommand(
            User.GetAuthProviderId(),
            User.GetEmail(),
            request.ChildDisplayName,
            request.Slug,
            request.ChildDateOfBirth,
            request.DefaultLocaleId)));

    /// <summary>
    /// Invite a person (by email) to this profile in a given role. Only a caller holding an active
    /// login on the profile may invite to it. The invited person claims it at /invitations/{id}/accept.
    /// </summary>
    [HttpPost("{id:guid}/invite")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Invite(Guid id, [FromBody] InviteToSiteRequest request)
        => Ok(await _sender.Send(new InviteToProfileCommand(
            id,
            User.GetAuthProviderId(),
            User.GetEmail(),
            request.Email,
            request.Role)));

    /// <summary>
    /// Transfers control of the profile to the other party ("athlete" | "guardian"). Only the current
    /// controller may initiate; guardian→athlete is age-gated; the receiving side's login must exist.
    /// </summary>
    [HttpPost("{id:guid}/transfer-control")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> TransferControl(Guid id, [FromBody] TransferControlRequest request)
        => Ok(await _sender.Send(new TransferControlCommand(id, User.GetAuthProviderId(), request.To)));

    /// <summary>
    /// Enables/disables shared editing — lets the non-controlling party edit the draft (+ media) but
    /// never publish, approve, or transfer. Does not change who controls. Only the controller may toggle it.
    /// </summary>
    [HttpPost("{id:guid}/collaboration")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetCollaboration(Guid id, [FromBody] SetCollaborationRequest request)
        => Ok(await _sender.Send(new SetCollaborationCommand(id, User.GetAuthProviderId(), request.SharedEditing)));

    // Guardian consent is no longer a profileId-keyed endpoint here — it flows through the secure
    // action-token link: POST /api/action-tokens/{id}/accept (the token's type runs the consent case).

    /// <summary>
    /// Gets the draft site snapshot for an athlete profile.
    /// </summary>
    [HttpGet("{id:guid}/config/draft")]
    [ProducesResponseType(typeof(SiteSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteSnapshotResponse>> GetDraftConfig(Guid id)
        => Ok(await _sender.Send(new GetDraftAthleteSiteSnapshotQuery(id)));

    // TODO: Re-enable once the draft-edit write path is rebuilt. EditDraftAthleteSiteSnapshotCommand was
    // removed during the Site/SiteSnapshot refactor (SiteSnapshot no longer carries a Version for the
    // optimistic-concurrency check this endpoint relied on).
    /// <summary>
    /// Replaces the draft site snapshot for an athlete profile.
    /// Runs validation, sanitization, and optimistic concurrency checks.
    /// </summary>
    //[HttpPut("{id:guid}/config/draft")]
    //public async Task<ActionResult<SiteSnapshotResponse>> UpdateDraftConfig(Guid id, [FromBody] UpdateAthleteSiteSnapshotRequest request)
    //    => Ok(await _sender.Send(new EditDraftAthleteSiteSnapshotCommand(
    //        id,
    //        request.Layout,
    //        request.GlobalSettings,
    //        request.ExpectedVersion)));
}
