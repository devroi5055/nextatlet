using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IAthleteProfileRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<AthleteProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The profile this user owns (via an AthleteOwner login), or null. One profile per owner.</summary>
    Task<AthleteProfile?> GetOwnedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(AthleteProfile profile);
}
