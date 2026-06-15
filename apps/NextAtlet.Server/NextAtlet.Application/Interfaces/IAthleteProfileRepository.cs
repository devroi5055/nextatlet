using NextAtlet.Domain.Entities.AthleteProfile;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IAthleteSiteRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<AthleteSite?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The profile this user owns (via an AthleteOwner login), or null. One profile per owner.</summary>
    Task<AthleteSite?> GetOwnedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(AthleteSite site);
}
