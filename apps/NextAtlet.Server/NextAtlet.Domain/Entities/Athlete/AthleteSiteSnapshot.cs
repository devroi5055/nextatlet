using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.Entities.Shared;

namespace NextAtlet.Domain.Entities.Athlete;

public class AthleteSiteSnapshot : CreatedOnlyEntity
{
    public required Guid AthleteProfileId { get; set; }
    public required Guid ThemeId { get; set; }

    /// <summary>
    /// Pins the theme version at snapshot-creation time for render stability.
    /// A theme update does not break existing published snapshots.
    /// </summary>
    public int ThemeVersion { get; set; } = 1;

    /// <summary>
    /// Ordered list of typed sections + per-section data.
    /// Shape: { "sections": [ { "id", "type", "order", "data": { ... } } ] }
    /// Translatable short-text fields use per-field locale maps:
    /// { "headline": { "da": "...", "en": "..." } }
    /// </summary>
    public required SiteLayout Layout { get; set; }

    /// <summary>
    /// Color/font/accent overrides — only slots the effective capability allows.
    /// </summary>
    public GlobalSettings? GlobalSettings { get; set; }

    /// <summary>
    /// Optimistic concurrency token. Increment on every save.
    /// </summary>
    public int Version { get; set; } = 1;

    public DateTime? PublishedUtc { get; set; }

    // Navigation — non-nullable to match non-nullable FKs
    public AthleteSite AthleteSite { get; set; } = default!; //TODO: might remove to not have backwards dependencies
    public Theme Theme { get; set; } = default!;
}
