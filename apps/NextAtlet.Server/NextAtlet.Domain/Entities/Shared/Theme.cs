using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Shared;

public class Theme : AuditableEntity
{
    public required string Name { get; set; }

    /// <summary>
    /// Bumped when the theme changes. SiteConfig pins ThemeVersion at
    /// creation time so a theme update never breaks existing published configs.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// The FeatureKey(s) required to unlock this theme.
    /// Uses the same capability vocabulary as PlanCapability.
    /// Null means available to all tiers (e.g. the Free "Classic" theme).
    /// </summary>
    public ThemeCapabilityRequirement? MinimumCapability { get; set; }

    /// <summary>
    /// Declares supported section types, color/font slots, and constraints.
    /// This is the render contract between backend and frontend.
    /// </summary>
    public required ThemeManifest Manifest { get; set; }

    public string? PreviewImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<SiteConfig> SiteConfigs { get; set; } = [];

    //TODO: implement ClubPageConfig and add navigation here
    //public ICollection<ClubPageConfig> ClubPageConfigs { get; set; } = [];
}
