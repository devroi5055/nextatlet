using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Shared;

namespace NextAtlet.Application.Features.Account;

/// <summary>
/// Resolves the authenticated caller's domain <see cref="User"/>, provisioning just-in-time from
/// verified token claims. Shared by registration and invitation-accept so the rule lives in one place.
///
/// We never create a User before the person authenticates — an invite stores only an
/// <see cref="NextAtlet.Domain.Entities.Athlete.Invitation"/>, which is the single source of pending
/// state. So a User row always carries a real subject (no null <c>AuthProviderId</c> in the DB, no
/// backfill, no claim-by-email): match by subject, else create from claims.
/// </summary>
public sealed class UserProvisioner
{
    private readonly IUserRepository _users;

    public UserProvisioner(IUserRepository users) => _users = users;

    public async Task<User> GetOrCreateAsync(string email, string authProviderId, CancellationToken cancellationToken)
    {
        var existing = await _users.GetByAuthProviderIdAsync(authProviderId, cancellationToken);
        if (existing is not null)
            return existing;

        var user = new User { Email = email, AuthProviderId = authProviderId };
        _users.Add(user);
        return user;
    }
}
