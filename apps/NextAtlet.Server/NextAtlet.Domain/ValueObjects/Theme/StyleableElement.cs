namespace NextAtlet.Domain.ValueObjects;

// one type serves both — default + options, string dictionaries
public class StyleableElement
{
    public required Dictionary<string, string> Overrides { get; set; }
    public List<StyleOption> Options { get; set; } = [];
}

public class StyleOption
{
    public required string Key { get; set; }
    public required string DisplayName { get; set; }
    public required Dictionary<string, string> Styles { get; set; }
}