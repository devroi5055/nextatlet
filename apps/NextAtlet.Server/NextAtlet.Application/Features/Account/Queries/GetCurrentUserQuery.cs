using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Enumerations;

namespace NextAtlet.Application.Features.Account.Queries;

/// <summary>
/// The domain-gate check: tells the frontend which side of registration the authenticated caller is
/// on (owned profile, guarded profiles, pending invites), so it can route to the registration form,
/// the dashboard, or an "accept invitation" prompt. Identity comes from the validated token, never the body.
/// </summary>
public record GetCurrentUserQuery(string AuthProviderId, string Email) : IRequest<MeDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, MeDto>
{
    private static readonly IReadOnlyList<Guid> None = [];

    private readonly IUserRepository _users;
    private readonly IAthleteProfileRepository _profiles;
    private readonly IProfileLoginRepository _logins;
    private readonly IInvitationRepository _invitations;
    private readonly PermissionResolver _permissions;

    public GetCurrentUserQueryHandler(
        IUserRepository users,
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IInvitationRepository invitations,
        PermissionResolver permissions)
    {
        _users = users;
        _profiles = profiles;
        _logins = logins;
        _invitations = invitations;
        _permissions = permissions;
    }

    public async Task<MeDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Pending invites are keyed by email, so they surface even before any User row exists (an
        // invited person who hasn't accepted yet — no User is created until they authenticate + accept).
        var pendingInvites = await _invitations.CountPendingByEmailAsync(request.Email, cancellationToken);

        // A User always carries its real subject, so match by subject only.
        var user = await _users.GetByAuthProviderIdAsync(request.AuthProviderId, cancellationToken);

        if (user is null)
        {
            // No presence yet — but a pending invite still means "you're being invited as a guardian".
            var role = pendingInvites > 0 ? ProfileRole.Guardian.Id : null;
            return new MeDto(Registered: false, Role: role, ProfileId: null, ControlMode: null,
                IsInControl: false, CanEdit: false, GuardedProfileIds: None, PendingGuardianInvites: pendingInvites);
        }

        var ownedProfile = await _profiles.GetOwnedByUserIdAsync(user.Id, cancellationToken);
        var guardedProfileIds = await _logins.GetActiveGuardianProfileIdsByUserIdAsync(user.Id, cancellationToken);

        // Owning a profile is "registered"; the caller may also guard children (both states at once).
        // Control fields describe the caller's own owned profile (resolved via PermissionResolver).
        if (ownedProfile is not null)
        {
            var ownerLogin = await _logins.GetActiveLoginAsync(user.Id, ownedProfile.Id, cancellationToken);
            var isInControl = ownerLogin is not null && _permissions.IsController(ownerLogin, ownedProfile);
            var canEdit = ownerLogin is not null && _permissions.Resolve(ownerLogin, ownedProfile).CanEditContent;

            return new MeDto(Registered: true, Role: ProfileRole.AthleteOwner.Id, ProfileId: ownedProfile.Id,
                ControlMode: ownedProfile.ControlMode, IsInControl: isInControl, CanEdit: canEdit,
                GuardedProfileIds: guardedProfileIds, PendingGuardianInvites: pendingInvites);
        }

        if (guardedProfileIds.Count > 0 || pendingInvites > 0)
            return new MeDto(Registered: false, Role: ProfileRole.Guardian.Id, ProfileId: null, ControlMode: null,
                IsInControl: false, CanEdit: false, GuardedProfileIds: guardedProfileIds, PendingGuardianInvites: pendingInvites);

        return new MeDto(Registered: false, Role: null, ProfileId: null, ControlMode: null,
            IsInControl: false, CanEdit: false, GuardedProfileIds: None, PendingGuardianInvites: pendingInvites);
    }
}
