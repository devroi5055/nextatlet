using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of the <see cref="GuardianConsent"/> audit record — the four GDPR facts defaulted
/// and overridable (who / how / what-version / when).
/// </summary>
public static class GuardianConsents
{
    public static GuardianConsent AGuardianConsent(
        Guid? athleteProfileId = null,
        Guid? guardianUserId = null,
        ConsentMethod method = ConsentMethod.VerifiedEmail,
        string termsVersion = "2026-01",
        DateTime? consentedUtc = null)
        => new()
        {
            AthleteProfileId = athleteProfileId ?? Guid.NewGuid(),
            GuardianUserId = guardianUserId ?? Guid.NewGuid(),
            Method = method,
            TermsVersion = termsVersion,
            ConsentedUtc = consentedUtc ?? DateTime.UtcNow
        };
}
