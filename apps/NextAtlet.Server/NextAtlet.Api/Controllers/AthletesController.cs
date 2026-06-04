using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Athletes.Queries;

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
    [HttpPost("register")]
    public async Task<ActionResult<AthleteProfileDto>> RegisterOwn([FromBody] RegisterOwnAthleteRequest request)
    {
        var dto = await _sender.Send(new RegisterOwnAthleteCommand(
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
    [HttpPost("register-child")]
    public async Task<ActionResult<AthleteProfileDto>> RegisterChild([FromBody] RegisterChildAthleteRequest request)
    {
        var dto = await _sender.Send(new RegisterChildAthleteCommand(
            User.GetAuthProviderId(),
            User.GetEmail(),
            request.ChildDisplayName,
            request.Slug,
            request.ChildDateOfBirth,
            request.DefaultLocaleId));

        return Created($"/api/athletes/{dto.Id}", dto);
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
