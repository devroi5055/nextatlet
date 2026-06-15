using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Media;

public sealed class MediaAssetType : Enumeration
{
    public static readonly MediaAssetType Image = new()
    {
        Id = "image",
        Title = new LocalizedText { Da = "Billede", En = "Image" },
        Description = new LocalizedText { Da = "Foto eller grafik", En = "Photo or graphic" }
    };

    public static readonly MediaAssetType Video = new()
    {
        Id = "video",
        Title = new LocalizedText { Da = "Video", En = "Video" },
        Description = new LocalizedText { Da = "Hostet video. Kun tilgængeligt på betalte tiers — gratis tier bruger indlejrede links", En = "Hosted video. Only available on paid tiers — free tier uses embedded links instead" }
    };

    public static IReadOnlyCollection<MediaAssetType> All => [Image, Video];

    public static MediaAssetType FromId(string id) =>
        All.FirstOrDefault(t => t.Id == id)
        ?? throw new ArgumentException($"Unknown media asset type: '{id}'");
}
