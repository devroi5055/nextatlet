using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Enumerations.Verification;
using NextAtlet.Infrastructure.Services;

namespace NextAtlet.Infrastructure.Tests.Services;

public class ClubCanonicalizerTests
{
    private readonly ClubCanonicalizer _sut = new();

    private static ScrapedClub ClubWith(
        IEnumerable<string>? sports = null,
        IEnumerable<ScrapedClubOfficial>? officials = null) => new()
        {
            SourceKey = "1",
            Source = "test",
            Name = "Test Klub",
            Address = null,
            Sports = (sports ?? []).ToList(),
            ScrapedOfficials = (officials ?? []).ToList(),
        };

    private static ScrapedClubOfficial OfficialWithRole(string role) => new()
    {
        Name = "Jens Hansen",
        Role = role,
    };

    // ── Sports: substring match handles combined words like "Judoklub" ──

    [Theory]
    [InlineData("judo")]
    [InlineData("Judo")]
    [InlineData("Judoklub")]
    [InlineData("Københavns Judoklub")]
    public void Sports_plain_or_combined_word_resolves_to_Judo(string label)
    {
        var result = _sut.Canonicalize(ClubWith(sports: [label]));

        Assert.Equal(new[] { Sport.Judo.Id }, result.Sports);
    }

    [Theory]
    [InlineData("jiujitsu")]
    [InlineData("jiu_jitsu")]
    [InlineData("Jiu-Jitsu Klub")]
    [InlineData("jujutsu")]
    public void Sports_jiujitsu_aliases_resolve_to_JiuJitsu(string label)
    {
        var result = _sut.Canonicalize(ClubWith(sports: [label]));

        Assert.Equal(new[] { Sport.JiuJitsu.Id }, result.Sports);
    }

    [Fact]
    public void Sports_unknown_label_is_dropped()
    {
        var result = _sut.Canonicalize(ClubWith(sports: ["Karate Klub"]));

        Assert.Empty(result.Sports);
    }

    [Fact]
    public void Sports_single_label_naming_two_sports_yields_both()
    {
        var result = _sut.Canonicalize(ClubWith(sports: ["Judo og Jiu-Jitsu Klub"]));

        Assert.Equal(2, result.Sports.Count);
        Assert.Contains(Sport.Judo.Id, result.Sports);
        Assert.Contains(Sport.JiuJitsu.Id, result.Sports);
    }

    [Fact]
    public void Sports_duplicates_and_aliases_are_deduped()
    {
        var result = _sut.Canonicalize(ClubWith(sports: ["Judoklub", "JUDO"]));

        Assert.Equal(new[] { Sport.Judo.Id }, result.Sports);
    }

    [Fact]
    public void Sports_empty_stays_empty()
    {
        var result = _sut.Canonicalize(ClubWith(sports: []));

        Assert.Empty(result.Sports);
    }

    // ── Officials: Danish role labels canonicalize to ClubOfficialRole ids ──

    [Theory]
    [InlineData("Formand", "chairman")]
    [InlineData("Kasserer", "cashier")]
    [InlineData("Postadresse", "postal_address")]
    [InlineData("Instruktør", "instructor")]
    [InlineData("Anden klubkontakt", "other")]
    [InlineData("Webmaster", "other")]   // unrecognized role falls back to Other (no crash)
    public void Officials_danish_role_is_canonicalized_to_role_id(string rawRole, string expectedId)
    {
        var result = _sut.Canonicalize(ClubWith(officials: [OfficialWithRole(rawRole)]));

        Assert.Equal(expectedId, result.ScrapedOfficials.Single().Role);
    }
}
