using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Domain.ValueObjects.ThemeStyle;
public class ColorPalette
{
    public required string Primary { get; set; }
    public required string Secondary { get; set; }
    public required string Accent { get; set; }
    public required string Background { get; set; }
    public required string Surface { get; set; }
    public required string Text { get; set; }
    //public string Button { get; set; } = "";

}
