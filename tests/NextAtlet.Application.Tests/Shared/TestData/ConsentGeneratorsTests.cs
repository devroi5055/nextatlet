using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>Sanity checks for the guardian-consent test-data generators.</summary>
public class ConsentGeneratorsTests
{
    [Fact]
    public void APendingGuardianConsentAthlete_AwaitsConsent()
    {
        var athlete = TestAthletes.APendingGuardianConsentAthlete();

        Assert.Equal(ConsentState.PendingGuardianConsent, athlete.ConsentState);
        Assert.True(athlete.AwaitsGuardianConsent);
    }

    [Fact]
    public void AGuardianConsent_HasAllFourGdprFacts()
    {
        var consent = GuardianConsents.AGuardianConsent();

        Assert.NotEqual(Guid.Empty, consent.GuardianUserId);        // who
        Assert.Equal(ConsentMethod.VerifiedEmail, consent.Method);  // how
        Assert.False(string.IsNullOrWhiteSpace(consent.TermsVersion)); // what
        Assert.NotEqual(default, consent.ConsentedUtc);             // when
        Assert.NotEqual(Guid.Empty, consent.AthleteProfileId);
    }
}
