using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Self-registration: the authenticated caller registers an AthleteProfile for themselves and
/// becomes its AthleteOwner. If the caller is a minor, a guardian must be invited (created in the
/// same transaction) — the core "minor profile always has a guardian" rule.
/// Identity (sub/email) comes from the authenticated principal via the controller, never the body.
/// </summary>
public record SelfRegisterAthleteCommand(
    string AuthProviderId,
    string Email,
    string DisplayName,
    string Slug,
    DateTime DateOfBirth,
    string DefaultLocaleId,
    string? GuardianEmail = null) : IRequest<AthleteProfileDto>;

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
        var caller = await GetOrCreateUserAsync(request.Email, request.AuthProviderId, cancellationToken);

        // A user can't self-register two owned profiles.
        if (await Profiles.GetOwnedByUserIdAsync(caller.Id, cancellationToken) is not null)
            throw new DomainException(ErrorCodes.ProfileAlreadyExists);

        var isMinor = IsMinor(request.DateOfBirth);
        if (isMinor && string.IsNullOrWhiteSpace(request.GuardianEmail))
            throw new DomainException(ErrorCodes.GuardianEmailRequired);

        var profile = await CreateAthleteProfileCoreAsync(request.Slug, request.DisplayName, request.DateOfBirth, request.DefaultLocaleId, cancellationToken);

        Logins.Add(ProfileLogin.CreateOwner(caller.Id, profile.Id));

        // Minor → issue a guardian Invitation in the SAME transaction. The guardian's ProfileLogin is
        // materialized when they accept; until then the Invitation is the pending state.
        Invitation? guardianInvite = isMinor
            ? Inviter.Issue(profile.Id, request.GuardianEmail!, ProfileRole.Guardian.Id, caller.Id)
            : null;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        // Fire the email only after the row is durably committed (avoids inviting on a rolled-back tx).
        if (guardianInvite is not null)
            await Inviter.NotifyAsync(guardianInvite, cancellationToken);

        return MapToDto(profile);
    }
}
