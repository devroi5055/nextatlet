using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.ActionTokens.Models;
using NextAtlet.Application.Features.ActionTokens.Strategies;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Organization;

public class OrgEmailVerificationStrategy : IActionTokenStrategy
{
    public ActionTokenType ActionTokenType => ActionTokenType.OrgEmailVerification;
    public bool authRequired => false;

    private readonly IOrganizationProfileRepository _orgProfiles;
    private readonly UserProvisioner _userProvisioner;
    private readonly IClock _clock;

    public OrgEmailVerificationStrategy(
        IOrganizationProfileRepository orgProfiles,
        UserProvisioner userProvisioner,
        IClock clock)
    {
        _orgProfiles = orgProfiles;
        _userProvisioner = userProvisioner;
        _clock = clock;
    }

    public async Task<Result> ExecuteAsync(ActionToken token, User? actorUser, CancellationToken ct)
    {
        var payload = (OrgEmailVerificationPayload)token.Payload;

        var org = await _orgProfiles.GetBySiteIdAsync(token.TargetSiteId, ct);
        if (org is null)
            return Error.FromCode(ErrorCodes.OrganizationProfileNotFound);

        org.VerificationStatusId = VerificationStatus.Verified.Id;

        org.Verification = new OrgVerification
        {
            //add the userId if the user is logged in - Probably not
            VerifiedByUserId = actorUser?.Id,
            //add the email that the verification was sent to
            VerifiedByEmail = payload.Email,
            MethodId = VerificationMethod.Email.Id,
            VerifiedUtc = _clock.UtcNow
        };

        return Result.Success();
    }
}