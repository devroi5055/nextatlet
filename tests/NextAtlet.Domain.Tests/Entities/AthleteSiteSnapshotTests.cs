using FluentAssertions;
using NextAtlet.Domain.Entities.Athlete;
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

    private static AthleteSiteSnapshot ADraftSnapshot() => new()
    {
        AthleteProfileId = Guid.NewGuid(),
        ThemeId = Guid.NewGuid(),
        ThemeVersion = 1,
        Layout = AValidLayout(),
        GlobalSettings = new GlobalSettings { AccentColor = "#ffd700", FontFamily = "Inter" },
        Version = 1
    };

    [Fact]
    public void NewSnapshot_StartsAtVersionOne()
    {
        var snapshot = ADraftSnapshot();

        snapshot.Version.Should().Be(1);
    }

    [Fact]
    public void DraftSnapshot_HasNoPublishedTimestamp()
    {
        var snapshot = ADraftSnapshot();

        snapshot.PublishedUtc.Should().BeNull();
    }
}
