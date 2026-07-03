using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;
using NextAtlet.Infrastructure.Services.SectionRegistry;

namespace NextAtlet.Infrastructure.Tests.Services.SectionRegistry;

/// <summary>
/// Section validators (Strategy) + the <see cref="SectionTypeRegistry"/> (Registry) that routes a
/// section's typed data to its validator. Shape is guaranteed by the type; these assert business rules.
/// </summary>
public class SectionValidatorTests
{
    // ── Hero ────────────────────────────────────────────────────────────────

    [Fact]
    public void Hero_WithHeadline_IsValid()
    {
        var data = new HeroSectionData { Headline = new LocalizedText { Da = "Forsidetekst" } };

        var result = new HeroSectionValidator().Validate(data);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Hero_WithoutHeadline_IsInvalid()
    {
        var result = new HeroSectionValidator().Validate(new HeroSectionData { Headline = null });

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Hero_WithEmptyHeadline_IsInvalid()
    {
        var data = new HeroSectionData { Headline = new LocalizedText { Da = "  ", En = null } };

        Assert.False(new HeroSectionValidator().Validate(data).IsValid);
    }

    [Fact]
    public void Hero_WrongDataType_IsInvalid()
    {
        var result = new HeroSectionValidator().Validate(new BioSectionData { Bio = new LocalizedText { Da = "x" } });

        Assert.False(result.IsValid);
    }

    // ── Bio ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Bio_WithBioAndValidHighlights_IsValid()
    {
        var data = new BioSectionData
        {
            Bio = new LocalizedText { En = "My story" },
            HighlightItems = { new HighlightItem { Label = new LocalizedText { En = "Rank" }, Value = "#1" } }
        };

        var result = new BioSectionValidator().Validate(data);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Bio_WithoutBioText_IsInvalid()
    {
        Assert.False(new BioSectionValidator().Validate(new BioSectionData()).IsValid);
    }

    [Fact]
    public void Bio_HighlightWithoutLabelOrValue_IsInvalid()
    {
        var data = new BioSectionData
        {
            Bio = new LocalizedText { En = "ok" },
            HighlightItems = { new HighlightItem { Label = new LocalizedText(), Value = "" } }
        };

        var result = new BioSectionValidator().Validate(data);

        Assert.False(result.IsValid);
        // one error for the missing label, one for the empty value.
        Assert.True(result.Errors.Count >= 2);
    }

    [Fact]
    public void Bio_WrongDataType_IsInvalid()
    {
        Assert.False(new BioSectionValidator().Validate(new HeroSectionData()).IsValid);
    }

    // ── Registry ──────────────────────────────────────────────────────────────

    [Fact]
    public void Registry_SupportsRegisteredSectionTypes()
    {
        var registry = new SectionTypeRegistry();

        Assert.True(registry.IsSupported(HeroSectionData.TypeId));
        Assert.True(registry.IsSupported(BioSectionData.TypeId));
        Assert.False(registry.IsSupported("gallery"));
    }

    [Fact]
    public void Registry_RoutesToTheMatchingValidator()
    {
        var registry = new SectionTypeRegistry();

        var valid = registry.Validate(new HeroSectionData { Headline = new LocalizedText { Da = "Hej" } });
        var invalid = registry.Validate(new HeroSectionData { Headline = null });

        Assert.True(valid.IsValid);
        Assert.False(invalid.IsValid);
    }

    [Fact]
    public void Registry_UnregisteredType_ReturnsInvalid()
    {
        var registry = new SectionTypeRegistry();

        var result = registry.Validate(new UnknownSectionData());

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Registry_Register_AddsSupportForNewType()
    {
        var registry = new SectionTypeRegistry();
        registry.Register(new StubValidator());

        Assert.True(registry.IsSupported("stub"));
        Assert.True(registry.Validate(new UnknownSectionData()).IsValid);
    }

    private sealed class UnknownSectionData : SectionData
    {
        public override string TypeKey => "stub";
    }

    private sealed class StubValidator : ISectionValidator
    {
        public string SectionType => "stub";
        public ValidationResult Validate(SectionData data) => new() { IsValid = true };
    }
}
