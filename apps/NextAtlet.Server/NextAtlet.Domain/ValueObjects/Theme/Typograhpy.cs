using NextAtlet.Domain.strings;

namespace NextAtlet.Domain.ValueObjects;
public class Typography
{
    public string HeadingFont { get; set; } = Strings.Fonts.Sora;
    public string BodyFont { get; set; } = Strings.Fonts.Inter;
    public string HeadingWeight { get; set; } = "700";
    public string BodyWeight { get; set; } = "400";
}
