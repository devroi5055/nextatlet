using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Domain.Entities.Athlete;

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
        // v1: this flow is for minors. An adult must self-register.
        if (!IsMinor(request.ChildDateOfBirth))
            throw new DomainException(ErrorCodes.GuardianCannotRegisterAdult);

        var guardian = await GetOrCreateUserAsync(request.Email, request.AuthProviderId, cancellationToken);

        var profile = await CreateAthleteProfileCoreAsync(
            request.Slug, request.ChildDisplayName, request.ChildDateOfBirth, request.DefaultLocaleId, cancellationToken);

        // Caller becomes the Guardian, ACTIVE by construction (consent given by creating the profile);
        // the child's AthleteOwner login is deferred.
        Logins.Add(ProfileLogin.CreateGuardian(guardian.Id, profile, active: true));

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(profile);
    }
}
