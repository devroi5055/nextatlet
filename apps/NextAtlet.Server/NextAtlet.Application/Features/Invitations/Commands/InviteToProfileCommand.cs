using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Time;
using NextAtlet.Domain.Enumerations;

namespace NextAtlet.Application.Features.Invitations.Commands;

/// <summary>
/// Invite a person (by email) to an existing profile in a given role. Authorization is natural: only
/// someone holding an Active login on the profile may invite to it. The credential is materialized at
/// accept time, not here. Caller identity comes from the validated token (controller), never the body.
/// </summary>
public record InviteToProfileCommand(
    Guid ProfileId,
    string CallerAuthProviderId,
    string CallerEmail,
    string Email,
    string Role) : IRequest<InvitationDto>;

public class InviteToProfileCommandHandler : IRequestHandler<InviteToProfileCommand, InvitationDto>
{
    private readonly IUserRepository _users;
    private readonly IAthleteProfileRepository _profiles;
    private readonly IProfileLoginRepository _logins;
    private readonly IInvitationRepository _invitations;
    private readonly InvitationIssuer _inviter;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public InviteToProfileCommandHandler(
        IUserRepository users,
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IInvitationRepository invitations,
        InvitationIssuer inviter,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _users = users;
        _profiles = profiles;
        _logins = logins;
        _invitations = invitations;
        _inviter = inviter;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<InvitationDto> Handle(InviteToProfileCommand request, CancellationToken cancellationToken)
    {
        // Role must be a known ProfileRole — reject early rather than create an unusable invitation.
        if (request.Role != ProfileRole.AthleteOwner.Id && request.Role != ProfileRole.Guardian.Id)
            throw new DomainException(ErrorCodes.InvitationRoleInvalid, request.Role);

        // The caller must already be a known user; an unknown subject holds no rights anywhere.
        var caller = await _users.GetByAuthProviderIdAsync(request.CallerAuthProviderId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.NotAuthorized);

        var profile = await _profiles.GetByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.ProfileNotFound);

        // Authorization: only someone with an Active login on this profile may invite to it.
        if (await _logins.GetActiveLoginAsync(caller.Id, profile.Id, cancellationToken) is null)
            throw new DomainException(ErrorCodes.NotAuthorized);

        // A guardian only makes sense for a minor — refuse to invite one onto an adult profile.
        if (request.Role == ProfileRole.Guardian.Id && !profile.IsMinor(_clock.UtcNow))
            throw new DomainException(ErrorCodes.GuardianCannotRegisterAdult);

        // Don't double-invite the same email+role on the same profile.
        if (await _invitations.HasPendingAsync(profile.Id, request.Email, request.Role, cancellationToken))
            throw new DomainException(ErrorCodes.InvitationAlreadyPending);

        var invitation = _inviter.Issue(profile.Id, request.Email, request.Role, caller.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _inviter.NotifyAsync(invitation, cancellationToken);

        return new InvitationDto(invitation.Id, invitation.TargetProfileId, invitation.Email, invitation.RoleId, invitation.ExpiresUtc);
    }
}
