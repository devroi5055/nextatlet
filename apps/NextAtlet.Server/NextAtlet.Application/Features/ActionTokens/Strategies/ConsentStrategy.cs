using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.ActionTokens.Models;
using NextAtlet.Application.Features.ActionTokens.Strategies;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Consent;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Organization;

public class ConsentStrategy : IActionTokenStrategy
{
    public ActionTokenType ActionTokenType => ActionTokenType.Consent;
    public bool authRequired => true;

    private readonly IGuardianConsentRepository _guardianConsentRepository;
    private readonly UserProvisioner _userProvisioner;
    private readonly IClock _clock;

    public ConsentStrategy(
        UserProvisioner userProvisioner,
        IClock clock,
        IGuardianConsentRepository guardianConsentRepository)
    {
        _userProvisioner = userProvisioner;
        _clock = clock;
        _guardianConsentRepository = guardianConsentRepository;
    }

    public async Task<Result> ExecuteAsync(ActionToken token, User? actorUser, CancellationToken ct)
    {
        if (actorUser is null)
            throw new InvalidOperationException("User should always be authenticated to enter this strategy");
        
        var consentExists = await _guardianConsentRepository.ExistsForProfileAsync(token.TargetSiteId, ct);
        if (consentExists)
            return Error.FromCode(ErrorCodes.ConsentNotNeeded);

        var payload = (ConsentPayload)token.Payload;

        var consent = new GuardianConsent
        {
            SiteId = token.TargetSiteId,
            GuardianUserId = actorUser.Id,
            MethodId = ConsentMethods.Email.Id,
            TermsVersion = payload.TermsVersion,
        };

        _guardianConsentRepository.Add(consent);

        return Result.Success();
    }
}