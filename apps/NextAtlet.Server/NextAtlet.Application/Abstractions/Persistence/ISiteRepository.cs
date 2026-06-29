using NextAtlet.Application.Common.Models;
using NextAtlet.Application.Contracts.Sites.Request;
using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface ISiteRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The profile this user owns (via an AthleteOwner login), or null. One profile per owner.</summary>
    Task<Site?> GetOwnedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>A filtered/sorted page of sites for the listing endpoint.</summary>
    Task<PagedResult<Site>> GetPagedAsync(SiteListRequest filter, CancellationToken cancellationToken = default);

    void Add(Site site);
}
