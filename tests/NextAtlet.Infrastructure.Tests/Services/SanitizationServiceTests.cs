using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;
using NextAtlet.Infrastructure.Services;

namespace NextAtlet.Infrastructure.Tests.Services;

/// <summary>
/// <see cref="SanitizationService"/> strips the XSS surface (tags, javascript: protocol, event
/// handlers) from free text and walks the typed layout, sanitizing each section's text in place.
/// </summary>
public class SanitizationServiceTests
{
    private readonly SanitizationService _sut = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Sanitize_NullOrWhitespace_ReturnsEmpty(string? input)
    {
        Assert.Equal(string.Empty, _sut.Sanitize(input));
    }

    [Fact]
    public void Sanitize_StripsHtmlTags()
    {
        var result = _sut.Sanitize("<b>Bold</b> and <i>italic</i>");

        Assert.DoesNotContain("<", result);
        Assert.Equal("Bold and italic", result);
    }

    [Fact]
    public void Sanitize_RemovesScriptElementMarkup()
    {
        var result = _sut.Sanitize("<script>alert('xss')</script>Safe");

        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain("script", result);
        Assert.Contains("Safe", result);
    }

    [Fact]
    public void Sanitize_RemovesJavascriptProtocol()
    {
        var result = _sut.Sanitize("javascript:stealCookies()");

        Assert.DoesNotContain("javascript:", result);
    }

    [Fact]
    public void Sanitize_RemovesEventHandlerAttributes_WhenTagAlreadyStripped()
    {
        // After tag-stripping a dangling "onclick=" must still be removed.
        var result = _sut.Sanitize("hello onclick= world");

        Assert.DoesNotContain("onclick=", result);
    }

    [Fact]
    public void Sanitize_DecodesEntities_AndNormalizesWhitespace()
    {
        var result = _sut.Sanitize("  Tom   &amp;   Jerry  ");

        Assert.Equal("Tom & Jerry", result);
    }

    [Fact]
    public void SanitizeLayout_SanitizesHeroAndBioTextInPlace_AndReturnsSameInstance()
    {
        var hero = new HeroSectionData
        {
            Headline   = new LocalizedText { Da = "<b>Hej</b>", En = "<i>Hi</i>" },
            Subheading = new LocalizedText { Da = "<span>under</span>", En = null }
        };
        var bio = new BioSectionData
        {
            Bio = new LocalizedText { Da = "<p>Min bio</p>", En = "<p>My bio</p>" },
            HighlightItems =
            {
                new HighlightItem { Label = new LocalizedText { En = "<u>Rank</u>" }, Value = "<b>#1</b>" }
            }
        };
        var layout = new SiteLayout
        {
            Sections =
            {
                new SiteSection { Id = Guid.NewGuid(), Order = 0, Data = hero },
                new SiteSection { Id = Guid.NewGuid(), Order = 1, Data = bio }
            }
        };

        var returned = _sut.SanitizeLayout(layout);

        Assert.Same(layout, returned);
        Assert.Equal("Hej", hero.Headline!.Da);
        Assert.Equal("Hi", hero.Headline.En);
        Assert.Equal("under", hero.Subheading!.Da);
        Assert.Equal("Min bio", bio.Bio.Da);
        Assert.Equal("My bio", bio.Bio.En);
        Assert.Equal("Rank", bio.HighlightItems[0].Label.En);
        Assert.Equal("#1", bio.HighlightItems[0].Value);
    }
}
