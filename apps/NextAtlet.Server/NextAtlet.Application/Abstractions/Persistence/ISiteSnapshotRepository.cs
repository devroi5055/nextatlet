using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Interfaces.Repositories;

public interface ISiteSnapshotRepository
{
    /// <summary>Returns the tracked draft snapshot for a profile via IndividualProfile.CurrentDraftSnapshotId, or null if none exists.</summary>
    Task<SiteSnapshot?> GetCurrentDraftBySiteIdAsync(Guid SiteId, CancellationToken cancellationToken = default);
    void Add(SiteSnapshot snapshot);
}
