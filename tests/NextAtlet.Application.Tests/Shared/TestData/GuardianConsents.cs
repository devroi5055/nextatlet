using NextAtlet.Domain.Entities.Sites;

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
        string method = "verified_email",
        string termsVersion = "2026-01",
        DateTime? consentedUtc = null)
        => new()
        {
            AthleteProfileId = athleteProfileId ?? Guid.NewGuid(),
            GuardianUserId = guardianUserId ?? Guid.NewGuid(),
            MethodId = method,
            TermsVersion = termsVersion,
        };
}
