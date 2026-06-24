using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Infrastructure.Services.SectionRegistry;

/// <summary>
/// Validates Hero sections (<see cref="HeroSectionData"/>).
/// Shape is guaranteed by the type; this asserts business rules only.
/// </summary>
public class HeroSectionValidator : ISectionValidator
{
    public string SectionType => HeroSectionData.TypeId;

    public ValidationResult Validate(SectionData data)
    {
        var result = new ValidationResult { IsValid = true };

        if (data is not HeroSectionData hero)
        {
            result.IsValid = false;
            result.Errors.Add($"Expected '{HeroSectionData.TypeId}' section data but got '{data.TypeKey}'");
            return result;
        }

        // Headline is required and must carry at least one locale.
        if (hero.Headline is null || !hero.Headline.HasAnyValue)
        {
            result.IsValid = false;
            result.Errors.Add("Hero headline must have at least one of 'da' or 'en'");
        }

        // Subheading is optional. BackgroundImageAssetId is Guid? — validity is enforced by the type.
        return result;
    }
}
