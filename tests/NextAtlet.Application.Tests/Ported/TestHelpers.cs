using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.ThemeStyle;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Shared fixture builders for application-layer handler tests.
/// </summary>
internal static class TestHelpers
{
    internal static readonly DateTime UtcNow = new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    internal static User AdultUser(string authId = "auth0|adult") => new()
    {
        Email = "athlete@test.com",
        AuthProviderId = authId
    };

    internal static Theme ClassicTheme() => new()
    {
        Name = "Classic",
        Manifest = new ThemeManifest
        {
            Colors = new ColorPalette
            {
                Primary    = "#000000",
                Secondary  = "#ffffff",
                Accent     = "#ffd700",
                Background = "#f5f5f5",
                Surface    = "#eeeeee",
                Text       = "#111111"
            }
        }
    };

    internal static Site IndividualSite(string slug = "test-athlete") => new()
    {
        Slug        = slug,
        DisplayName = "Test Athlete",
        SiteTypeId  = NextAtlet.Domain.Enumerations.Individual.SiteType.Individual.Id
    };
}
