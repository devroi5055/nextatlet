using FluentAssertions;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;
using System.Globalization;
using Xunit;

namespace NextAtlet.Domain.Tests.Entities;

public class SiteConfigTests
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

    private static SiteConfig ADraftConfig() => new()
    {
        AthleteProfileId = Guid.NewGuid(),
        IsDraft = true,
        ThemeId = Guid.NewGuid(),
        ThemeVersion = 1,
        Layout = AValidLayout(),
        GlobalSettings = new GlobalSettings { AccentColor = "#ffd700", FontFamily = "Inter" },
        Version = 1
    };

    [Fact]
    public void NewConfig_StartsAtVersionOne()
    {
        var config = ADraftConfig();

        config.Version.Should().Be(1);
    }

    [Fact]
    public void DraftConfig_HasNoPublishedTimestamp()
    {
        var config = ADraftConfig();

        config.PublishedUtc.Should().BeNull();
    }

    //[Fact(Skip = "Confirm whether Publish() is entity behaviour or a command concern.")]
    //TODO: SOMETHING HERE
    //public void Publish_StampsPublishedTimestampAndBumpsVersion()
    //{
    //    var config = ADraftConfig();
    //    var before = config.Version;

    //    config.Publish();

    //    config.PublishedUtc.Should().NotBeNull();
    //    config.Version.Should().Be(before + 1);
    //}
}
