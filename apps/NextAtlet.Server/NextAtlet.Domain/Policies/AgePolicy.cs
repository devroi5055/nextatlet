using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Domain.Policies;

/// <summary>
/// Computes an athlete's age and <see cref="AgeBand"/>. Thresholds (13/16/18) are hardcoded for the
/// Denmark MVP — move to IOptions when expanding to other EU markets.
/// </summary>
public static class AgePolicy
{
    /// <summary>Completed years between <paramref name="dob"/> and <paramref name="on"/>.</summary>
    public static int AgeAt(DateOnly dob, DateOnly on)
    {
        var age = on.Year - dob.Year;
        if (dob > on.AddYears(-age)) age--; // birthday hasn't occurred yet this year
        return age;
    }

    public static AgeBand BandToday(DateOnly dob, DateTime utcNow) =>
        AgeAt(dob, DateOnly.FromDateTime(utcNow)) switch
        {
            < 13 => AgeBand.BelowMinimum,
            < 16 => AgeBand.YoungMinor,
            < 18 => AgeBand.OlderMinor,
            _    => AgeBand.Adult
        };

    public static AgeBand BandToday(DateTime dob, DateTime utcNow) => BandToday(DateOnly.FromDateTime(dob), utcNow);

    /// <summary>
    /// True when a guardian must consent (GDPR Art. 8): the athlete is below the self-consent age.
    /// The threshold is passed in (from <c>AgeThresholdOptions</c>) so Domain stays config-free and the
    /// rule is trivially testable at any threshold (e.g. an EU shift to 16).
    /// </summary>
    public static bool RequiresGuardianConsent(DateOnly dob, DateTime utcNow, int selfConsentAge)
        => AgeAt(dob, DateOnly.FromDateTime(utcNow)) < selfConsentAge;

    public static bool RequiresGuardianConsent(DateTime dob, DateTime utcNow, int selfConsentAge)
        => RequiresGuardianConsent(DateOnly.FromDateTime(dob), utcNow, selfConsentAge);
}
