using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Enumerations;

namespace NextAtlet.Application.Features.Account.Queries;

/// <summary>
/// The domain-gate check: tells the frontend which side of registration the authenticated caller is
/// on (role + any pending guardian invites), so it can route to the registration form, the dashboard,
/// or an "accept guardianship" prompt. Identity comes from the validated token (controller), never the body.
/// </summary>
public record GetCurrentUserQuery(string AuthProviderId, string Email) : IRequest<MeDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, MeDto>
{
    private readonly IUserRepository _users;
    private readonly IAthleteProfileRepository _profiles;
    private readonly IProfileLoginRepository _logins;

    public GetCurrentUserQueryHandler(
        IUserRepository users,
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins)
    {
        _users = users;
        _profiles = profiles;
        _logins = logins;
    }

    public async Task<MeDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Match by subject (claimed) or by the invited email (a guardian who hasn't claimed yet).
        var user = await _users.GetByAuthProviderIdAsync(request.AuthProviderId, cancellationToken)
            ?? await _users.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            return new MeDto(Registered: false, Role: null, PendingGuardianInvites: 0);

        var pendingInvites = (await _logins.GetPendingGuardianLoginsByUserIdAsync(user.Id, cancellationToken)).Count;

        // A user may own a profile, be a guardian, or both. Owning a profile is "registered".
        var ownedProfile = await _profiles.GetOwnedByUserIdAsync(user.Id, cancellationToken);
        if (ownedProfile is not null)
            return new MeDto(Registered: true, Role: ProfileRole.AthleteOwner.Id, PendingGuardianInvites: pendingInvites);

        var isGuardian = await _logins.HasGuardianLoginAsync(user.Id, cancellationToken);
        return new MeDto(Registered: false, Role: isGuardian ? ProfileRole.Guardian.Id : null, PendingGuardianInvites: pendingInvites);
    }
}
