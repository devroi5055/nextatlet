using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Shared;
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
    string DefaultLocaleId) : IRequest<Result<AthleteSiteDto>>;

public class GuardianRegisterAthleteCommandHandler
    : AthleteRegistrationHandlerBase, IRequestHandler<GuardianRegisterAthleteCommand, Result<AthleteSiteDto>>
{
    public GuardianRegisterAthleteCommandHandler(
        IAthleteSiteRepository sites,
        IProfileLoginRepository logins,
        IThemeRepository themes,
        IAthleteSiteSnapshotRepository siteSnapshots,
        UserProvisioner userProvisioner,
        InvitationIssuer inviter,
        IClock clock,
        IUnitOfWork unitOfWork)
        : base(sites, logins, themes, siteSnapshots, userProvisioner, inviter, clock, unitOfWork) { }

    public async Task<Result<AthleteSiteDto>> Handle(GuardianRegisterAthleteCommand request, CancellationToken cancellationToken)
    {
        // v1: this flow is for minors. An adult must self-register. Under-13 IS allowed here — that is
        // the intended path for very young children (the age floor only applies to self-register).
        if (AgePolicy.BandToday(request.ChildDateOfBirth, Clock.UtcNow) == AgeBand.Adult)
            return Error.FromCode(ErrorCodes.GuardianCannotRegisterAdult);

        var guardian = await GetOrCreateUserAsync(request.Email, request.AuthProviderId, cancellationToken);

        // Guardian-register always starts GuardianControlled — the guardian created the profile.
        var created = await CreateAthleteProfileCoreAsync(
            request.Slug, request.ChildDisplayName, request.ChildDateOfBirth, request.DefaultLocaleId,
            ControlMode.GuardianControlled, cancellationToken);
        if (!created.IsSuccess)
            return created.Error!;
        var profile = created.Value!;

        // Caller becomes the Guardian (Active by construction). The child's AthleteOwner login is deferred.
        Logins.Add(ProfileLogin.CreateGuardian(guardian.Id, profile.Id));

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(profile);
    }
}
