using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Interfaces.Services;

/// <summary>
/// Strips XSS-prone content from free text. Implemented in Infrastructure.
/// </summary>
public interface ISanitizationService
{
    string Sanitize(string? input);

    /// <summary>Sanitizes every text field in the layout and returns it.</summary>
    SiteLayout SanitizeLayout(SiteLayout layout);
}
