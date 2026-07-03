using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Individuals.Control;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests;

public class TransferControlHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly ISiteRepository              _sites    = Substitute.For<ISiteRepository>();
    private readonly ISiteLoginRepository         _logins   = Substitute.For<ISiteLoginRepository>();
    private readonly IUserRepository              _users    = Substitute.For<IUserRepository>();
    private readonly IIndividualProfileRepository _profiles = Substitute.For<IIndividualProfileRepository>();
    private readonly IUnitOfWork                  _uow      = Substitute.For<IUnitOfWork>();
    private readonly IClock                       _clock    = Substitute.For<IClock>();

    private TransferControlCommandHandler BuildHandler()
    {
        _clock.UtcNow.Returns(TestHelpers.UtcNow);
        return new TransferControlCommandHandler(
            _sites, _logins, _users, new PermissionResolver(), _uow, _clock, _profiles);
    }

    // Profile that is currently guardian-controlled (caller will be the guardian controller).
    private static IndividualProfile GuardianControlledProfile(Guid siteId, DateOnly dob) => new()
    {
        SiteId         = siteId,
        DateOfBirth    = dob,
        ConsentStateId = ConsentStates.NotRequired.Id,
        ControlModeId  = ControlModes.GuardianControlled.Id
    };

    private static IndividualProfile AthleteControlledProfile(Guid siteId, DateOnly dob) => new()
    {
        SiteId         = siteId,
        DateOfBirth    = dob,
        ConsentStateId = ConsentStates.NotRequired.Id,
        ControlModeId  = ControlModes.AthleteControlled.Id
    };

    // ── Input validation ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_InvalidTransferTarget_ReturnsTransferTargetInvalidError()
    {
        var handler = BuildHandler();
        var result  = await handler.Handle(
            new TransferControlCommand(Guid.NewGuid(), "auth0|caller", "both"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.TransferTargetInvalid, result.Error!.Code);
    }

    // ── Not found / auth ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProfileNotFound_ReturnsProfileNotFoundError()
    {
        _profiles.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                 .Returns((IndividualProfile?)null);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            new TransferControlCommand(Guid.NewGuid(), "auth0|caller", TransferControlCommandHandler.ToAthlete),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.IndividualProfileNotFound, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_CallerNotFound_ThrowsInvalidOperation()
    {
        var siteId    = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var profile   = GuardianControlledProfile(siteId, new DateOnly(2010, 1, 1));

        _profiles.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var handler = BuildHandler();
        // An authenticated caller with no User row violates an invariant → throws (not a NotAuthorized result).
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new TransferControlCommand(profileId, "auth0|ghost", TransferControlCommandHandler.ToAthlete),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CallerNotController_ReturnsNotAuthorizedError()
    {
        var caller    = TestHelpers.AdultUser();
        var siteId    = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        // Profile is athlete-controlled; caller has a guardian login → not the controller.
        var profile   = AthleteControlledProfile(siteId, new DateOnly(2010, 1, 1));
        var login     = SiteLogin.CreateGuardian(caller.Id, siteId);

        _profiles.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            new TransferControlCommand(profileId, caller.AuthProviderId!, TransferControlCommandHandler.ToAthlete),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotAuthorized, result.Error!.Code);
    }

    // ── Transfer-to-athlete guards ────────────────────────────────────────────

    [Fact]
    public async Task Handle_TransferToAthlete_AthleteToYoung_ReturnsAthleteTooYoungError()
    {
        var caller    = TestHelpers.AdultUser();
        var siteId    = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        // 10-year-old: BelowMinimum band — cannot receive control.
        var dob     = new DateOnly(TestHelpers.UtcNow.Year - 10, 1, 1);
        var profile = GuardianControlledProfile(siteId, dob);
        var login   = SiteLogin.CreateGuardian(caller.Id, siteId);

        _profiles.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            new TransferControlCommand(profileId, caller.AuthProviderId!, TransferControlCommandHandler.ToAthlete),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.AthleteTooYoungForControl, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_TransferToAthlete_NoAthleteLogin_ReturnsNoAthleteLoginError()
    {
        var caller    = TestHelpers.AdultUser();
        var siteId    = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var dob       = new DateOnly(TestHelpers.UtcNow.Year - 15, 1, 1); // 15 — above minimum
        var profile   = GuardianControlledProfile(siteId, dob);
        var login     = SiteLogin.CreateGuardian(caller.Id, siteId);

        _profiles.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);
        _logins.HasActiveOwnerLoginAsync(profileId, Arg.Any<CancellationToken>()).Returns(false);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            new TransferControlCommand(profileId, caller.AuthProviderId!, TransferControlCommandHandler.ToAthlete),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NoAthleteLoginExists, result.Error!.Code);
    }

    // ── Transfer-to-guardian guards ───────────────────────────────────────────

    [Fact]
    public async Task Handle_TransferToGuardian_NoGuardianLogin_ReturnsNoGuardianLoginError()
    {
        var caller    = TestHelpers.AdultUser();
        var siteId    = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var profile   = AthleteControlledProfile(siteId, new DateOnly(2000, 1, 1));
        var login     = SiteLogin.CreateAthlete(caller.Id, siteId);

        _profiles.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);
        _logins.HasActiveGuardianLoginAsync(profileId, Arg.Any<CancellationToken>()).Returns(false);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            new TransferControlCommand(profileId, caller.AuthProviderId!, TransferControlCommandHandler.ToGuardian),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NoGuardianLoginExists, result.Error!.Code);
    }

    // ── Happy paths ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_TransferToAthlete_SetsAthleteControlledMode()
    {
        var caller    = TestHelpers.AdultUser();
        var siteId    = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var dob       = new DateOnly(TestHelpers.UtcNow.Year - 15, 1, 1);
        var profile   = GuardianControlledProfile(siteId, dob);
        var login     = SiteLogin.CreateGuardian(caller.Id, siteId);

        _profiles.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);
        _logins.HasActiveOwnerLoginAsync(profileId, Arg.Any<CancellationToken>()).Returns(true);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            new TransferControlCommand(profileId, caller.AuthProviderId!, TransferControlCommandHandler.ToAthlete),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ControlModes.AthleteControlled.Id, profile.ControlModeId);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TransferToGuardian_SetsGuardianControlledMode()
    {
        var caller    = TestHelpers.AdultUser();
        var siteId    = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var profile   = AthleteControlledProfile(siteId, new DateOnly(2000, 1, 1));
        var login     = SiteLogin.CreateAthlete(caller.Id, siteId);

        _profiles.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);
        _logins.HasActiveGuardianLoginAsync(profileId, Arg.Any<CancellationToken>()).Returns(true);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            new TransferControlCommand(profileId, caller.AuthProviderId!, TransferControlCommandHandler.ToGuardian),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ControlModes.GuardianControlled.Id, profile.ControlModeId);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
