namespace NextAtlet.Domain.ValueObjects;


/// <summary>
/// Declares the FeatureKey(s) required to unlock a theme.
/// Null MinimumCapability on a Theme means available to all tiers.
/// </summary>
public class ThemeCapabilityRequirement
{
    public List<string> RequiredFeatureKeys { get; set; } = [];
}