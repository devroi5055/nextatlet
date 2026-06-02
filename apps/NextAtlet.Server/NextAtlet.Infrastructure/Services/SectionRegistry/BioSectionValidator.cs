namespace NextAtlet.Infrastructure.Services.SectionRegistry;

/// <summary>
/// Validates Bio sections.
/// Expected data structure:
/// {
///   "bio": { "da": "...", "en": "..." },
///   "highlightItems": [
///     { "label": { "da": "...", "en": "..." }, "value": "..." }
///   ]
/// }
/// </summary>
public class BioSectionValidator : ISectionValidator
{
    public string SectionType => "bio";

    public ValidationResult Validate(Section section)
    {
        var result = new ValidationResult { IsValid = true };

        if (section.Data == null)
        {
            result.IsValid = false;
            result.Errors.Add("Bio section data cannot be null");
            return result;
        }

        // Validate bio text (required, with translations)
        if (!ValidateTranslatedField(section.Data, "bio"))
        {
            result.IsValid = false;
            result.Errors.Add("Bio text must be a localized object with 'da' and/or 'en' keys");
        }

        // Validate highlightItems (optional array)
        if (section.Data.TryGetValue("highlightItems", out var items) && items != null)
        {
            if (items is not System.Collections.IEnumerable enumItems)
            {
                result.IsValid = false;
                result.Errors.Add("Bio highlightItems must be an array");
            }
            else
            {
                int itemIndex = 0;
                foreach (var item in enumItems)
                {
                    if (item is Dictionary<string, object> highlightItem)
                    {
                        if (!ValidateTranslatedField(highlightItem, "label"))
                        {
                            result.IsValid = false;
                            result.Errors.Add($"Bio highlightItem[{itemIndex}].label must be a localized object with 'da' and/or 'en' keys");
                        }

                        if (!highlightItem.TryGetValue("value", out var value) || value == null || value is not string)
                        {
                            result.IsValid = false;
                            result.Errors.Add($"Bio highlightItem[{itemIndex}].value must be a non-empty string");
                        }
                    }
                    itemIndex++;
                }
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
