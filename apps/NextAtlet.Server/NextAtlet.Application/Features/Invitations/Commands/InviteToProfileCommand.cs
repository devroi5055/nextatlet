using MediatR;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Enumerations.AthleteProfile;

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
    string Role) : IRequest<Result<InvitationDto>>;

public class InviteToProfileCommandHandler : IRequestHandler<InviteToProfileCommand, Result<InvitationDto>>
{
    private readonly IUserRepository _users;
    private readonly IAthleteSiteRepository _sites;
    private readonly IProfileLoginRepository _logins;
    private readonly IInvitationRepository _invitations;
    private readonly InvitationIssuer _inviter;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public InviteToProfileCommandHandler(
        IUserRepository users,
        IAthleteSiteRepository sites,
        IProfileLoginRepository logins,
        IInvitationRepository invitations,
        InvitationIssuer inviter,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _users = users;
        _sites = sites;
        _logins = logins;
        _invitations = invitations;
        _inviter = inviter;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<InvitationDto>> Handle(InviteToProfileCommand request, CancellationToken cancellationToken)
    {
        // Role must be a known ProfileRole — reject early rather than create an unusable invitation.
        if (request.Role != ProfileRole.AthleteOwner.Id && request.Role != ProfileRole.Guardian.Id)
            return Error.FromCode(ErrorCodes.InvitationRoleInvalid);

        // The caller must already be a known user; an unknown subject holds no rights anywhere.
        var caller = await _users.GetByAuthProviderIdAsync(request.CallerAuthProviderId, cancellationToken);
        if (caller is null)
            return Error.FromCode(ErrorCodes.NotAuthorized);

        var profile = await _sites.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
            return Error.FromCode(ErrorCodes.SiteNotFound);

        // Authorization: only someone with an Active login on this profile may invite to it.
        if (await _logins.GetActiveLoginAsync(caller.Id, profile.Id, cancellationToken) is null)
            return Error.FromCode(ErrorCodes.NotAuthorized);

        // A guardian only makes sense for a minor — refuse to invite one onto an adult profile.
        if (request.Role == ProfileRole.Guardian.Id && !profile.IsMinor(_clock.UtcNow))
            return Error.FromCode(ErrorCodes.GuardianCannotRegisterAdult);

        // Don't double-invite the same email+role on the same profile.
        if (await _invitations.HasPendingAsync(profile.Id, request.Email, request.Role, cancellationToken))
            return Error.FromCode(ErrorCodes.InvitationAlreadyPending);

        var invitation = _inviter.Issue(profile.Id, request.Email, request.Role, caller.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _inviter.NotifyAsync(invitation, cancellationToken);

        return new InvitationDto(invitation.Id, invitation.TargetProfileId, invitation.Email, invitation.RoleId, invitation.ExpiresUtc);
    }
}
