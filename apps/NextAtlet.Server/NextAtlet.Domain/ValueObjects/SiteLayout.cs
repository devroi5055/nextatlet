using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Domain.ValueObjects;


/// <summary>
/// Typed wrapper for SiteConfig.Layout jsonb payload.
/// Shape: ordered list of typed sections.
/// </summary>
public class SiteLayout
{
    public List<SiteSection> Sections { get; set; } = [];
}

/// <summary>
/// One ordered section. The section type lives on <see cref="Data"/> as the polymorphic
/// "type" discriminator (and is also exposed via <c>Data.TypeKey</c>).
/// </summary>
public class SiteSection
{
    public required string Id { get; set; }
    public int Order { get; set; }
    public required SectionData Data { get; set; }
}
