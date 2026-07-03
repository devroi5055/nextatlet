using System.Text.RegularExpressions;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Infrastructure.Services;

/// <summary>
/// Sanitizes free-text fields to prevent XSS attacks.
/// Applied to all text content before save. Operates on the typed layout model —
/// each section type's text fields are sanitized in place.
/// </summary>
public class SanitizationService : ISanitizationService
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
    /// Sanitizes every text field in the layout, in place, and returns the same instance.
    /// Safe because the layout comes from a freshly-deserialized request (not shared).
    /// New section types add a case in <see cref="SanitizeSection"/>.
    /// </summary>
    public SiteLayout SanitizeLayout(SiteLayout layout)
    {
        foreach (var section in layout.Sections)
            SanitizeSection(section.Data);

        return layout;
    }

    private void SanitizeSection(SectionData data)
    {
        switch (data)
        {
            case HeroSectionData hero:
                if (hero.Headline is not null)
                    SanitizeLocalized(hero.Headline);
                if (hero.Subheading is not null)
                    SanitizeLocalized(hero.Subheading);
                break;

            case BioSectionData bio:
                SanitizeLocalized(bio.Bio);
                foreach (var item in bio.HighlightItems)
                {
                    SanitizeLocalized(item.Label);
                    item.Value = Sanitize(item.Value);
                }
                break;
        }
    }

    /// <summary>Sanitizes each present locale; leaves a missing (null) locale untouched.</summary>
    private void SanitizeLocalized(LocalizedText text)
    {
        if (text.Da is not null) text.Da = Sanitize(text.Da);
        if (text.En is not null) text.En = Sanitize(text.En);
    }
}
