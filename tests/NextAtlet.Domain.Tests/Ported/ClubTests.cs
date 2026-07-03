using NextAtlet.Domain.Entities.ClubRegistry;

namespace NextAtlet.Domain.Tests;

public class ClubTests
{
    private static Club GivenClub(params string[] initialSports) => new()
    {
        SourceKey = "dk-12345",
        Source = "dju_portal",
        CountryId = "dk",
        Name = "Test Club",
        Address = null,
        LastImportedUtc = DateTime.UtcNow,
        SportIds = initialSports.ToList()
    };

    // ── AddSports ─────────────────────────────────────────────────────────

    [Fact]
    public void AddSports_EmptyClub_AddsSports()
    {
        var club = GivenClub();
        club.AddSports(["judo", "sambo"]);
        Assert.Equal(["judo", "sambo"], club.SportIds.OrderBy(x => x));
    }

    [Fact]
    public void AddSports_DuplicateOfExisting_NoDuplicate()
    {
        var club = GivenClub("judo");
        club.AddSports(["judo"]);
        Assert.Single(club.SportIds);
        Assert.Equal("judo", club.SportIds[0]);
    }

    [Fact]
    public void AddSports_MixOfExistingAndNew_AddsOnlyNew()
    {
        var club = GivenClub("judo");
        club.AddSports(["judo", "sambo"]);
        Assert.Equal(2, club.SportIds.Count);
        Assert.Contains("judo", club.SportIds);
        Assert.Contains("sambo", club.SportIds);
    }

    // ── RemoveSports ──────────────────────────────────────────────────────

    [Fact]
    public void RemoveSports_ExistingSport_RemovesIt()
    {
        var club = GivenClub("judo", "sambo");
        club.RemoveSports(["judo"]);
        Assert.Single(club.SportIds);
        Assert.Equal("sambo", club.SportIds[0]);
    }

    [Fact]
    public void RemoveSports_NonExistentSport_NoOp()
    {
        var club = GivenClub("judo");
        club.RemoveSports(["wrestling"]);
        Assert.Single(club.SportIds);
        Assert.Equal("judo", club.SportIds[0]);
    }

    [Fact]
    public void RemoveSports_RemoveAll_LeavesEmpty()
    {
        var club = GivenClub("judo", "sambo");
        club.RemoveSports(["judo", "sambo"]);
        Assert.Empty(club.SportIds);
    }
}
