using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Shared;

public class Theme : CreatedOnlyEntity
{
    public required string Name { get; set; }

    /// <summary>
    /// Bumped when the theme changes. AthleteSiteSnapshot pins ThemeVersion at
    /// creation time so a theme update never breaks existing published snapshots.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Declares supported section types, color/font slots, and constraints.
    /// This is the render contract between backend and frontend.
    /// </summary>
    public required ThemeManifest Manifest { get; set; }

    public string? PreviewImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
