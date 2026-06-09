using AutoFixture;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="SiteConfig"/> — always a valid default Layout (hero + bio) and
/// GlobalSettings. Draft vs published differ by <c>IsDraft</c> + <c>PublishedUtc</c>.
/// </summary>
public static class SiteConfigs
{
    public static SiteConfig ADraftSiteConfig(Action<SiteConfig>? customize = null)
        => ASiteConfig(isDraft: true, publishedUtc: null, customize);

    public static SiteConfig APublishedSiteConfig(DateTime? publishedUtc = null, Action<SiteConfig>? customize = null)
        => ASiteConfig(isDraft: false, publishedUtc: publishedUtc ?? DateTime.UtcNow, customize);

    public static SiteConfig ASiteConfig(bool isDraft = true, DateTime? publishedUtc = null, Action<SiteConfig>? customize = null)
    {
        var config = TestFixture.Create().Build<SiteConfig>()
            .Without(c => c.AthleteProfile)
            .Without(c => c.Theme)
            .With(c => c.AthleteProfileId, Guid.NewGuid())
            .With(c => c.ThemeId, Guid.NewGuid())
            .With(c => c.ThemeVersion, 1)
            .With(c => c.Version, 1)
            .With(c => c.IsDraft, isDraft)
            .With(c => c.Layout, DefaultLayout())
            .With(c => c.GlobalSettings, new GlobalSettings { AccentColor = "#ffd700", FontFamily = "Inter" })
            .With(c => c.PublishedUtc, publishedUtc)
            .Create();
        customize?.Invoke(config);
        return config;
    }

    private static SiteLayout DefaultLayout() => new()
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
}
