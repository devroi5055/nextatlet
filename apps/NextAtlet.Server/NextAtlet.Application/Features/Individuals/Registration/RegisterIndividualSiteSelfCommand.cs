using MediatR;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Contracts.Sites.Response;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Policies;

namespace NextAtlet.Application.Features.Individuals.Registration;

/// <summary>
/// Self-registration: the authenticated caller registers an IndividualProfile for themselves and becomes
/// its AthleteOwner — so the profile is always <see cref="ControlModes.AthleteControlled"/>. Below the
/// absolute minimum age it is rejected. Below the self-consent age (GDPR Art. 8) the profile is created
/// <see cref="ConsentStates.PendingGuardianConsent"/> and a consent-request <b>email</b> is sent to the
/// guardian — they confirm at the consent endpoint, which records consent and lifts the publish gate.
/// No profile invitation is issued here: a guardian <i>joining</i> the profile is a separate,
/// owner-initiated step. Identity comes from the token, never the body.
/// </summary>
public record RegisterIndividualSiteSelfCommand(
    string AuthProviderId,
    string Email,
    string DisplayName,
    string Slug,
    DateTime DateOfBirth,
    string DefaultLocaleId,
    string? GuardianEmail) : IRequest<Result<SiteResponse>>;

public class RegisterIndividualSiteSelfCommandHandler
    : IndividualSiteRegistrationHandlerBase, IRequestHandler<RegisterIndividualSiteSelfCommand, Result<SiteResponse>>
{
    private readonly AgeThresholdOptions _thresholds;
    private readonly IEmailService _email;
    private readonly IActionTokenRepository _tokens;
    private readonly TermsOptions _terms;
    private readonly InvitationOptions _invitationOptions;

    public RegisterIndividualSiteSelfCommandHandler(
        ISiteRepository sites,
        ISiteLoginRepository logins,
        IIndividualProfileRepository profiles,
        IThemeRepository themes,
        ISiteSnapshotRepository siteSnapshots,
        UserProvisioner userProvisioner,
        IClock clock,
        IOptions<AgeThresholdOptions> ageThresholds,
        IEmailService email,
        IActionTokenRepository tokens,
        IOptions<TermsOptions> terms,
        IOptions<InvitationOptions> invitationOptions,
        IUnitOfWork unitOfWork)
        : base(sites, logins, profiles, themes, siteSnapshots, userProvisioner, clock, ageThresholds.Value, unitOfWork)
    {
        _thresholds = ageThresholds.Value;
        _email = email;
        _tokens = tokens;
        _terms = terms.Value;
        _invitationOptions = invitationOptions.Value;
    }

    public async Task<Result<SiteResponse>> Handle(RegisterIndividualSiteSelfCommand request, CancellationToken cancellationToken)
    {
        // Age gates only — never a permission input. Below the absolute floor → cannot register at all.
        var today = DateOnly.FromDateTime(_clock.UtcNow);
        var dob = DateOnly.FromDateTime(request.DateOfBirth);
        if (AgePolicy.AgeAt(dob, today) < _thresholds.AbsoluteMinimumAge)
            return Error.FromCode(ErrorCodes.BelowMinimumAge);

        // Below the self-consent age a guardian must consent → we need their email to send the request.
        var needsConsent = AgePolicy.RequiresGuardianConsent(request.DateOfBirth, _clock.UtcNow, _thresholds.SelfConsentAge);
        if (needsConsent && string.IsNullOrWhiteSpace(request.GuardianEmail))
            return Error.FromCode(ErrorCodes.GuardianEmailRequired);

        // The guardian must be someone other than the athlete themselves.
        if (needsConsent && string.Equals(request.GuardianEmail, request.Email, StringComparison.OrdinalIgnoreCase))
            return Error.FromCode(ErrorCodes.GuardianEmailRequired);

        var caller = await GetOrCreateUserAsync(request.Email, request.AuthProviderId, cancellationToken);

        // A user can't self-register two owned sites.
        if (await _sites.GetOwnedByUserIdAsync(caller.Id, cancellationToken) is not null)
            return Error.FromCode(ErrorCodes.SiteAlreadyExists);

        // Self-register always starts AthleteControlled — the athlete chose to create their own profile.
        var created = await CreateIndividualProfileCoreAsync(
            request.Slug, request.DisplayName, request.DateOfBirth, request.DefaultLocaleId,
            ControlModes.AthleteControlled, cancellationToken);
        if (!created.IsSuccess)
            return created.Error!;
        var siteDto = created.Value!;

        _logins.Add(SiteLogin.CreateAthlete(caller.Id, siteDto.Id));

        // Consent request is a tokened link (Consent action token) — NOT a profile invitation
        // (consent ≠ joining). The token is staged in the same transaction; the link is emailed after
        // commit so a rolled-back registration never emails. A guardian joins later, if at all, via a
        // separate owner-initiated invitation.
        ActionToken? consentToken = null;
        if (needsConsent)
        {
            consentToken = ActionToken.Issue(
                ActionTokenType.Consent.Id,
                siteDto.Id,
                new ConsentPayload { Email = request.GuardianEmail!, TermsVersion = _terms.CurrentVersion },
                expiresUtc: _clock.UtcNow.AddDays(_invitationOptions.ExpiryDays));
            _tokens.Add(consentToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (consentToken is not null)
            await _email.SendConsentRequestAsync(request.GuardianEmail!, consentToken.Id, cancellationToken);

        return siteDto;
    }
}
