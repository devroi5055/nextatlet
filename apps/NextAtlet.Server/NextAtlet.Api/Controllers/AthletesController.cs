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
    public async Task<IActionResult> SelfRegister([FromBody] RegisterOwnAthleteRequest request)
        => Ok(await _sender.Send(new SelfRegisterAthleteCommand(
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
    public async Task<IActionResult> GuardianRegister([FromBody] RegisterChildAthleteRequest request)
        => Ok(await _sender.Send(new GuardianRegisterAthleteCommand(
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
    public async Task<IActionResult> Invite(Guid id, [FromBody] InviteToProfileRequest request)
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
    public async Task<IActionResult> TransferControl(Guid id, [FromBody] TransferControlRequest request)
        => Ok(await _sender.Send(new TransferControlCommand(id, User.GetAuthProviderId(), request.To)));

    /// <summary>
    /// Enables/disables shared editing — lets the non-controlling party edit the draft (+ media) but
    /// never publish, approve, or transfer. Does not change who controls. Only the controller may toggle it.
    /// </summary>
    [HttpPost("{id:guid}/collaboration")]
    public async Task<IActionResult> SetCollaboration(Guid id, [FromBody] SetCollaborationRequest request)
        => Ok(await _sender.Send(new SetCollaborationCommand(id, User.GetAuthProviderId(), request.SharedEditing)));

    /// <summary>
    /// Guardian gives consent (GDPR Art. 8) for a minor's profile by following the emailed link and
    /// authenticating. Records the consent and lifts the publish gate. Does not join the profile.
    /// </summary>
    [HttpPost("{id:guid}/consent")]
    public async Task<IActionResult> GiveConsent(Guid id)
        => Ok(await _sender.Send(new RecordGuardianConsentCommand(id, User.GetAuthProviderId(), User.GetEmail())));

    /// <summary>
    /// Gets the draft site snapshot for an athlete profile.
    /// </summary>
    [HttpGet("{id:guid}/config/draft")]
    public async Task<ActionResult<AthleteSiteSnapshotDto>> GetDraftConfig(Guid id)
        => Ok(await _sender.Send(new GetDraftAthleteSiteSnapshotQuery(id)));

    /// <summary>
    /// Replaces the draft site snapshot for an athlete profile.
    /// Runs validation, sanitization, and optimistic concurrency checks.
    /// </summary>
    [HttpPut("{id:guid}/config/draft")]
    public async Task<ActionResult<AthleteSiteSnapshotDto>> UpdateDraftConfig(Guid id, [FromBody] UpdateAthleteSiteSnapshotRequest request)
        => Ok(await _sender.Send(new EditDraftAthleteSiteSnapshotCommand(
            id,
            request.Layout,
            request.GlobalSettings,
            request.ExpectedVersion)));
}
