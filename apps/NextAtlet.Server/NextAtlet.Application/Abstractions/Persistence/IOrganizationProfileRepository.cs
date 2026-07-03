using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IOrganizationProfileRepository
{
    Task<OrganizationProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The profile this user owns (via an OrganizationOwner login), or null. One profile per owner and per site.</summary>
    Task<OrganizationProfile?> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);

    void Add(OrganizationProfile profile);
}