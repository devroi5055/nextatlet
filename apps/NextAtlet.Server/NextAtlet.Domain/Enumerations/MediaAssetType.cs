using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class MediaAssetType : Enumeration
{
    public static readonly MediaAssetType Image = new()
    {
        Id = "image",
        Title = "Image",
        Description = "Photo or graphic"
    };

    public static readonly MediaAssetType Video = new()
    {
        Id = "video",
        Title = "Video",
        Description = "Hosted video. Only available on paid tiers — free tier uses embedded links instead"
    };

    public static IReadOnlyCollection<MediaAssetType> All => [Image, Video];

    public static MediaAssetType FromId(string id) =>
        All.FirstOrDefault(t => t.Id == id)
        ?? throw new ArgumentException($"Unknown media asset type: '{id}'");
}