using NextAtlet.Domain.Entities.Consent;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of the <see cref="GuardianConsent"/> audit record — the four GDPR facts defaulted
/// and overridable (who / how / what-version / when).
/// </summary>
public static class GuardianConsents
{
    public static GuardianConsent AGuardianConsent(
        Guid? siteId = null,
        Guid? guardianUserId = null,
        string? method = null,
        string termsVersion = "2026-01")
        => new()
        {
            SiteId = siteId ?? Guid.NewGuid(),
            GuardianUserId = guardianUserId ?? Guid.NewGuid(),
            MethodId = method ?? ConsentMethods.Email.Id,
            TermsVersion = termsVersion,
        };
}
