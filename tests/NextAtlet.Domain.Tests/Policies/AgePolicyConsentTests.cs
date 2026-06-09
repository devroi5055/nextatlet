using FluentAssertions;
using NextAtlet.Domain.Policies;
using Xunit;

namespace NextAtlet.Domain.Tests.Policies;

/// <summary>
/// <see cref="AgePolicy.RequiresGuardianConsent"/> — guardian consent (GDPR Art. 8) is required below
/// the configurable self-consent age. Threshold is a parameter, so configurability is provable here.
/// </summary>
public class AgePolicyConsentTests
{
    private static readonly DateTime Now = new(2026, 6, 9);
    private static DateOnly Today => DateOnly.FromDateTime(Now);

    [Theory] // Denmark default: self-consent age 13
    [InlineData(12, true)]
    [InlineData(13, false)]
    [InlineData(16, false)]
    [InlineData(18, false)]
    public void AtSelfConsentAge13_RequiresConsentOnlyUnder13(int age, bool expected)
    {
        var dob = Today.AddYears(-age);

        AgePolicy.RequiresGuardianConsent(dob, Now, selfConsentAge: 13).Should().Be(expected);
    }

    [Theory] // If the EU mandates 16, the 13–15 band becomes consent-required — config only, no code change
    [InlineData(12, true)]
    [InlineData(13, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    [InlineData(17, false)]
    public void AtSelfConsentAge16_RequiresConsentUnder16(int age, bool expected)
    {
        var dob = Today.AddYears(-age);

        AgePolicy.RequiresGuardianConsent(dob, Now, selfConsentAge: 16).Should().Be(expected);
    }

    [Fact]
    public void OnExactSelfConsentBirthday_DoesNotRequireConsent()
    {
        // Turning 13 exactly today → self-consents (not < 13).
        AgePolicy.RequiresGuardianConsent(Today.AddYears(-13), Now, 13).Should().BeFalse();
        // Birthday tomorrow → still 12 today → consent required.
        AgePolicy.RequiresGuardianConsent(Today.AddYears(-13).AddDays(1), Now, 13).Should().BeTrue();
    }

    [Fact]
    public void LeapDayBirthday_IsHandledInNonLeapYear()
    {
        // Born 2012-02-29; at 2026-06-09 they are 14 → consent required under a self-consent age of 16.
        AgePolicy.RequiresGuardianConsent(new DateOnly(2012, 2, 29), Now, 16).Should().BeTrue();
        AgePolicy.RequiresGuardianConsent(new DateOnly(2012, 2, 29), Now, 13).Should().BeFalse();
    }
}
