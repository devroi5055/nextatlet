using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface ISiteConfigRepository
{
    /// <summary>Returns the tracked draft config for a profile, or null if none exists.</summary>
    Task<SiteConfig?> GetDraftByProfileIdAsync(Guid athleteProfileId, CancellationToken cancellationToken = default);
    void Add(SiteConfig config);
}
