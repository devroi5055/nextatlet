using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Interfaces.Services;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Infrastructure.Services.SectionRegistry;

/// <summary>
/// Validates Bio sections (<see cref="BioSectionData"/>).
/// Shape is guaranteed by the type; this asserts business rules only.
/// </summary>
public class BioSectionValidator : ISectionValidator
{
    public string SectionType => BioSectionData.TypeId;

    public ValidationResult Validate(SectionData data)
    {
        var result = new ValidationResult { IsValid = true };

        if (data is not BioSectionData bio)
        {
            result.IsValid = false;
            result.Errors.Add($"Expected '{BioSectionData.TypeId}' section data but got '{data.TypeKey}'");
            return result;
        }

        // Bio text is required and must carry at least one locale.
        if (!bio.Bio.HasAnyValue)
        {
            result.IsValid = false;
            result.Errors.Add("Bio text must have at least one of 'da' or 'en'");
        }

        for (var i = 0; i < bio.HighlightItems.Count; i++)
        {
            var item = bio.HighlightItems[i];

            if (!item.Label.HasAnyValue)
            {
                result.IsValid = false;
                result.Errors.Add($"Bio highlightItem[{i}].label must have at least one of 'da' or 'en'");
            }

            if (string.IsNullOrWhiteSpace(item.Value))
            {
                result.IsValid = false;
                result.Errors.Add($"Bio highlightItem[{i}].value must be a non-empty string");
            }
        }

        return result;
    }
}
