using MediatR;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.Policies;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Self-registration: the authenticated caller registers an AthleteProfile for themselves and becomes
/// its AthleteOwner — so the profile is always <see cref="ControlMode.AthleteControlled"/>. Below the
/// absolute minimum age it is rejected. Below the self-consent age (GDPR Art. 8) the profile is created
/// <see cref="ConsentState.PendingGuardianConsent"/> and a consent-request <b>email</b> is sent to the
/// guardian — they confirm at the consent endpoint, which records consent and lifts the publish gate.
/// No profile invitation is issued here: a guardian <i>joining</i> the profile is a separate,
/// owner-initiated step. Identity comes from the token, never the body.
/// </summary>
public record SelfRegisterAthleteCommand(
    string AuthProviderId,
    string Email,
    string DisplayName,
    string Slug,
    DateTime DateOfBirth,
    string DefaultLocaleId,
    string? GuardianEmail) : IRequest<AthleteProfileDto>;

public class SelfRegisterAthleteCommandHandler
    : AthleteRegistrationHandlerBase, IRequestHandler<SelfRegisterAthleteCommand, AthleteProfileDto>
{
    private readonly AgeThresholdOptions _thresholds;
    private readonly IEmailService _email;

    public SelfRegisterAthleteCommandHandler(
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IThemeRepository themes,
        ISiteConfigRepository siteConfigs,
        UserProvisioner userProvisioner,
        InvitationIssuer inviter,
        IClock clock,
        IOptions<AgeThresholdOptions> ageThresholds,
        IEmailService email,
        IUnitOfWork unitOfWork)
        : base(profiles, logins, themes, siteConfigs, userProvisioner, inviter, clock, unitOfWork)
    {
        _thresholds = ageThresholds.Value;
        _email = email;
    }

    public async Task<AthleteProfileDto> Handle(SelfRegisterAthleteCommand request, CancellationToken cancellationToken)
    {
        // Age gates only — never a permission input. Below the absolute floor → cannot register at all.
        var today = DateOnly.FromDateTime(Clock.UtcNow);
        if (AgePolicy.AgeAt(DateOnly.FromDateTime(request.DateOfBirth), today) < _thresholds.AbsoluteMinimumAge)
            throw new DomainException(ErrorCodes.BelowMinimumAge);

        // Below the self-consent age a guardian must consent → we need their email to send the request.
        var needsConsent = AgePolicy.RequiresGuardianConsent(request.DateOfBirth, Clock.UtcNow, _thresholds.SelfConsentAge);
        if (needsConsent && string.IsNullOrWhiteSpace(request.GuardianEmail))
            throw new DomainException(ErrorCodes.GuardianEmailRequired);

        // The guardian must be someone other than the athlete themselves.
        if (needsConsent && string.Equals(request.GuardianEmail, request.Email, StringComparison.OrdinalIgnoreCase))
            throw new DomainException(ErrorCodes.GuardianEmailRequired);

        var caller = await GetOrCreateUserAsync(request.Email, request.AuthProviderId, cancellationToken);

        // A user can't self-register two owned profiles.
        if (await Profiles.GetOwnedByUserIdAsync(caller.Id, cancellationToken) is not null)
            throw new DomainException(ErrorCodes.ProfileAlreadyExists);

        // Self-register always starts AthleteControlled — the athlete chose to create their own profile.
        var profile = await CreateAthleteProfileCoreAsync(
            request.Slug, request.DisplayName, request.DateOfBirth, request.DefaultLocaleId,
            ControlMode.AthleteControlled, cancellationToken);

        // Below self-consent age → publish-gated pending guardian verification; otherwise no consent needed.
        profile.ConsentState = needsConsent ? ConsentState.PendingGuardianConsent : ConsentState.NotRequired;

        Logins.Add(ProfileLogin.CreateOwner(caller.Id, profile.Id));

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        // Consent request is an email to a consent endpoint — NOT a profile invitation (consent ≠ joining).
        // Sent after commit so a rolled-back registration never emails. A guardian joins later, if at all,
        // via a separate owner-initiated invitation.
        if (needsConsent)
            await _email.SendConsentRequestAsync(request.GuardianEmail!, profile.Id, cancellationToken);

        return MapToDto(profile);
    }
}
