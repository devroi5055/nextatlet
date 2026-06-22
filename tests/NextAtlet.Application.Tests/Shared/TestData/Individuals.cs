using AutoFixture;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="IndividualProfile"/>. Age-band variants set DateOfBirth relative to a
/// reference "now" so the computed minor/age-band logic is exercised; ControlMode variants cover the
/// stored control fact (default AthleteControlled). Site-level fields (slug, display name, visibility)
/// now live on <see cref="Site"/>, not here.
/// </summary>
public static class TestAthletes
{
    public static IndividualProfile AnAthlete(
        DateOnly? dateOfBirth = null,
        string controlModeId = "athlete_controlled",
        Action<IndividualProfile>? customize = null)
    {
        var athlete = TestFixture.Create().Build<IndividualProfile>()
            .With(a => a.SiteId, Guid.NewGuid())
            .With(a => a.SportId, Sport.Judo.Id)
            .With(a => a.DateOfBirth, dateOfBirth ?? AgeYears(30))
            .With(a => a.ControlModeId, controlModeId ?? ControlModes.AthleteControlled.Id)
            .With(a => a.ConsentStateId, ConsentStates.NotRequired.Id)
            .Create();
        customize?.Invoke(athlete);
        return athlete;
    }

    // ── Age-band variants (mid-band ages, safe from boundary drift) ──
    public static IndividualProfile AnUnder13Athlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(8, asOfUtc));
    public static IndividualProfile AYoungMinorAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(14, asOfUtc));
    public static IndividualProfile AnOlderMinorAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(17, asOfUtc));
    public static IndividualProfile AnAdultAthlete(DateTime? asOfUtc = null) => AnAthlete(AgeYears(30, asOfUtc));

    // ── ControlMode variants ──
    public static IndividualProfile AnAthleteControlledProfile() => AnAthlete(controlModeId: ControlModes.AthleteControlled.Id);
    public static IndividualProfile AGuardianControlledProfile() => AnAthlete(controlModeId: ControlModes.GuardianControlled.Id);
    public static IndividualProfile AnAthleteControlledSharedProfile() => AnAthlete(controlModeId: ControlModes.AthleteControlledShared.Id);
    public static IndividualProfile AGuardianControlledSharedProfile() => AnAthlete(controlModeId: ControlModes.GuardianControlledShared.Id);

    // ── ConsentState variant: awaiting guardian consent (publish-gated, draft-editable) ──
    public static IndividualProfile APendingGuardianConsentAthlete(DateTime? asOfUtc = null)
        => AnAthlete(AgeYears(8, asOfUtc), customize: a => a.ConsentStateId = ConsentStates.PendingGuardianConsent.Id);

    private static DateOnly AgeYears(int years, DateTime? asOfUtc = null)
        => DateOnly.FromDateTime((asOfUtc ?? DateTime.UtcNow).AddYears(-years));
}
