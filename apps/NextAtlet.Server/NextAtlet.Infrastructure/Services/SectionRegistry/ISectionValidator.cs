using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Infrastructure.Services.SectionRegistry;

/// <summary>
/// Strategy interface for validating a specific section type. Implementation detail of
/// <see cref="SectionTypeRegistry"/> — the Application layer talks to ISectionTypeRegistry,
/// not to individual validators. Uses the shared <see cref="ValidationResult"/> contract.
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
