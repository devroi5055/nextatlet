using NextAtlet.Domain.Enumerations.AthleteProfile;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>Sanity checks for the guardian-consent test-data generators.</summary>
public class ConsentGeneratorsTests
{
    [Fact]
    public void APendingGuardianConsentAthlete_AwaitsConsent()
    {
        var athlete = TestAthletes.APendingGuardianConsentAthlete();

        Assert.Equal(ConsentStates.PendingGuardianConsent.Id, athlete.ConsentStateId);
        Assert.True(athlete.AwaitsGuardianConsent);
    }

    [Fact]
    public void AGuardianConsent_HasAllFourGdprFacts()
    {
        var consent = GuardianConsents.AGuardianConsent();

        Assert.NotEqual(Guid.Empty, consent.GuardianUserId);        // who
        Assert.Equal(ConsentMethods.VerifiedEmail.Id, consent.MethodId);  // how
        Assert.False(string.IsNullOrWhiteSpace(consent.TermsVersion)); // what
        // "when" (CreatedUtc) is stamped by the DbContext at SaveChanges, not at construction.
        Assert.NotEqual(Guid.Empty, consent.AthleteProfileId);
    }
}
