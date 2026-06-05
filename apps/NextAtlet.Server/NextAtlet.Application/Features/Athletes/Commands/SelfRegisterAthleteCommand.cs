using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.Policies;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Self-registration: the authenticated caller registers an AthleteProfile for themselves and becomes
/// its AthleteOwner — so the profile is always <see cref="ControlMode.AthleteControlled"/>. Under-13 is
/// rejected (self-consent floor). A 13–17 athlete must name a guardian (invited in the same
/// transaction); 13–15 must also confirm parental consent. Identity comes from the token, never the body.
/// </summary>
public record SelfRegisterAthleteCommand(
    string AuthProviderId,
    string Email,
    string DisplayName,
    string Slug,
    DateTime DateOfBirth,
    string DefaultLocaleId,
    string? GuardianEmail = null,
    bool ParentalConsentConfirmed = false) : IRequest<AthleteProfileDto>;

public class SelfRegisterAthleteCommandHandler
    : AthleteRegistrationHandlerBase, IRequestHandler<SelfRegisterAthleteCommand, AthleteProfileDto>
{
    public SelfRegisterAthleteCommandHandler(
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IThemeRepository themes,
        ISiteConfigRepository siteConfigs,
        UserProvisioner userProvisioner,
        InvitationIssuer inviter,
        IUnitOfWork unitOfWork)
        : base(profiles, logins, themes, siteConfigs, userProvisioner, inviter, unitOfWork) { }

    public async Task<AthleteProfileDto> Handle(SelfRegisterAthleteCommand request, CancellationToken cancellationToken)
    {
        // Age band is a gate, never a permission input. Under-13 cannot self-register (self-consent floor).
        var band = AgePolicy.BandToday(request.DateOfBirth);
        if (band == AgeBand.BelowMinimum)
            throw new DomainException(ErrorCodes.BelowMinimumAge);

        // 13–17 must name a guardian (invited below). 16+ self-consents; only 13–15 declares parental consent.
        var needsGuardian = band is AgeBand.YoungMinor or AgeBand.OlderMinor;
        if (needsGuardian && string.IsNullOrWhiteSpace(request.GuardianEmail))
            throw new DomainException(ErrorCodes.GuardianEmailRequired);
        if (band == AgeBand.YoungMinor && !request.ParentalConsentConfirmed)
            throw new DomainException(ErrorCodes.ParentalConsentRequired);

        var caller = await GetOrCreateUserAsync(request.Email, request.AuthProviderId, cancellationToken);

        // A user can't self-register two owned profiles.
        if (await Profiles.GetOwnedByUserIdAsync(caller.Id, cancellationToken) is not null)
            throw new DomainException(ErrorCodes.ProfileAlreadyExists);

        // Self-register always starts AthleteControlled — the athlete chose to create their own profile.
        var profile = await CreateAthleteProfileCoreAsync(
            request.Slug, request.DisplayName, request.DateOfBirth, request.DefaultLocaleId,
            ControlMode.AthleteControlled, cancellationToken);

        if (band == AgeBand.YoungMinor)
            profile.ConsentCapturedUtc = DateTime.UtcNow; // the checkbox declaration; the guardian accept is the verifiable act

        Logins.Add(ProfileLogin.CreateOwner(caller.Id, profile.Id));

        // A named guardian is invited in the SAME transaction (required 13–17, optional 18+). The
        // guardian's ProfileLogin is materialized when they accept; until then the Invitation is the
        // pending state. A self-registered minor stays in control — the guardian is ReadOnly unless
        // collaboration is enabled later.
        Invitation? guardianInvite = string.IsNullOrWhiteSpace(request.GuardianEmail)
            ? null
            : Inviter.Issue(profile.Id, request.GuardianEmail!, ProfileRole.Guardian.Id, caller.Id);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        // Fire the email only after the row is durably committed (avoids inviting on a rolled-back tx).
        if (guardianInvite is not null)
            await Inviter.NotifyAsync(guardianInvite, cancellationToken);

        return MapToDto(profile);
    }
}
