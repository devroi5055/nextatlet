namespace NextAtlet.Domain.ValueObjects;

/// <summary>
/// The render contract between backend and frontend.
/// Declares which section types the theme supports and its design token slots.
/// </summary>
public class ThemeManifest
{
    public List<string> SupportedSectionTypes { get; set; } = [];
    public List<string> ColorSlots { get; set; } = [];
    public List<string> FontSlots { get; set; } = [];
}