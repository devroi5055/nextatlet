using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Infrastructure.Services.SectionRegistry;

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
/// Receives the already-typed (polymorphically deserialized) payload — validators now
/// assert business rules, not JSON shape (the type system covers shape).
/// </summary>
public interface ISectionValidator
{
    /// <summary>
    /// The section type this validator handles (e.g. HeroSectionData.TypeId).
    /// </summary>
    string SectionType { get; }

    /// <summary>
    /// Validates a section's typed data and returns any errors.
    /// </summary>
    ValidationResult Validate(SectionData data);
}
