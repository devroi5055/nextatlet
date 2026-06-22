using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IIndividualProfileRepository
{
    Task<IndividualProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The profile this user owns (via an AthleteOwner login), or null. One profile per owner.</summary>
    Task<IndividualProfile?> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);

    void Add(IndividualProfile profile);
}