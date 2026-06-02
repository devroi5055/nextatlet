namespace NextAtlet.Domain.Entities;

public class MediaAsset
{
    public Guid Id { get; set; }
    public required Guid AthleteProfileId { get; set; }
    public required string Type { get; set; } // "Image", "Video"
    public string Origin { get; set; } = "SelfUpload"; // "SelfUpload", "AdminUpload", "ClubFundedShoot"
    public bool IsClubBranding { get; set; } = false;
    public required string StorageKey { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AltText { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public AthleteProfile? AthleteProfile { get; set; }
}
