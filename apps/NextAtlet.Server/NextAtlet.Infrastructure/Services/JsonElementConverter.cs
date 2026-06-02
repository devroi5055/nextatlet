using System.Text.Json;

namespace NextAtlet.Infrastructure.Services;

/// <summary>
/// Helper to convert System.Text.Json elements (from Npgsql jsonb deserialization)
/// to strongly-typed dictionaries for validation.
/// </summary>
public static class JsonElementConverter
{
    /// <summary>
    /// Recursively converts JsonElement to Dictionary<string, object>.
    /// Handles nested objects, arrays, and all JSON types.
    /// </summary>
    public static Dictionary<string, object> ToDict(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Expected JSON object, got {element.ValueKind}");

        var dict = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            dict[property.Name] = ConvertValue(property.Value);
        }

        return dict;
    }

    /// <summary>
    /// Converts a JsonElement value to a C# object.
    /// </summary>
    private static object ConvertValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null!,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Number => element.GetDecimal(),
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Array => ConvertArray(element),
        JsonValueKind.Object => ToDict(element),
        _ => throw new InvalidOperationException($"Unexpected JSON value kind: {element.ValueKind}")
    };

    /// <summary>
    /// Converts a JSON array to List<object>.
    /// </summary>
    private static List<object> ConvertArray(JsonElement element)
    {
        var list = new List<object>();

        foreach (var item in element.EnumerateArray())
        {
            list.Add(ConvertValue(item));
        }

        return list;
    }

    /// <summary>
    /// Safely converts Layout from request (which may contain JsonElement or Dictionary)
    /// into a clean Dictionary<string, object> for processing.
    /// </summary>
    public static Dictionary<string, object> NormalizeLayout(Dictionary<string, object>? layout)
    {
        if (layout == null)
            return new Dictionary<string, object> { { "sections", new List<object>() } };

        // If it's already clean, return as-is
        if (!layout.TryGetValue("sections", out var sectionsObj))
            return layout;

        // If sections is already a List<object>, we're good
        if (sectionsObj is List<object> list)
            return layout;

        // If it's a JsonElement, convert it
        if (sectionsObj is JsonElement jsonElement)
        {
            layout["sections"] = ConvertArray(jsonElement);
        }

        return layout;
    }

    /// <summary>
    /// Converts section data (which may contain JsonElement after deserialization)
    /// to a clean Dictionary.
    /// </summary>
    public static Dictionary<string, object> NormalizeSectionData(Dictionary<string, object>? data)
    {
        if (data == null)
            return new Dictionary<string, object>();

        var normalized = new Dictionary<string, object>();

        foreach (var kvp in data)
        {
            if (kvp.Value is JsonElement je)
            {
                normalized[kvp.Key] = ConvertValue(je);
            }
            else
            {
                normalized[kvp.Key] = kvp.Value;
            }
        }

        return normalized;
    }
}
