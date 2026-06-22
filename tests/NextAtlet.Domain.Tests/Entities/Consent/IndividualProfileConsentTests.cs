using FluentAssertions;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Domain.Tests.Entities.Consent;

/// <summary>
/// The publish gate is computed from the stored <see cref="ConsentStates"/>. The publish path refuses
/// to make the profile public while <see cref="IndividualProfile.AwaitsGuardianConsent"/> is true.
/// </summary>
public class IndividualProfileConsentTests
{
    private static IndividualProfile AProfile(ConsentStates state) => new()
    {
        SiteId = Guid.NewGuid(),
        DateOfBirth = new DateOnly(2015, 1, 1),
        ConsentStateId = state.Id
    };

    [Theory]
    [InlineData("pending_guardian_consent", true)]
    [InlineData("consented", false)]
    [InlineData("not_required", false)]
    public void AwaitsGuardianConsent_TrueOnlyWhilePending(string stateId, bool expected)
        => AProfile(ConsentStates.FromId(stateId)).AwaitsGuardianConsent.Should().Be(expected);
}
