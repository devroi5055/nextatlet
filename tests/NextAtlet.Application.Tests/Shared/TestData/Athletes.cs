using AutoFixture;
using NextAtlet.Domain.Entities.AthleteProfile;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="AthleteProfile"/>. Age-band variants set DateOfBirth relative to a
/// reference "now" so the computed minor/age-band logic is exercised; ControlMode variants cover the
/// stored control fact (default AthleteControlled).
/// </summary>
public static class TestAthletes
{
    public static AthleteSite AnAthlete(
        DateOnly? dateOfBirth = null,
        string controlModeId = "athlete_controlled",
        Action<AthleteSite>? customize = null)
    {
        var athlete = TestFixture.Create().Build<AthleteSite>()
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
            .With(a => a.ControlModeId, controlModeId ?? ControlMode.AthleteControlled.Id)
            .With(a => a.ConsentStateId, ConsentState.NotRequired.Id)
            .Create();
        customize?.Invoke(athlete);
        return athlete;
    }

    // ── Age-band variants (mid-band ages, safe from boundary drift) ──
    public static AthleteSite AnUnder13Athlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(8, asOfUtc));
    public static AthleteSite AYoungMinorAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(14, asOfUtc));
    public static AthleteSite AnOlderMinorAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(17, asOfUtc));
    public static AthleteSite AnAdultAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(30, asOfUtc));

    // ── ControlMode variants ──
    public static AthleteSite AnAthleteControlledProfile() => AnAthlete(controlModeId: ControlMode.AthleteControlled.Id);
    public static AthleteSite AGuardianControlledProfile() => AnAthlete(controlModeId: ControlMode.GuardianControlled.Id);
    public static AthleteSite AnAthleteControlledSharedProfile() => AnAthlete(controlModeId: ControlMode.AthleteControlledShared.Id);
    public static AthleteSite AGuardianControlledSharedProfile() => AnAthlete(controlModeId: ControlMode.GuardianControlledShared.Id);

    // ── ConsentState variant: awaiting guardian consent (publish-gated, draft-editable) ──
    public static AthleteSite APendingGuardianConsentAthlete(DateTime? asOfUtc = null)
        => AnAthlete(AgeYears(8, asOfUtc), customize: a => a.ConsentStateId = ConsentState.PendingGuardianConsent.Id);

    private static DateOnly AgeYears(int years, DateTime? asOfUtc = null)
        => DateOnly.FromDateTime((asOfUtc ?? DateTime.UtcNow).AddYears(-years));
}
