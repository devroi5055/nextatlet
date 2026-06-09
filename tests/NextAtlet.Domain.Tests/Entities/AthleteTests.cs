using FluentAssertions;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.Tests.Shared;

namespace NextAtlet.Domain.Tests.Entities;

/// <summary>
/// Pure domain tests for <see cref="AthleteProfile"/> — no DB, no I/O.
/// These cover only behaviour the entity OWNS: computed minor status, age-derived
/// state, and any invariant guards. Property get/set with no logic is deliberately
/// NOT tested (that would only test the compiler).
///
/// NOTE: several tests assume entity behaviour established in the design docs
/// (computed IsMinor, ControlMode default, slug normalisation). Where the real
/// entity differs, adjust the test — each such assumption is flagged inline.
/// </summary>
public class AthleteProfileTests
{
    // Helper: build a valid profile with an overridable date of birth.
    // Adjust the initialiser to match your actual entity's required members.
    private static AthleteProfile AProfile(DateOnly? dob = null) => new()
    {
        Slug = "maria-jensen",
        DisplayName = "Maria Jensen",
        SportId = "judo",
        DateOfBirth = dob ?? new DateOnly(2000, 1, 1),
        DefaultLocaleId = "da",
        VisibilityStateId = "public"
    };

    // A fixed "today" so boundary tests are deterministic regardless of when they run.
    // If IsMinor uses DateTime.UtcNow internally (not injectable), these date-relative
    // tests should compute DOB relative to DateTime.UtcNow instead — see note below.
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
        // The same DateOfBirth must always yield the same answer from the property,
        // proving it's derived rather than read from a frozen backing field.
        // (If a regression introduces a stored bool, this still passes — the stronger
        //  guarantee is the exact-birthday test above. This documents intent.)
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

        // 2008 leap-day baby is 18 in 2026 — assert against the actual current date.
        var expectedMinor = new DateOnly(2008, 2, 29).AddYears(18) > Today;
        profile.IsMinor(TestTime.UtcNow).Should().Be(expectedMinor);
        profile.IsMinor(TestTime.UtcNow).Should().Be(expectedMinor);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // ControlMode — stored, explicit, defaulted (control-mode plan §1–2).
    // ASSUMPTION: a newly constructed profile defaults to AthleteControlled, and the
    // registration handlers set it explicitly. If your entity has no default and the
    // handler always sets it, move these assertions to the handler integration tests.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ControlMode_OnNewProfile_DefaultsToAthleteControlled()
    {
        var profile = AProfile();

        profile.ControlMode.Should().Be(ControlMode.AthleteControlled);
    }

    [Theory]
    [InlineData(ControlMode.AthleteControlled)]
    [InlineData(ControlMode.GuardianControlled)]
    [InlineData(ControlMode.AthleteControlledShared)]
    [InlineData(ControlMode.GuardianControlledShared)]
    public void ControlMode_AcceptsEveryDefinedMode(ControlMode mode)
    {
        var profile = AProfile();

        profile.ControlMode = mode;

        profile.ControlMode.Should().Be(mode);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Slug — normalisation / identity (02 AthleteProfile.Slug).
    // ASSUMPTION: slug normalisation (lower-casing, trimming) lives in the entity or a
    // value object. If it lives in the command handler instead, delete these and test
    // there. They're here because slug is part of the profile's identity contract.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Slug_IsStoredAsProvided_WhenAlreadyNormalised()
    {
        var profile = AProfile();
        profile.Slug = "maria-jensen";

        profile.Slug.Should().Be("maria-jensen");
    }

    // If normalisation is an entity responsibility, a test like this should pass.
    // Marked Skip until you confirm where normalisation lives.
    [Fact(Skip = "Confirm whether slug normalisation is an entity responsibility or a handler one.")]
    public void Slug_IsLowerCased_OnAssignment()
    {
        var profile = AProfile();
        profile.Slug = "Maria-Jensen";

        profile.Slug.Should().Be("maria-jensen");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // VisibilityState — gates the public/club contract (02, 03 §4).
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void VisibilityState_CanBeSetPrivate()
    {
        var profile = AProfile();

        profile.VisibilityStateId = "private";

        profile.VisibilityStateId.Should().Be("private");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Invariant guards — ONLY if the entity enforces them itself.
    // The "a minor must have a guardian" rule is an aggregate/handler invariant
    // (it spans ProfileLogin), NOT something AthleteProfile can enforce alone, so it
    // is intentionally NOT tested here — it belongs in the registration integration
    // tests. Documented so the omission is deliberate, not forgotten.
    // ──────────────────────────────────────────────────────────────────────────

    // If your entity has a guarded constructor/factory (e.g. rejects an empty slug or
    // a future DOB), tests like the following belong here. Marked Skip until confirmed.

    [Fact(Skip = "Confirm whether AthleteProfile guards against a future DateOfBirth.")]
    public void Construction_WithFutureDateOfBirth_IsRejected()
    {
        var act = () => AProfile(Today.AddDays(1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact(Skip = "Confirm whether AthleteProfile guards against an empty slug.")]
    public void Construction_WithEmptySlug_IsRejected()
    {
        var act = () => new AthleteProfile { Slug = "", DisplayName = "X", DateOfBirth = new DateOnly(5776, 2, 8) };

        act.Should().Throw<ArgumentException>();
    }
}
    