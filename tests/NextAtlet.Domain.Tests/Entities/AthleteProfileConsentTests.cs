using FluentAssertions;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using Xunit;

namespace NextAtlet.Domain.Tests.Entities;

/// <summary>
/// The publish gate is computed from the stored <see cref="ConsentState"/>. The publish path refuses
/// to make the profile public while <see cref="AthleteProfile.AwaitsGuardianConsent"/> is true.
/// </summary>
public class AthleteProfileConsentTests
{
    private static AthleteProfile AProfile(ConsentState state) => new()
    {
        Slug = "maria",
        DisplayName = "Maria",
        DateOfBirth = new DateOnly(2015, 1, 1),
        ConsentState = state
    };

    [Fact]
    public void NewProfile_DefaultsToConsentNotRequired()
        => new AthleteProfile { Slug = "x", DisplayName = "X", DateOfBirth = new DateOnly(2015, 1, 1) }
            .ConsentState.Should().Be(ConsentState.NotRequired);

    [Theory]
    [InlineData(ConsentState.PendingGuardianConsent, true)]
    [InlineData(ConsentState.Consented, false)]
    [InlineData(ConsentState.NotRequired, false)]
    public void AwaitsGuardianConsent_TrueOnlyWhilePending(ConsentState state, bool expected)
        => AProfile(state).AwaitsGuardianConsent.Should().Be(expected);
}
