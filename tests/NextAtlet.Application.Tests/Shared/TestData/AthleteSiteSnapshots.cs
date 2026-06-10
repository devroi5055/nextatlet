using AutoFixture;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="AthleteSiteSnapshot"/> — always a valid default Layout (hero + bio) and
/// GlobalSettings. Draft vs published identified by PublishedUtc.
/// </summary>
public static class AthleteSiteSnapshots
{
    public static AthleteSiteSnapshot ADraftSiteSnapshot(Action<AthleteSiteSnapshot>? customize = null)
        => ASiteSnapshot(publishedUtc: null, customize);

    public static AthleteSiteSnapshot APublishedSiteSnapshot(DateTime? publishedUtc = null, Action<AthleteSiteSnapshot>? customize = null)
        => ASiteSnapshot(publishedUtc: publishedUtc ?? DateTime.UtcNow, customize);

    public static AthleteSiteSnapshot ASiteSnapshot(DateTime? publishedUtc = null, Action<AthleteSiteSnapshot>? customize = null)
    {
        var snapshot = TestFixture.Create().Build<AthleteSiteSnapshot>()
            .Without(c => c.AthleteProfile)
            .Without(c => c.Theme)
            .With(c => c.AthleteProfileId, Guid.NewGuid())
            .With(c => c.ThemeId, Guid.NewGuid())
            .With(c => c.ThemeVersion, 1)
            .With(c => c.Version, 1)
            .With(c => c.Layout, DefaultLayout())
            .With(c => c.GlobalSettings, new GlobalSettings { AccentColor = "#ffd700", FontFamily = "Inter" })
            .With(c => c.PublishedUtc, publishedUtc)
            .Create();
        customize?.Invoke(snapshot);
        return snapshot;
    }

    public static SiteLayout DefaultLayout() => new()
    {
        Sections =
        [
            new SiteSection
            {
                Id = Guid.NewGuid().ToString(),
                Order = 0,
                Data = new HeroSectionData { Headline = new LocalizedText(), Subheading = new LocalizedText() }
            },
            new SiteSection
            {
                Id = Guid.NewGuid().ToString(),
                Order = 1,
                Data = new BioSectionData { Bio = new LocalizedText() }
            }
        ]
    };

    public static Theme DefaultFreeTheme() => new()
    {
        Id = Guid.NewGuid(),
        Version = 1,
        Name = "Classic",
        MinimumTierId = AthleteTier.Free.Id,
        Manifest = new ThemeManifest
        {
            SupportedSectionTypes = ["hero", "bio"],
            ColorSlots = ["primary", "accent", "background"],
            FontSlots = ["heading", "body"]
        },
    };
}
