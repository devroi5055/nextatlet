using MediatR;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Athletes.Queries;

namespace NextAtlet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AthletesController : ControllerBase
{
    private readonly ISender _sender;

    public AthletesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Creates a new athlete profile with an AthleteOwner login and (if minor) a Pending guardian link.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AthleteProfileDto>> CreateAthlete([FromBody] CreateAthleteRequest request)
    {
        // Owner identity is resolved from the authenticated token inside the handler (ICurrentUserContext).
        var dto = await _sender.Send(new RegisterAthleteProfileCommand(
            request.DisplayName,
            request.Slug,
            request.DateOfBirth,
            request.DefaultLocale.Id,
            request.GuardianEmail));

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
