namespace NextAtlet.Domain.ValueObjects.Sections;

/// <summary>
/// Bio section payload. Was the loose dict { "bio": {...}, "highlightItems": [ { "label": {...}, "value": "..." } ] }.
/// </summary>
public class BioSectionData : SectionData
{
    public const string TypeId = "bio";
    public override string TypeKey => TypeId;
    public static class Variants
    {
        public const string Classic = "classic";
    }
    public string Variant { get; set; } = Variants.Classic; 
    public LocalizedText Bio { get; set; } = new();
    public List<HighlightItem> HighlightItems { get; set; } = [];
}

/// <summary>A single labelled highlight (e.g. "World ranking" → "#3").</summary>
public class HighlightItem
{
    public LocalizedText Label { get; set; } = new();
    public string Value { get; set; } = string.Empty;
}
