using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Application.Features.Individuals.Registration;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Tests;

public class RegisterIndividualSiteGuardianHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────

    private readonly ISiteRepository              _sites     = Substitute.For<ISiteRepository>();
    private readonly ISiteLoginRepository         _logins    = Substitute.For<ISiteLoginRepository>();
    private readonly IIndividualProfileRepository _profiles  = Substitute.For<IIndividualProfileRepository>();
    private readonly IThemeRepository             _themes    = Substitute.For<IThemeRepository>();
    private readonly ISiteSnapshotRepository      _snapshots = Substitute.For<ISiteSnapshotRepository>();
    private readonly IUserRepository              _users     = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork                  _uow       = Substitute.For<IUnitOfWork>();
    private readonly IClock                       _clock     = Substitute.For<IClock>();

    private RegisterIndividualSiteGuardianCommandHandler BuildHandler()
    {
        _clock.UtcNow.Returns(TestHelpers.UtcNow);
        _themes.GetActiveByNameAsync("Classic", Arg.Any<CancellationToken>())
               .Returns(TestHelpers.ClassicTheme());

        var thresholds  = new AgeThresholdOptions { AbsoluteMinimumAge = 13, SelfConsentAge = 16, GuardianBoundary = 18 };
        var provisioner = new UserProvisioner(_users, _clock);
        return new RegisterIndividualSiteGuardianCommandHandler(
            _sites, _logins, _profiles, _themes, _snapshots, provisioner, _clock, thresholds, _uow);
    }

    // Convenience: child DateOfBirth at given age on the reference date.
    private static DateTime DobForAge(int age) =>
        TestHelpers.UtcNow.Date.AddYears(-age).AddDays(-1);

    private RegisterIndividualSiteGuardianCommand Command(DateTime childDob, string slug = "child-profile") =>
        new("auth0|guardian", "guardian@test.com", "Child Name", slug, childDob, "en");

    private void GivenNewGuardianUser()
    {
        _users.GetByAuthProviderIdAsync("auth0|guardian", Arg.Any<CancellationToken>())
              .Returns((User?)null);
    }

    private void GivenSlugAvailable()
    {
        _sites.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
    }

    // ── Age gate ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ChildIsAdult_ReturnsGuardianCannotRegisterAdultError()
    {
        var handler = BuildHandler();
        var result  = await handler.Handle(Command(childDob: DobForAge(18)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.GuardianCannotRegisterAdult, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ChildIsExactly17_PassesAgeGate()
    {
        // 17 is OlderMinor — a legitimate guardian-register scenario.
        GivenNewGuardianUser();
        GivenSlugAvailable();

        var handler = BuildHandler();
        var result  = await handler.Handle(Command(childDob: DobForAge(17)), CancellationToken.None);

        Assert.False(result.IsFailure && result.Error!.Code == ErrorCodes.GuardianCannotRegisterAdult);
    }

    // ── Slug validation ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SlugAlreadyTaken_ReturnsSlugAlreadyTakenError()
    {
        GivenNewGuardianUser();
        _sites.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var handler = BuildHandler();
        var result  = await handler.Handle(Command(childDob: DobForAge(10)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.SlugAlreadyTaken, result.Error!.Code);
    }

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidMinorChild_CreatesProfileAndGuardianLogin()
    {
        GivenNewGuardianUser();
        GivenSlugAvailable();

        var handler = BuildHandler();
        var result  = await handler.Handle(Command(childDob: DobForAge(10)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("child-profile", result.Value!.Slug);
        // Guardian login was added.
        _logins.Received(1).Add(Arg.Is<SiteLogin>(l =>
            l.SiteRoleId == NextAtlet.Domain.Enumerations.Individual.IndividualRole.Guardian.Id));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
