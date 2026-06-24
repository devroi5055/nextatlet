using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Individuals.Control;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests;

public class SetCollaborationHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly ISiteRepository              _sites    = Substitute.For<ISiteRepository>();
    private readonly ISiteLoginRepository         _logins   = Substitute.For<ISiteLoginRepository>();
    private readonly IUserRepository              _users    = Substitute.For<IUserRepository>();
    private readonly IIndividualProfileRepository _profiles = Substitute.For<IIndividualProfileRepository>();
    private readonly IUnitOfWork                  _uow      = Substitute.For<IUnitOfWork>();

    private SetCollaborationCommandHandler BuildHandler() =>
        new(_sites, _logins, _users, new PermissionResolver(), _uow, _profiles);

    private static IndividualProfile ProfileWithMode(Guid siteId, string controlModeId) => new()
    {
        SiteId         = siteId,
        DateOfBirth    = new DateOnly(2000, 1, 1),
        ConsentStateId = ConsentStates.NotRequired.Id,
        ControlModeId  = controlModeId
    };

    // ── Not found / auth ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProfileNotFound_ReturnsProfileNotFoundError()
    {
        _profiles.GetBySiteIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                 .Returns((IndividualProfile?)null);

        var result = await BuildHandler().Handle(
            new SetCollaborationCommand(Guid.NewGuid(), "auth0|caller", true),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.IndividualProfileNotFound, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_CallerNotFound_ReturnsNotAuthorizedError()
    {
        var siteId = Guid.NewGuid();
        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>())
                 .Returns(ProfileWithMode(siteId, ControlModes.AthleteControlled.Id));
        _users.GetByAuthProviderIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var result = await BuildHandler().Handle(
            new SetCollaborationCommand(siteId, "auth0|ghost", true),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotAuthorized, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_CallerNotController_ReturnsNotAuthorizedError()
    {
        var caller = TestHelpers.AdultUser();
        var siteId = Guid.NewGuid();
        // Athlete-controlled but caller has guardian login → not the controller.
        var profile  = ProfileWithMode(siteId, ControlModes.AthleteControlled.Id);
        var login    = SiteLogin.CreateGuardian(caller.Id, siteId);

        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        var result = await BuildHandler().Handle(
            new SetCollaborationCommand(siteId, caller.AuthProviderId!, true),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotAuthorized, result.Error!.Code);
    }

    // ── Toggle shared ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EnableSharing_AthleteControlled_FlipsToShared()
    {
        var caller  = TestHelpers.AdultUser();
        var siteId  = Guid.NewGuid();
        var profile = ProfileWithMode(siteId, ControlModes.AthleteControlled.Id);
        var login   = SiteLogin.CreateAthlete(caller.Id, siteId);

        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        var result = await BuildHandler().Handle(
            new SetCollaborationCommand(siteId, caller.AuthProviderId!, SharedEditing: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ControlModes.AthleteControlledShared.Id, profile.ControlModeId);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DisableSharing_AthleteControlledShared_FlipsToUnshared()
    {
        var caller  = TestHelpers.AdultUser();
        var siteId  = Guid.NewGuid();
        var profile = ProfileWithMode(siteId, ControlModes.AthleteControlledShared.Id);
        var login   = SiteLogin.CreateAthlete(caller.Id, siteId);

        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        var result = await BuildHandler().Handle(
            new SetCollaborationCommand(siteId, caller.AuthProviderId!, SharedEditing: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ControlModes.AthleteControlled.Id, profile.ControlModeId);
    }

    [Fact]
    public async Task Handle_EnableSharing_GuardianControlled_FlipsToShared()
    {
        var caller  = TestHelpers.AdultUser("auth0|guardian");
        var siteId  = Guid.NewGuid();
        var profile = ProfileWithMode(siteId, ControlModes.GuardianControlled.Id);
        var login   = SiteLogin.CreateGuardian(caller.Id, siteId);

        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        var result = await BuildHandler().Handle(
            new SetCollaborationCommand(siteId, caller.AuthProviderId!, SharedEditing: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ControlModes.GuardianControlledShared.Id, profile.ControlModeId);
    }

    [Fact]
    public async Task Handle_DisableSharing_GuardianControlledShared_FlipsToUnshared()
    {
        var caller  = TestHelpers.AdultUser("auth0|guardian");
        var siteId  = Guid.NewGuid();
        var profile = ProfileWithMode(siteId, ControlModes.GuardianControlledShared.Id);
        var login   = SiteLogin.CreateGuardian(caller.Id, siteId);

        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        var result = await BuildHandler().Handle(
            new SetCollaborationCommand(siteId, caller.AuthProviderId!, SharedEditing: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ControlModes.GuardianControlled.Id, profile.ControlModeId);
    }

    [Fact]
    public async Task Handle_AlreadyInTargetState_IsNoOp()
    {
        var caller  = TestHelpers.AdultUser();
        var siteId  = Guid.NewGuid();
        // AthleteControlled + enable sharing → AthleteControlledShared
        // Calling enable again from AthleteControlledShared → same mode (no-op via _ branch)
        var profile = ProfileWithMode(siteId, ControlModes.AthleteControlledShared.Id);
        var login   = SiteLogin.CreateAthlete(caller.Id, siteId);

        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        // Enable sharing when already shared — hits the _ arm → mode unchanged
        var result = await BuildHandler().Handle(
            new SetCollaborationCommand(siteId, caller.AuthProviderId!, SharedEditing: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ControlModes.AthleteControlledShared.Id, profile.ControlModeId);
    }
}
