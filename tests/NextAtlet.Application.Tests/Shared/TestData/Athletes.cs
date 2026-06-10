using AutoFixture;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="AthleteProfile"/>. Age-band variants set DateOfBirth relative to a
/// reference "now" so the computed minor/age-band logic is exercised; ControlMode variants cover the
/// stored control fact (default AthleteControlled).
/// </summary>
public static class TestAthletes
{
    public static AthleteProfile AnAthlete(
        DateOnly? dateOfBirth = null,
        ControlMode controlMode = ControlMode.AthleteControlled,
        Action<AthleteProfile>? customize = null)
    {
        var athlete = TestFixture.Create().Build<AthleteProfile>()
            .Without(a => a.ProfileLogins)
            .Without(a => a.CurrentDraftSnapshotId)
            .Without(a => a.CurrentPublishedSnapshotId)
            .Without(a => a.MediaAssets)
            .With(a => a.Slug, "athlete-" + Guid.NewGuid().ToString("N")[..8])
            .With(a => a.DisplayName, "Maria Jensen")
            .With(a => a.SportId, Sport.Judo.Id)
            .With(a => a.DefaultLocaleId, Locale.Da.Id)
            .With(a => a.VisibilityStateId, VisibilityState.Public.Id)
            .With(a => a.DateOfBirth, dateOfBirth ?? AgeYears(30))
            .With(a => a.ControlMode, controlMode)
            .Create();
        customize?.Invoke(athlete);
        return athlete;
    }

    // ── Age-band variants (mid-band ages, safe from boundary drift) ──
    public static AthleteProfile AnUnder13Athlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(8, asOfUtc));
    public static AthleteProfile AYoungMinorAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(14, asOfUtc));
    public static AthleteProfile AnOlderMinorAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(17, asOfUtc));
    public static AthleteProfile AnAdultAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(30, asOfUtc));

    // ── ControlMode variants ──
    public static AthleteProfile AnAthleteControlledProfile() => AnAthlete(controlMode: ControlMode.AthleteControlled);
    public static AthleteProfile AGuardianControlledProfile() => AnAthlete(controlMode: ControlMode.GuardianControlled);
    public static AthleteProfile AnAthleteControlledSharedProfile() => AnAthlete(controlMode: ControlMode.AthleteControlledShared);
    public static AthleteProfile AGuardianControlledSharedProfile() => AnAthlete(controlMode: ControlMode.GuardianControlledShared);

    // ── ConsentState variant: awaiting guardian consent (publish-gated, draft-editable) ──
    public static AthleteProfile APendingGuardianConsentAthlete(DateTime? asOfUtc = null)
        => AnAthlete(AgeYears(8, asOfUtc), customize: a => a.ConsentState = ConsentState.PendingGuardianConsent);

    private static DateOnly AgeYears(int years, DateTime? asOfUtc = null)
        => DateOnly.FromDateTime((asOfUtc ?? DateTime.UtcNow).AddYears(-years));
}
