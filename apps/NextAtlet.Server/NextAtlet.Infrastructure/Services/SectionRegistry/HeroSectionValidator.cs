namespace NextAtlet.Infrastructure.Services.SectionRegistry;

/// <summary>
/// Validates Hero sections.
/// Expected data structure:
/// {
///   "headline": { "da": "...", "en": "..." },
///   "subheading": { "da": "...", "en": "..." },
///   "backgroundImageAssetId": "uuid or null"
/// }
/// </summary>
public class HeroSectionValidator : ISectionValidator
{
    public string SectionType => "hero";

    public ValidationResult Validate(Section section)
    {
        var result = new ValidationResult { IsValid = true };

        if (section.Data == null)
        {
            result.IsValid = false;
            result.Errors.Add("Hero section data cannot be null");
            return result;
        }

        // Validate headline (required, with translations)
        if (!ValidateTranslatedField(section.Data, "headline"))
        {
            result.IsValid = false;
            result.Errors.Add("Hero headline must be a localized object with 'da' and/or 'en' keys");
        }

        // Validate subheading (optional, but if present must be localized)
        if (section.Data.TryGetValue("subheading", out var subheading) && subheading != null)
        {
            if (!ValidateTranslatedField(section.Data, "subheading"))
            {
                result.IsValid = false;
                result.Errors.Add("Hero subheading must be a localized object with 'da' and/or 'en' keys");
            }
        }

        // Validate backgroundImageAssetId (optional, must be null or valid UUID)
        if (section.Data.TryGetValue("backgroundImageAssetId", out var bgImage) && bgImage != null)
        {
            if (bgImage is not string bgImageStr || !Guid.TryParse(bgImageStr, out _))
            {
                result.IsValid = false;
                result.Errors.Add("Hero backgroundImageAssetId must be a valid UUID or null");
            }
        }

        return result;
    }

    private bool ValidateTranslatedField(Dictionary<string, object> data, string fieldName)
    {
        if (!data.TryGetValue(fieldName, out var field) || field == null)
            return false;

        if (field is not Dictionary<string, object> translations)
        {
            // Try to deserialize if it's a JsonElement or similar
            if (field is System.Text.Json.JsonElement jsonElement)
            {
                translations = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText())
                    ?? new Dictionary<string, object>();
            }
            else
            {
                return false;
            }
        }

        // At least one of 'da' or 'en' must be present
        var hasLocale = translations.ContainsKey("da") || translations.ContainsKey("en");
        return hasLocale;
    }
}
