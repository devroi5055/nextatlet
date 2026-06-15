using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IAthleteSiteSnapshotRepository
{
    /// <summary>Returns the tracked draft snapshot for a profile via AthleteProfile.CurrentDraftSnapshotId, or null if none exists.</summary>
    Task<AthleteSiteSnapshot?> GetDraftByProfileIdAsync(Guid athleteProfileId, CancellationToken cancellationToken = default);
    void Add(AthleteSiteSnapshot snapshot);
}
