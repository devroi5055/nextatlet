using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface ISiteSnapshotRepository
{
    /// <summary>Returns the tracked draft snapshot for a profile via AthleteProfile.CurrentDraftSnapshotId, or null if none exists.</summary>
    Task<SiteSnapshot?> GetCurrentDraftBySiteIdAsync(Guid SiteId, CancellationToken cancellationToken = default);
    void Add(SiteSnapshot snapshot);
}
