using NextAtlet.Domain.ValueObjects.ThemeStyle;

namespace NextAtlet.Domain.ValueObjects;

/// <summary>
/// The render contract between backend and frontend.
/// Declares which section types the theme supports and its design token slots.
/// </summary>
public class ThemeManifest
{
    public required ColorPalette Colors { get; set; }
    public Typography Typography { get; set; } = new();

    // keyed by component name ("buttons", "cards")
    // component style overrides: optional but HIGHLY ENCOURAGED
    // used for making components fit specific themes
    // goes to frontend defaults if none is applied
    public Dictionary<string, StyleableElement> ComponentStyles { get; set; } = [];

    // keyed by "sectionType.variant" ("gallery.carousel")
    // variant overrides: optional, sparse, only when theme has an opinion
    // used for making variants fit specific themes
    // goes to frontend defaults if none is applied
    public Dictionary<string, StyleableElement> SectionStyles { get; set; } = [];
}