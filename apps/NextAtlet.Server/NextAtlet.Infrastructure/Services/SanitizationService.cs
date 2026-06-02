using System.Text.RegularExpressions;

namespace NextAtlet.Infrastructure.Services;

/// <summary>
/// Sanitizes free-text fields to prevent XSS attacks.
/// Applied to all text content before save.
/// </summary>
public class SanitizationService
{
    private static readonly Regex HtmlTagsRegex = new(@"<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex ScriptPatternRegex = new(@"javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex EventHandlerRegex = new(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove HTML tags
        var sanitized = HtmlTagsRegex.Replace(input, string.Empty);

        // Remove javascript: protocol
        sanitized = ScriptPatternRegex.Replace(sanitized, string.Empty);

        // Remove event handlers
        sanitized = EventHandlerRegex.Replace(sanitized, string.Empty);

        // HTML-decode common entities (but don't re-encode; keep as plain text)
        sanitized = System.Net.WebUtility.HtmlDecode(sanitized);

        // Trim and normalize whitespace
        sanitized = Regex.Replace(sanitized.Trim(), @"\s+", " ");

        return sanitized;
    }

    /// <summary>
    /// Sanitizes all text fields in a layout jsonb object.
    /// </summary>
    public Dictionary<string, object>? SanitizeLayout(Dictionary<string, object>? layout)
    {
        if (layout == null || !layout.TryGetValue("sections", out var sectionsObj))
            return layout;

        if (sectionsObj is not System.Collections.IEnumerable sections)
            return layout;

        var sanitizedSections = new List<object>();

        foreach (var section in sections)
        {
            if (section is Dictionary<string, object> sectionDict)
            {
                if (sectionDict.TryGetValue("data", out var dataObj) && dataObj is Dictionary<string, object> data)
                {
                    var sanitizedData = SanitizeSectionData(data);
                    sectionDict["data"] = sanitizedData;
                }
            }
            sanitizedSections.Add(section);
        }

        layout["sections"] = sanitizedSections;
        return layout;
    }

    /// <summary>
    /// Recursively sanitizes text fields in section data.
    /// </summary>
    private Dictionary<string, object> SanitizeSectionData(Dictionary<string, object> data)
    {
        var sanitized = new Dictionary<string, object>();

        foreach (var kvp in data)
        {
            if (kvp.Value is string strValue)
            {
                sanitized[kvp.Key] = Sanitize(strValue);
            }
            else if (kvp.Value is Dictionary<string, object> dictValue)
            {
                // Handle localized fields like { "da": "...", "en": "..." }
                sanitized[kvp.Key] = SanitizeLocalizedField(dictValue);
            }
            else if (kvp.Value is System.Collections.IEnumerable enumValue && kvp.Key != "sections")
            {
                // Handle arrays (but not nested sections)
                sanitized[kvp.Key] = SanitizeArray(enumValue);
            }
            else
            {
                sanitized[kvp.Key] = kvp.Value;
            }
        }

        return sanitized;
    }

    private Dictionary<string, object> SanitizeLocalizedField(Dictionary<string, object> localized)
    {
        var sanitized = new Dictionary<string, object>();

        foreach (var kvp in localized)
        {
            if (kvp.Value is string strValue)
            {
                sanitized[kvp.Key] = Sanitize(strValue);
            }
            else
            {
                sanitized[kvp.Key] = kvp.Value;
            }
        }

        return sanitized;
    }

    private List<object> SanitizeArray(System.Collections.IEnumerable array)
    {
        var sanitized = new List<object>();

        foreach (var item in array)
        {
            if (item is string strValue)
            {
                sanitized.Add(Sanitize(strValue));
            }
            else if (item is Dictionary<string, object> dictValue)
            {
                sanitized.Add(SanitizeSectionData(dictValue));
            }
            else if (item is System.Collections.IEnumerable enumValue)
            {
                sanitized.Add(SanitizeArray(enumValue));
            }
            else
            {
                sanitized.Add(item);
            }
        }

        return sanitized;
    }
}
