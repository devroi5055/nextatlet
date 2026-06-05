namespace NextAtlet.Domain.Policies;

/// <summary>
/// Age band of an athlete, computed from DateOfBirth at request time — never stored. Its sole job is
/// to <b>gate what is allowed</b> at registration and at control transfer; it never feeds the
/// permission resolver and never mutates who is in control (that is <c>ControlMode</c>, an explicit
/// stored fact).
/// </summary>
public enum AgeBand
{
    BelowMinimum, // < 13
    YoungMinor,   // 13–15
    OlderMinor,   // 16–17
    Adult         // 18+
}

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

    public static AgeBand BandToday(DateOnly dob) =>
        AgeAt(dob, DateOnly.FromDateTime(DateTime.UtcNow)) switch
        {
            < 13 => AgeBand.BelowMinimum,
            < 16 => AgeBand.YoungMinor,
            < 18 => AgeBand.OlderMinor,
            _    => AgeBand.Adult
        };

    public static AgeBand BandToday(DateTime dob) => BandToday(DateOnly.FromDateTime(dob));
}
