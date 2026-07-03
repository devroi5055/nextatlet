using AutoFixture;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;
using NextAtlet.Domain.ValueObjects.Theme.Builders;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="SiteSnapshot"/> — always a valid default Layout (hero + bio) and
/// GlobalSettings. Draft vs published identified by PublishedUtc.
/// </summary>
public static class SiteSnapshots
{
    public static SiteSnapshot ADraftSiteSnapshot(Action<SiteSnapshot>? customize = null)
        => ASiteSnapshot(publishedUtc: null, customize);

    public static SiteSnapshot APublishedSiteSnapshot(DateTime? publishedUtc = null, Action<SiteSnapshot>? customize = null)
        => ASiteSnapshot(publishedUtc: publishedUtc ?? DateTime.UtcNow, customize);

    public static SiteSnapshot ASiteSnapshot(DateTime? publishedUtc = null, Action<SiteSnapshot>? customize = null)
    {
        var snapshot = TestFixture.Create().Build<SiteSnapshot>()
            .Without(c => c.Theme)
            .With(c => c.SiteId, Guid.NewGuid())
            .With(c => c.ThemeId, Guid.NewGuid())
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
                Id = Guid.NewGuid(),
                Order = 0,
                Data = new HeroSectionData { Headline = new LocalizedText(), Subheading = new LocalizedText() }
            },
            new SiteSection
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Data = new BioSectionData { Bio = new LocalizedText() }
            }
        ]
    };

    public static Theme DefaultFreeTheme() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Classic",
        Manifest = ClassicTheme.Manifest()
    };
}
