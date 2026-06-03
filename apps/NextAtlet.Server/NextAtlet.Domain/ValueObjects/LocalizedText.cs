using System.Text.Json.Serialization;
using NextAtlet.Domain.Enumerations;

namespace NextAtlet.Domain.ValueObjects;

/// <summary>
/// A short translatable text field. Per-field locale map (da/en at MVP).
/// Replaces the loose Dictionary&lt;string,string&gt; { "da": "...", "en": "..." } shape.
/// </summary>
public class LocalizedText
{
    public string? Da { get; set; }
    public string? En { get; set; }

    /// <summary>True if at least one locale has non-whitespace content.</summary>
    [JsonIgnore]
    public bool HasAnyValue => !string.IsNullOrWhiteSpace(Da) || !string.IsNullOrWhiteSpace(En);

    /// <summary>
    /// Resolve the value for a locale, falling back to the other locale, then to <paramref name="fallback"/>.
    /// Locale ids come from the <see cref="Locale"/> enumeration — the single source of truth.
    /// </summary>
    public string Get(string localeId, string fallback = "")
        => (localeId == Locale.En.Id ? En ?? Da : Da ?? En) ?? fallback;
}
