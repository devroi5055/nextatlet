using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Sites;

public class Theme : CreatedOnlyEntity, IRetirable
{
    public required string Name { get; init; }

    /// <summary>
    /// Declares supported section types, color/font slots, and constraints.
    /// This is the render contract between backend and frontend.
    /// Must never be mutated
    /// </summary>
    public required ThemeManifest Manifest { get; init; }

    public string? PreviewImageUrl { get; set; }


    //RETIRE-ABLE

    public DateTime? RetiredUtc { get; private set; }
    public bool IsRetired => RetiredUtc != null;
    public void Retire(DateTime utcNow) => RetiredUtc = utcNow;
}
