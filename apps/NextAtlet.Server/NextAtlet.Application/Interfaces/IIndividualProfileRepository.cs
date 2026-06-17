using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IAthleteProfileRepository
{
    Task<AthleteProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The profile this user owns (via an AthleteOwner login), or null. One profile per owner.</summary>
    Task<AthleteProfile?> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);

    void Add(AthleteProfile profile);
}