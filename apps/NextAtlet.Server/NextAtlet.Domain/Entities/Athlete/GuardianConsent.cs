using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Domain.Entities.Athlete;

/// <summary>
/// The GDPR Art. 8 audit record — proof a guardian consented to a minor's data processing. Captures
/// exactly the four required facts: WHO (the authenticated guardian identity, not a typed-in name),
/// HOW (verification method), WHAT (privacy-notice version), WHEN. Written only when a consent-request
/// invitation is accepted; never mutated afterwards.
/// </summary>
public class GuardianConsent : CreatedOnlyEntity
{
    /// <summary>FK → the minor's AthleteProfile this consent covers.</summary>
    public required Guid AthleteProfileId { get; set; }

    /// <summary>WHO — the authenticated guardian's User identity (stronger evidence than a name string).</summary>
    public required Guid GuardianUserId { get; set; }

    /// <summary>HOW — the verification method (<see cref="ConsentMethod"/>), stored as its enum name.</summary>
    public required ConsentMethod Method { get; set; }

    /// <summary>WHAT — the privacy-notice version consented to.</summary>
    public required string TermsVersion { get; set; }

    // Navigation
    public AthleteSite AthleteSite { get; set; } = default!;
    public User Guardian { get; set; } = default!;
}
