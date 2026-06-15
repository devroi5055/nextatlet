using System.Text.Json.Serialization;

namespace NextAtlet.Domain.ValueObjects.Sections;

/// <summary>
/// Base type for a section's typed payload. Persisted inside SiteConfig.Layout (jsonb)
/// and carried over the API contract. Polymorphism is driven by the "type" discriminator,
/// so a section's concrete shape is known at compile time — no more Dictionary&lt;string,object&gt;.
///
/// To add a section type: create a SectionData subclass, register a [JsonDerivedType] here,
/// and add an ISectionValidator. The discriminator string is the single source of truth on
/// each subclass (e.g. HeroSectionData.TypeId).
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(HeroSectionData), HeroSectionData.TypeId)]
[JsonDerivedType(typeof(BioSectionData), BioSectionData.TypeId)]
public abstract class SectionData
{
    /// <summary>
    /// Stable section-type key — matches the JSON "type" discriminator and the
    /// SectionTypeRegistry / theme manifest vocabulary. Not re-serialized; the
    /// polymorphic discriminator already writes "type".
    /// </summary>
    [JsonIgnore]
    public abstract string TypeKey { get; }
}
