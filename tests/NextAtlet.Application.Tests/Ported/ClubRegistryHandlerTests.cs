using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.ClubRegistry.Commands;
using NextAtlet.Domain.Entities.ClubRegistry;
using NextAtlet.Domain.Enumerations.Verification;

namespace NextAtlet.Application.Tests;

public class ClubRegistryHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly IClubRepository _clubs = Substitute.For<IClubRepository>();
    private readonly IUnitOfWork     _uow   = Substitute.For<IUnitOfWork>();

    private static Club ClubWithSports(params string[] sports)
    {
        var club = new Club
        {
            SourceKey        = "DJU|001",
            Source           = "DJU",
            CountryId        = "dk",
            Name             = "Test Club",
            Address          = null,
            LastImportedUtc  = DateTime.UtcNow
        };
        club.SportIds = sports.ToList();
        return club;
    }

    private static Club ClubWithOfficials(IReadOnlyCollection<ClubOfficial> officials)
    {
        // Club.Officials has init-only navigation; we create a subclass to set it.
        var club = new Club
        {
            SourceKey       = "DJU|002",
            Source          = "DJU",
            CountryId       = "dk",
            Name            = "Officials Club",
            Address         = null,
            LastImportedUtc = DateTime.UtcNow
        };
        // IReadOnlyCollection<ClubOfficial> is init-only navigation; reflected via the property setter.
        typeof(Club)
            .GetProperty(nameof(Club.Officials))!
            .SetValue(club, officials);
        return club;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AddSportsCommandHandler
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddSports_ClubNotFound_ThrowsDomainException()
    {
        _clubs.GetClubByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
              .Returns((Club?)null);

        var handler = new AddSportsCommandHandler(_clubs, _uow);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new AddSportsCommand(Guid.NewGuid(), ["judo"]), CancellationToken.None));

        Assert.Equal(ErrorCodes.ClubNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task AddSports_ClubFound_AddsSportsAndSaves()
    {
        var club = ClubWithSports("judo");
        _clubs.GetClubByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(club);

        var handler = new AddSportsCommandHandler(_clubs, _uow);
        await handler.Handle(new AddSportsCommand(Guid.NewGuid(), ["boxing"]), CancellationToken.None);

        Assert.Contains("judo",   club.SportIds);
        Assert.Contains("boxing", club.SportIds);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RemoveSportsCommandHandler
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveSports_ClubNotFound_ThrowsDomainException()
    {
        _clubs.GetClubByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
              .Returns((Club?)null);

        var handler = new RemoveSportsCommandHandler(_clubs, _uow);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new RemoveSportsCommand(Guid.NewGuid(), ["judo"]), CancellationToken.None));

        Assert.Equal(ErrorCodes.ClubNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task RemoveSports_ClubFound_RemovesSportsAndSaves()
    {
        var club = ClubWithSports("judo", "boxing");
        _clubs.GetClubByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(club);

        var handler = new RemoveSportsCommandHandler(_clubs, _uow);
        await handler.Handle(new RemoveSportsCommand(Guid.NewGuid(), ["boxing"]), CancellationToken.None);

        Assert.Contains("judo",         club.SportIds);
        Assert.DoesNotContain("boxing", club.SportIds);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ListClubOfficialsCommandHandler
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListOfficials_ClubNotFound_ThrowsDomainException()
    {
        _clubs.GetClubByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
              .Returns((Club?)null);

        var handler = new ListClubOfficialsCommandHandler(_clubs);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new ListClubOfficialsCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(ErrorCodes.ClubNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task ListOfficials_ClubHasOfficials_ReturnsList()
    {
        var official = new ClubOfficial
        {
            ClubId = Guid.NewGuid(),
            Name   = "Chair Person",
            Email  = "chair@club.dk",
            Phone  = "+45 12345678",
            RoleId = ClubOfficialRole.Chairman.Id
        };
        var club = ClubWithOfficials([official]);
        _clubs.GetClubByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(club);

        var handler = new ListClubOfficialsCommandHandler(_clubs);
        var result  = await handler.Handle(new ListClubOfficialsCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Chair Person", result.Value![0].Name);
    }
}
