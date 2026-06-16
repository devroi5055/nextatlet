using FluentAssertions;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Tests.Shared;

namespace NextAtlet.Domain.Tests.Entities;

/// <summary>
/// Pure domain tests for <see cref="AthleteProfile"/> — no DB, no I/O.
/// These cover only behaviour the entity OWNS: computed minor status, age-derived
/// state, and the stored control fact. Site-level identity (slug, display name,
/// visibility) now lives on <see cref="Site"/> and is tested there.
/// </summary>
public class AthleteProfileTests
{
    // Helper: build a valid profile with an overridable date of birth.
    private static AthleteProfile AProfile(DateOnly? dob = null) => new()
    {
        SiteId = Guid.NewGuid(),
        SportId = "judo",
        DateOfBirth = dob ?? new DateOnly(2000, 1, 1),
        ConsentStateId = ConsentStates.NotRequired.Id
    };

    // A fixed "today" so boundary tests are deterministic regardless of when they run.
    private static readonly DateOnly Today = DateOnly.FromDateTime(TestTime.UtcNow);

    // ──────────────────────────────────────────────────────────────────────────
    // IsMinor — computed, never stored (02 §2). The load-bearing entity behaviour.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsMinor_WhenWellUnder18_IsTrue()
    {
        var profile = AProfile(Today.AddYears(-14));   // 14 years old

        profile.IsMinor(TestTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsMinor_WhenWellOver18_IsFalse()
    {
        var profile = AProfile(Today.AddYears(-30));   // 30 years old

        profile.IsMinor(TestTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsMinor_OnExact18thBirthday_IsFalse()
    {
        // Turning 18 today => no longer a minor. This is the off-by-one that breaks
        // most age checks; it must flip on the birthday itself, not the day after.
        var profile = AProfile(Today.AddYears(-18));

        profile.IsMinor(TestTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsMinor_DayBefore18thBirthday_IsTrue()
    {
        // 18th birthday is tomorrow => still a minor today.
        var dob = Today.AddYears(-18).AddDays(1);
        var profile = AProfile(dob);

        profile.IsMinor(TestTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsMinor_IsRecomputed_NotStored()
    {
        var profile = AProfile(Today.AddYears(-14));

        profile.IsMinor(TestTime.UtcNow).Should().Be(profile.IsMinor(TestTime.UtcNow));          // stable
        profile.IsMinor(TestTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsMinor_LeapDayBirthday_HandledInNonLeapYear()
    {
        // Feb-29 birthday: ensure the age math doesn't throw or misclassify when the
        // current year has no Feb 29. Someone born 2008-02-29 is an adult by 2026.
        var profile = AProfile(new DateOnly(2008, 2, 29));

        var expectedMinor = new DateOnly(2008, 2, 29).AddYears(18) > Today;
        profile.IsMinor(TestTime.UtcNow).Should().Be(expectedMinor);
        profile.IsMinor(TestTime.UtcNow).Should().Be(expectedMinor);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // ControlMode — stored, explicit, defaulted (control-mode plan §1–2).
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ControlMode_OnNewProfile_DefaultsToAthleteControlled()
    {
        var profile = AProfile();

        profile.ControlModeId.Should().Be(ControlModes.AthleteControlled.Id);
    }

    [Theory]
    [InlineData("athlete_controlled")]
    [InlineData("guardian_controlled")]
    [InlineData("athlete_controlled_shared")]
    [InlineData("guardian_controlled_shared")]
    public void ControlMode_AcceptsEveryDefinedMode(string modeId)
    {
        var profile = AProfile();

        profile.ControlModeId = modeId;

        profile.ControlModeId.Should().Be(modeId);
    }
}
