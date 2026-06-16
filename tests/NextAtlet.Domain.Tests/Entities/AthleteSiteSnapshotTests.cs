using FluentAssertions;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Domain.Tests.Entities;

public class AthleteSiteSnapshotTests
{
    private static SiteLayout AValidLayout() => new()
    {
        Sections =
        [
            new SiteSection
            {
                Id = Guid.NewGuid().ToString(),
                Order = 0,
                Data = new HeroSectionData
                {
                    Headline = new LocalizedText(),
                    Subheading = new LocalizedText()
                }
            },
            new SiteSection
            {
                Id = Guid.NewGuid().ToString(),
                Order = 1,
                Data = new BioSectionData { Bio = new LocalizedText() }
            }
        ]
    };

    private static SiteSnapshot ADraftSnapshot() => new()
    {
        SiteId = Guid.NewGuid(),
        ThemeId = Guid.NewGuid(),
        Layout = AValidLayout(),
        GlobalSettings = new GlobalSettings { AccentColor = "#ffd700", FontFamily = "Inter" }
    };

    [Fact]
    public void DraftSnapshot_HasNoPublishedTimestamp()
    {
        var snapshot = ADraftSnapshot();

        snapshot.PublishedUtc.Should().BeNull();
    }
}
