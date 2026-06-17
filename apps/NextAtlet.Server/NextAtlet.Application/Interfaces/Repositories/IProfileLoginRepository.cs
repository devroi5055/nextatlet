using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Interfaces.Repositories;

public interface ISiteLoginRepository
{
    /// <summary>The caller's Active login on a profile (any role), or null. Feeds the PermissionResolver + the invite gate.</summary>
    Task<SiteLogin?> GetActiveLoginAsync(Guid userId, Guid SiteId, CancellationToken cancellationToken = default);

    /// <summary>Ids of sites this user actively guards (Active Guardian logins). Drives /me.</summary>
    Task<IReadOnlyList<Guid>> GetActiveGuardianSiteIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>True if the site has an Active AthleteOwner login — transfer-to-athlete can't hand control to a ghost.</summary>
    Task<bool> HasActiveOwnerLoginAsync(Guid SiteId, CancellationToken cancellationToken = default);

    /// <summary>True if the site has an Active Guardian login — transfer-to-guardian needs a guardian to receive it.</summary>
    Task<bool> HasActiveGuardianLoginAsync(Guid SiteId, CancellationToken cancellationToken = default);

    void Add(SiteLogin login);
}
