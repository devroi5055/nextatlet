using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Domain.Entities.Consent;

/// <summary>
/// The GDPR Art. 8 audit record — proof a guardian consented to a minor's data processing. Captures
/// exactly the four required facts: WHO (the authenticated guardian identity, not a typed-in name),
/// HOW (verification method), WHAT (privacy-notice version), WHEN. Written only when a consent-request
/// invitation is accepted; never mutated afterwards.
/// </summary>
public class GuardianConsent : CreatedOnlyEntity
{
    /// <summary>FK → the minor's IndividualProfile this consent covers.</summary>
    public required Guid IndividualProfileId { get; set; }

    /// <summary>WHO — the authenticated guardian's User identity (stronger evidence than a name string).</summary>
    public required Guid GuardianUserId { get; set; }

    /// <summary>HOW — the verification method (<see cref="ConsentMethods"/>), stored as its enum name.</summary>
    public required string MethodId { get; set; }

    /// <summary>WHAT — the privacy-notice version consented to.</summary>
    public required string TermsVersion { get; set; }

    // Navigation
    public IndividualProfile AthleteSite { get; set; } = default!;
    public User Guardian { get; set; } = default!;
}
