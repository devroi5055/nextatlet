using FluentAssertions;
using NextAtlet.Domain.Entities.AthleteProfile;
using NextAtlet.Domain.Enumerations.AthleteProfile;

namespace NextAtlet.Domain.Tests.Entities;

/// <summary>
/// The publish gate is computed from the stored <see cref="ConsentState"/>. The publish path refuses
/// to make the profile public while <see cref="AthleteProfile.AwaitsGuardianConsent"/> is true.
/// </summary>
public class AthleteProfileConsentTests
{
    private static AthleteSite AProfile(ConsentState state) => new()
    {
        Slug = "maria",
        DisplayName = "Maria",
        DateOfBirth = new DateOnly(2015, 1, 1),
        ConsentStateId = state.Id
    };

    [Fact]
    public void NewProfile_DefaultsToConsentNotRequired()
        => new AthleteSite { Slug = "x", DisplayName = "X", DateOfBirth = new DateOnly(2015, 1, 1) }
            .ConsentStateId.Should().Be(ConsentState.NotRequired.Id);

    [Theory]
    [InlineData("pending_guardian_consent", true)]
    [InlineData("consented", false)]
    [InlineData("not_required", false)]
    public void AwaitsGuardianConsent_TrueOnlyWhilePending(string stateId, bool expected)
        => AProfile(ConsentState.FromId(stateId)).AwaitsGuardianConsent.Should().Be(expected);
}
