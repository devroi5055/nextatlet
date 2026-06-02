namespace NextAtlet.Domain.Entities;

public class SiteConfig
{
    public Guid Id { get; set; }
    public required Guid AthleteProfileId { get; set; }
    public required string State { get; set; } = "Draft"; // "Draft", "Published"
    public required Guid ThemeId { get; set; }
    public int ThemeVersion { get; set; } = 1;
    public required Dictionary<string, object> Layout { get; set; } // jsonb: { sections: [...] }
    public Dictionary<string, object>? GlobalSettings { get; set; } // jsonb
    public int Version { get; set; } = 1; // optimistic concurrency
    public DateTime? PublishedUtc { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public AthleteProfile? AthleteProfile { get; set; }
    public Theme? Theme { get; set; }
}
