using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Application.Abstractions.Services;

/// <summary>
/// Result of validating a section's typed payload.
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Knows which section types are supported and validates a section payload against
/// its type-specific rules. The per-type validator strategy is an Infrastructure detail
/// hidden behind this abstraction.
/// </summary>
public interface ISectionTypeRegistry
{
    bool IsSupported(string sectionType);

    ValidationResult Validate(SectionData data);
}
