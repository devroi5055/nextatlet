using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.DTOs;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Athletes.Queries;

namespace NextAtlet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AthletesController : ControllerBase
{
    private readonly CreateAthleteCommand _createAthleteCommand;
    private readonly GetDraftConfigQuery _getDraftConfigQuery;
    private readonly UpdateDraftConfigCommand _updateDraftConfigCommand;

    public AthletesController(
        CreateAthleteCommand createAthleteCommand,
        GetDraftConfigQuery getDraftConfigQuery,
        UpdateDraftConfigCommand updateDraftConfigCommand)
    {
        _createAthleteCommand = createAthleteCommand;
        _getDraftConfigQuery = getDraftConfigQuery;
        _updateDraftConfigCommand = updateDraftConfigCommand;
    }

    /// <summary>
    /// Creates a new athlete profile with an AthleteOwner login and (if minor) a Pending guardian link.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AthleteProfileDto>> CreateAthlete([FromBody] CreateAthleteRequest request)
    {
        try
        {
            // TODO: In production, extract these from JWT token claims
            var authProviderId = Guid.NewGuid().ToString(); // Placeholder
            var userEmail = request.Email;

            var profile = await _createAthleteCommand.ExecuteAsync(
                userEmail,
                authProviderId,
                request.DisplayName,
                request.Slug,
                request.DateOfBirth,
                request.DefaultLocale,
                request.GuardianEmail);

            var dto = new AthleteProfileDto
            {
                Id = profile.Id,
                Slug = profile.Slug,
                DisplayName = profile.DisplayName,
                DateOfBirth = profile.DateOfBirth,
                IsMinor = profile.IsMinor,
                DefaultLocale = profile.DefaultLocale
            };

            return Created($"/api/athletes/{profile.Id}", dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { StatusCode = 400, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { StatusCode = 500, Message = "Internal server error", Details = new List<string> { ex.Message } });
        }
    }

    /// <summary>
    /// Gets the draft SiteConfig for an athlete profile.
    /// </summary>
    [HttpGet("{id:guid}/config/draft")]
    public async Task<ActionResult<SiteConfigDto>> GetDraftConfig(Guid id)
    {
        try
        {
            // TODO: In production, verify the caller has access to this profile
            var config = await _getDraftConfigQuery.ExecuteAsync(id);
            return Ok(config);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse { StatusCode = 404, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { StatusCode = 500, Message = "Internal server error", Details = new List<string> { ex.Message } });
        }
    }

    /// <summary>
    /// Updates the draft SiteConfig for an athlete profile.
    /// Runs validation, sanitization, and optimistic concurrency checks.
    /// </summary>
    [HttpPut("{id:guid}/config/draft")]
    public async Task<ActionResult<SiteConfigDto>> UpdateDraftConfig(Guid id, [FromBody] UpdateSiteConfigRequest request)
    {
        try
        {
            // TODO: In production, verify the caller has edit permission on this profile
            var config = await _updateDraftConfigCommand.ExecuteAsync(
                id,
                request.Layout,
                request.GlobalSettings,
                request.ExpectedVersion);

            var dto = new SiteConfigDto
            {
                Id = config.Id,
                AthleteProfileId = config.AthleteProfileId,
                State = config.State,
                Layout = config.Layout,
                GlobalSettings = config.GlobalSettings,
                Version = config.Version
            };

            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { StatusCode = 400, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { StatusCode = 500, Message = "Internal server error", Details = new List<string> { ex.Message } });
        }
    }
}
