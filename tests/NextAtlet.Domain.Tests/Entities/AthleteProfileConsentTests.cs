using FluentAssertions;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;

namespace NextAtlet.Domain.Tests.Entities;

/// <summary>
/// The publish gate is computed from the stored <see cref="ConsentStates"/>. The publish path refuses
/// to make the profile public while <see cref="AthleteProfile.AwaitsGuardianConsent"/> is true.
/// </summary>
public class AthleteProfileConsentTests
{
    private static AthleteProfile AProfile(ConsentStates state) => new()
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
