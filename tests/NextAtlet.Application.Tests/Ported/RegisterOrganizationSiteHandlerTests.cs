using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Application.Features.Organizations.Registration;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests;

public class RegisterOrganizationSiteHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly ISiteRepository              _sites     = Substitute.For<ISiteRepository>();
    private readonly IOrganizationProfileRepository _profiles = Substitute.For<IOrganizationProfileRepository>();
    private readonly IThemeRepository              _themes    = Substitute.For<IThemeRepository>();
    private readonly ISiteSnapshotRepository       _snapshots = Substitute.For<ISiteSnapshotRepository>();
    private readonly IUserRepository               _users     = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork                   _uow       = Substitute.For<IUnitOfWork>();
    private readonly IClock                        _clock     = Substitute.For<IClock>();

    private RegisterOrganizationSiteCommandHandler BuildHandler()
    {
        _clock.UtcNow.Returns(TestHelpers.UtcNow);
        _themes.GetActiveByNameAsync("Classic", Arg.Any<CancellationToken>())
               .Returns(TestHelpers.ClassicTheme());
        var provisioner = new UserProvisioner(_users, _clock);
        return new RegisterOrganizationSiteCommandHandler(
            _sites, _profiles, _themes, _snapshots, provisioner, _uow);
    }

    private static RegisterOrganizationSiteCommand Command(string slug = "test-club") =>
        new("auth0|org", "org@test.com", slug, "Test Club", "club_free", "en", "club");

    private void GivenNewUser()
    {
        _users.GetByAuthProviderIdAsync("auth0|org", Arg.Any<CancellationToken>())
              .Returns((User?)null);
    }

    private void GivenSlugAvailable()
    {
        _sites.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
    }

    // ── Slug validation ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SlugAlreadyTaken_ReturnsSlugAlreadyTakenError()
    {
        GivenNewUser();
        _sites.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await BuildHandler().Handle(Command(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.SlugAlreadyTaken, result.Error!.Code);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("api")]
    [InlineData("settings")]
    [InlineData("dashboard")]
    public async Task Handle_ReservedSlug_ReturnsSlugReservedError(string reserved)
    {
        GivenNewUser();
        GivenSlugAvailable();

        var result = await BuildHandler().Handle(Command(slug: reserved), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.SlugReserved, result.Error!.Code);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_CreatesSiteProfileSnapshotAndSavesOnce()
    {
        GivenNewUser();
        GivenSlugAvailable();

        var result = await BuildHandler().Handle(Command("new-club"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("new-club", result.Value!.Slug);
        Assert.Equal("Test Club", result.Value.DisplayName);
        _sites.Received(1).Add(Arg.Any<NextAtlet.Domain.Entities.Sites.Site>());
        _profiles.Received(1).Add(Arg.Any<NextAtlet.Domain.Entities.Sites.OrganizationProfile>());
        _snapshots.Received(1).Add(Arg.Any<NextAtlet.Domain.Entities.Sites.SiteSnapshot>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ClassicThemeNotFound_ThrowsInvalidOperationException()
    {
        GivenNewUser();
        GivenSlugAvailable();
        // Build the handler first, then override the theme mock so BuildHandler's
        // internal setup doesn't re-set it when invoked inside the lambda.
        var handler = BuildHandler();
        _themes.GetActiveByNameAsync("Classic", Arg.Any<CancellationToken>())
               .Returns((NextAtlet.Domain.Entities.Sites.Theme?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(Command(), CancellationToken.None));
    }
}
