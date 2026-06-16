using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IGuardianConsentRepository
{
    void Add(GuardianConsent consent);

    /// <summary>True if any consent has been recorded for the profile (the audit trail exists).</summary>
    Task<bool> ExistsForProfileAsync(Guid athleteProfileId, CancellationToken cancellationToken = default);
}
