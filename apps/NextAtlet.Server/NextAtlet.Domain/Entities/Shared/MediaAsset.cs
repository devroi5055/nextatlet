using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Media;

namespace NextAtlet.Domain.Entities.Shared;

/// <summary>
/// Bytes live in blob/CDN — this is the reference only.
/// Owner is either an AthleteProfile OR an Organization (never both, never neither).
/// Enforced by a CHECK constraint in the DB migration.
/// </summary>
public class MediaAsset : CreatedOnlyEntity
{
    // Owner — XOR (exactly one non-null)
    public Guid? AthleteSiteId { get; set; }
    public Guid? OrganizationId { get; set; }

    public required string TypeId { get; set; } = MediaAssetType.Image.Id;
    public string OriginId { get; set; } = MediaOrigin.SelfUpload.Id;

    /// <summary>
    /// If true, this asset may revert to the club on membership end.
    /// Default false — capture funded ≠ identity owned.
    /// </summary>
    public bool IsClubBranding { get; set; } = false;

    /// <summary>
    /// Content-hashed blob/CDN key. Immutable once set.
    /// </summary>
    public required string StorageKey { get; set; }

    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AltText { get; set; }

    // Navigation — nullable because owner is XOR
    public AthleteProfile? AthleteSite { get; set; }

    //TODO: implement Organization
    // public Organization.Organization? Organization { get; set; }
}