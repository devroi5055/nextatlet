namespace NextAtlet.Domain.Entities;

public class Theme
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int Version { get; set; } = 1;
    public Dictionary<string, object>? MinimumCapability { get; set; } // jsonb
    public required Dictionary<string, object> Manifest { get; set; } // jsonb
    public string? PreviewImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SiteConfig> SiteConfigs { get; set; } = [];
}
