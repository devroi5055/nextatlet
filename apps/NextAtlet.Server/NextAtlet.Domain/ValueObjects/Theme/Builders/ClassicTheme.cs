using NextAtlet.Domain.strings;
using NextAtlet.Domain.ValueObjects.ThemeStyle;

namespace NextAtlet.Domain.ValueObjects.Theme.Builders;

public static class ClassicTheme
{
    public static readonly Guid Id = new("11111111-1111-1111-1111-111111111111");

    public static ThemeManifest Manifest() => new()
    {
        Colors     = Palette(),
        Typography = new() { HeadingFont = Strings.Fonts.Sora, BodyFont = Strings.Fonts.Inter },
        ComponentStyles = Components(),
        // no SectionStyleVariation — Classic uses defaults
    };

    private static ColorPalette Palette() => new()
    {
        Primary    = "#BA4336",
        Secondary  = "#874942",
        Accent     = "#EC2A15",
        Background = "#FAF8F7",
        Surface    = "#FFFFFF",
        Text       = "#332E2D"
    };

    private static Dictionary<string, StyleableElement> Components() => new()
    {
        ["buttons"] = Button(),
        ["cards"]   = Card(),
    };

    private static StyleableElement Button() => new()
    {
        Overrides = new() { [Strings.StyleKeys.Radius] = Strings.StyleValues.Rounded },
        Options =
        [
            new StyleOption
            {
                Key = "sharp",
                DisplayName = "Sharp Edges",
                Styles = new() { [Strings.StyleKeys.Radius] = Strings.StyleValues.None }
            }
        ]
    };

    private static StyleableElement Card() => new()
    {
        Overrides = new() { [Strings.StyleKeys.Radius] = Strings.StyleValues.Medium }
    };
}

