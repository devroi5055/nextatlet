namespace NextAtlet.Domain.ValueObjects.Sections;

/// <summary>
/// Hero section payload. Was the loose dict { "headline": {...}, "subheading": {...},
/// "backgroundImageAssetId": "uuid|null" }.
/// </summary>
public class HeroSectionData : SectionData
{
    public const string TypeId = "hero";
    public override string TypeKey => TypeId;
    public static class Variants
    {
        public const string Classic = "classic";
        public const string Split = "split";
        public const string FullBleed = "fullbleed";
        public const string Video = "video";
    }
    public static class StyleKeys
    {
        public const string Classic = $"{TypeId}.{Variants.Classic}";    // "hero.classic"
        public const string Split = $"{TypeId}.{Variants.Split}";
        public const string FullBleed = $"{TypeId}.{Variants.FullBleed}";
        public const string Video = $"{TypeId}.{Variants.Video}";
    }

    public string Variant { get; set; } = Variants.Classic;
    public LocalizedText? Headline { get; set; }
    public LocalizedText? Subheading { get; set; }

    /// <summary>Optional reference to a MediaAsset. Typed as Guid? — no more string/UUID parsing.</summary>
    public Guid? BackgroundImageAssetId { get; set; }
    public Guid? VideoAssetId { get; set; }

}
