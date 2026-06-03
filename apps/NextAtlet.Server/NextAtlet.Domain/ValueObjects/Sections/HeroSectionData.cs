namespace NextAtlet.Domain.ValueObjects.Sections;

/// <summary>
/// Hero section payload. Was the loose dict { "headline": {...}, "subheading": {...},
/// "backgroundImageAssetId": "uuid|null" }.
/// </summary>
public class HeroSectionData : SectionData
{
    public const string TypeId = "hero";

    public override string TypeKey => TypeId;

    public LocalizedText Headline { get; set; } = new();
    public LocalizedText? Subheading { get; set; }

    /// <summary>Optional reference to a MediaAsset. Typed as Guid? — no more string/UUID parsing.</summary>
    public Guid? BackgroundImageAssetId { get; set; }
}
