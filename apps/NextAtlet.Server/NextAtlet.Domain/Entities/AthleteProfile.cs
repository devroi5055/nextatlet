namespace NextAtlet.Domain.Entities;

public class AthleteProfile
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string DisplayName { get; set; }
    public required string Sport { get; set; } = "judo";
    public required DateTime DateOfBirth { get; set; }
    public required string DefaultLocale { get; set; } = "da";
    public string VisibilityState { get; set; } = "Public";
    public string? SelfTier { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }

    public ICollection<ProfileLogin> ProfileLogins { get; set; } = [];
    public ICollection<SiteConfig> SiteConfigs { get; set; } = [];
    public ICollection<MediaAsset> MediaAssets { get; set; } = [];

    /// <summary>
    /// Computes if this profile is a minor (under 18) based on DateOfBirth.
    /// This is NOT stored; it's computed at request time.
    /// </summary>
    public bool IsMinor => DateTime.UtcNow.AddYears(-18) < DateOfBirth;
}
