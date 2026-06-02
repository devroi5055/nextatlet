namespace NextAtlet.Infrastructure.Services.SectionRegistry;

/// <summary>
/// Represents a section in the Layout jsonb payload.
/// </summary>
public class Section
{
    public string? Id { get; set; }
    public required string Type { get; set; }
    public int Order { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Result of section validation.
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Strategy interface for validating a specific section type.
/// </summary>
public interface ISectionValidator
{
    /// <summary>
    /// The section type this validator handles (e.g. "hero", "bio").
    /// </summary>
    string SectionType { get; }

    /// <summary>
    /// Validates a section's data and returns any errors.
    /// </summary>
    ValidationResult Validate(Section section);
}
