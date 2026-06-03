namespace NextAtlet.Domain.ValueObjects;


/// <summary>
/// Typed wrapper for SiteConfig.GlobalSettings jsonb payload.
/// Only slots the effective capability (tier + perks) allows are writable.
/// </summary>
public class GlobalSettings
{
    public string? AccentColor { get; set; }
    public string? FontFamily { get; set; }
    // extend as theme capabilities grow
}