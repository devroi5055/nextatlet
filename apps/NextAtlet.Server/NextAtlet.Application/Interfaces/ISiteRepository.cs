using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface ISiteRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The profile this user owns (via an AthleteOwner login), or null. One profile per owner.</summary>
    Task<Site?> GetOwnedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(Site site);
}
