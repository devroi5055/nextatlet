namespace NextAtlet.Application.Common.Options;

/// <summary>
/// Configurable age thresholds, bound from the "AgeThresholds" config section. Defaults are the
/// Denmark launch values; the EU's likely shift to a self-consent age of 16 is then a config change,
/// not a code change. Single set of thresholds for now — no per-country matrix.
/// </summary>
public class AgeThresholdOptions
{
    public const string SectionName = "AgeThresholds";

    /// <summary>Cannot self-register at all below this age.</summary>
    public int AbsoluteMinimumAge { get; set; } = 13;

    /// <summary>Below this age a guardian must consent (GDPR Art. 8). DK = 13; raise to 16 if EU mandates.</summary>
    public int SelfConsentAge { get; set; } = 16;

    /// <summary>Adult boundary — guardian-register is rejected at/above this; the control/guardian boundary.</summary>
    public int GuardianBoundary { get; set; } = 18;
}
