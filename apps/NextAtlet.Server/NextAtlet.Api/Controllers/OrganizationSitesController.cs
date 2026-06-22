using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Features.Organizations.Registration;
using NextAtlet.Domain.Enumerations.Organization;

// ClaimsPrincipalExtensions (User.GetAuthProviderId()/GetEmail()) live in the NextAtlet.Api namespace.

namespace NextAtlet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrganizationSitesController : ControllerBase
{
    private readonly ISender _sender;

    public OrganizationSitesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Self-registration: the authenticated caller registers their own profile (becomes AthleteOwner;
    /// a guardian is invited if the caller is a minor).
    /// </summary>
    [HttpPost("club-register")]
    public async Task<IActionResult> ClubRegister([FromBody] ClubRegisterRequest request)
        => Ok(await _sender.Send(new RegisterOrganizationSiteCommand(
            User.GetAuthProviderId(),
            User.GetEmail(),
            request.Slug,
            request.DisplayName,
            request.PlanTierId,
            request.DefaultLocaleId,
            OrganizationType.Club.Id)));

    [HttpPost("send-offical-email-verification")]
    public async Task<IActionResult> SendOfficialEmailVerification([FromBody] SendOfficialEmailVerificationCommand request)
        => Ok(await _sender.Send(request));


}
