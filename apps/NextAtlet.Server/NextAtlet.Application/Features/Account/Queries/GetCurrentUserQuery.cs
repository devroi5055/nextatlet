using MediatR;
using NextAtlet.Application.Abstractions.Identity;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Enumerations;

namespace NextAtlet.Application.Features.Account.Queries;

/// <summary>
/// The domain-gate check: tells the frontend which side of registration the authenticated caller is
/// on (and their role), so it can route to the registration form vs. the dashboard. Identity comes
/// from the validated token, never the request.
/// </summary>
public record GetCurrentUserQuery : IRequest<MeDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, MeDto>
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IUserRepository _users;
    private readonly IAthleteProfileRepository _profiles;
    private readonly IProfileLoginRepository _logins;

    public GetCurrentUserQueryHandler(
        ICurrentUserContext currentUser,
        IUserRepository users,
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins)
    {
        _currentUser = currentUser;
        _users = users;
        _profiles = profiles;
        _logins = logins;
    }

    public async Task<MeDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByAuthProviderIdAsync(_currentUser.AuthProviderId, cancellationToken);
        if (user is null)
            return new MeDto(Registered: false, Role: null);

        // A user may own a profile, be a guardian, or both. Owning a profile is "registered".
        var ownedProfile = await _profiles.GetOwnedByUserIdAsync(user.Id, cancellationToken);
        if (ownedProfile is not null)
            return new MeDto(Registered: true, Role: ProfileRole.AthleteOwner.Id);

        var isGuardian = await _logins.HasGuardianLoginAsync(user.Id, cancellationToken);
        return new MeDto(Registered: false, Role: isGuardian ? ProfileRole.Guardian.Id : null);
    }
}
