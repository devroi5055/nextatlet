using MediatR;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Features.Identity;

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
    private readonly IIndividualProfileRepository _profiles;
    private readonly ISiteRepository _sites;
    private readonly ISiteLoginRepository _logins;
    private readonly IActionTokenRepository _tokens;
    private readonly PermissionResolver _permissions;

    public GetCurrentUserQueryHandler(
        IUserRepository users,
        IIndividualProfileRepository profiles,
        ISiteRepository sites,
        ISiteLoginRepository logins,
        IActionTokenRepository tokens,
        PermissionResolver permissions)
    {
        _users = users;
        _profiles = profiles;
        _sites = sites;
        _logins = logins;
        _tokens = tokens;
        _permissions = permissions;
    }

    public async Task<MeDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Pending invites are keyed by email, so they surface even before any User row exists (an
        // invited person who hasn't accepted yet — no User is created until they authenticate + accept).
        var pendingInvites = await _tokens.CountPendingInvitesByEmailAsync(request.Email, cancellationToken);

        // A User always carries its real subject, so match by subject only.
        var user = await _users.GetByAuthProviderIdAsync(request.AuthProviderId, cancellationToken);

        if (user is null)
        {
            // No presence yet — but a pending invite still means "you're being invited as a guardian".
            var role = pendingInvites > 0 ? IndividualRole.Guardian.Id : null;
            return new MeDto(Registered: false, Role: role, ProfileId: null, ControlMode: null,
                IsInControl: false, CanEdit: false, GuardedProfileIds: None, PendingGuardianInvites: pendingInvites);
        }

        var guardedSiteIds = await _logins.GetActiveGuardianSiteIdsByUserIdAsync(user.Id, cancellationToken);

        // Owning a profile is "registered"; the caller may also guard children (both states at once).
        // Control fields describe the caller's own owned profile (resolved via PermissionResolver).
        var site = await _sites.GetOwnedByUserIdAsync(user.Id, cancellationToken);
        if (site is null)
        {
            if (guardedSiteIds.Count > 0 || pendingInvites > 0)
                return new MeDto(Registered: false, Role: IndividualRole.Guardian.Id, ProfileId: null, ControlMode: null,
                    IsInControl: false, CanEdit: false, GuardedProfileIds: guardedSiteIds, PendingGuardianInvites: pendingInvites);

            return new MeDto(Registered: false, Role: null, ProfileId: null, ControlMode: null,
                IsInControl: false, CanEdit: false, GuardedProfileIds: None, PendingGuardianInvites: pendingInvites);
        }

        var profile = await _profiles.GetBySiteIdAsync(site.Id, cancellationToken);
        if (profile is null)
            throw new DomainException(ErrorCodes.ProfileNotFound);
            
        var siteLogin = await _logins.GetActiveLoginAsync(user.Id, site.Id, cancellationToken);
        var isInControl = siteLogin is not null && _permissions.IsController(siteLogin, profile);
        var canEdit = siteLogin is not null && _permissions.Resolve(siteLogin, profile).CanEditContent;

        return new MeDto(Registered: true, Role: IndividualRole.Owner.Id, ProfileId: profile.Id,
            ControlMode: ControlModes.FromId(profile.ControlModeId), IsInControl: isInControl, CanEdit: canEdit,
            GuardedProfileIds: guardedSiteIds, PendingGuardianInvites: pendingInvites);
            
    }
}
