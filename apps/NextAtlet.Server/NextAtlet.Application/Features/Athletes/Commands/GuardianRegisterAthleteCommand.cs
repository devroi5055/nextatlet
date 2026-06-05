using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.Policies;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Guardian-creates-profile-for-child: the authenticated caller (parent/guardian) registers an
/// AthleteProfile for their child and becomes its Guardian login. No AthleteOwner login exists in v1
/// (the child has no login yet — deferred). The guardian is attached in the same transaction, so the
/// "minor profile always has a guardian" rule holds by construction.
/// A guardian may register multiple children (no single-profile idempotency guard here).
/// </summary>
public record GuardianRegisterAthleteCommand(
    string AuthProviderId,
    string Email,
    string ChildDisplayName,
    string Slug,
    DateTime ChildDateOfBirth,
    string DefaultLocaleId) : IRequest<AthleteProfileDto>;

public class GuardianRegisterAthleteCommandHandler
    : AthleteRegistrationHandlerBase, IRequestHandler<GuardianRegisterAthleteCommand, AthleteProfileDto>
{
    public GuardianRegisterAthleteCommandHandler(
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IThemeRepository themes,
        ISiteConfigRepository siteConfigs,
        UserProvisioner userProvisioner,
        InvitationIssuer inviter,
        IUnitOfWork unitOfWork)
        : base(profiles, logins, themes, siteConfigs, userProvisioner, inviter, unitOfWork) { }

    public async Task<AthleteProfileDto> Handle(GuardianRegisterAthleteCommand request, CancellationToken cancellationToken)
    {
        // v1: this flow is for minors. An adult must self-register. Under-13 IS allowed here — that is
        // the intended path for very young children (the age floor only applies to self-register).
        if (AgePolicy.BandToday(request.ChildDateOfBirth) == AgeBand.Adult)
            throw new DomainException(ErrorCodes.GuardianCannotRegisterAdult);

        var guardian = await GetOrCreateUserAsync(request.Email, request.AuthProviderId, cancellationToken);

        // Guardian-register always starts GuardianControlled — the guardian created the profile.
        var profile = await CreateAthleteProfileCoreAsync(
            request.Slug, request.ChildDisplayName, request.ChildDateOfBirth, request.DefaultLocaleId,
            ControlMode.GuardianControlled, cancellationToken);

        // Caller becomes the Guardian (Active by construction). The child's AthleteOwner login is deferred.
        Logins.Add(ProfileLogin.CreateGuardian(guardian.Id, profile.Id));

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(profile);
    }
}
