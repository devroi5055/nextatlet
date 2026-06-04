using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IAthleteProfileRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<AthleteProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(AthleteProfile profile);
}
