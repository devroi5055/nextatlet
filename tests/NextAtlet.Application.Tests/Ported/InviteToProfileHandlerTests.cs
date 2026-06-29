using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Invitations.Commands;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests;

public class InviteToProfileHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────

    private readonly IUserRepository              _users    = Substitute.For<IUserRepository>();
    private readonly ISiteLoginRepository         _logins   = Substitute.For<ISiteLoginRepository>();
    private readonly IIndividualProfileRepository _profiles = Substitute.For<IIndividualProfileRepository>();
    private readonly IActionTokenRepository       _tokens   = Substitute.For<IActionTokenRepository>();
    private readonly IEmailService                _email    = Substitute.For<IEmailService>();
    private readonly IUnitOfWork                  _uow      = Substitute.For<IUnitOfWork>();
    private readonly IClock                       _clock    = Substitute.For<IClock>();

    private InviteToProfileCommandHandler BuildHandler()
    {
        _clock.UtcNow.Returns(TestHelpers.UtcNow);
        var options = Options.Create(new InvitationOptions { ExpiryDays = 7 });
        return new InviteToProfileCommandHandler(
            _users, _logins, _profiles, _tokens, _email, _uow, _clock, options);
    }

    private static InviteToProfileCommand Command(
        string roleId,
        string callerAuthId = "auth0|caller",
        string inviteeEmail = "invitee@test.com",
        Guid? siteId = null) =>
        new(siteId ?? Guid.NewGuid(), callerAuthId, "caller@test.com", inviteeEmail, roleId);

    private static IndividualProfile MinorProfile(Guid siteId) => new()
    {
        SiteId         = siteId,
        DateOfBirth    = new DateOnly(2012, 1, 1), // 13 years old on RefDate
        ConsentStateId = NextAtlet.Domain.Enumerations.Individual.ConsentStates.PendingGuardianConsent.Id
    };

    private static IndividualProfile AdultProfile(Guid siteId) => new()
    {
        SiteId         = siteId,
        DateOfBirth    = new DateOnly(2000, 1, 1),
        ConsentStateId = NextAtlet.Domain.Enumerations.Individual.ConsentStates.NotRequired.Id
    };

    // ── Role validation ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_InvalidRole_ReturnsInvitationRoleInvalidError()
    {
        var handler = BuildHandler();
        var cmd     = Command("unknown_role");

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvitationRoleInvalid, result.Error!.Code);
    }

    // ── Caller authorization ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_CallerNotKnown_ThrowsInvalidOperation()
    {
        _users.GetByAuthProviderIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var handler = BuildHandler();
        // An authenticated caller with no User row violates an invariant → throws (not a NotAuthorized result).
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            Command(IndividualRole.Guardian.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CallerHasNoActiveLogin_ReturnsNotAuthorizedError()
    {
        var caller = TestHelpers.AdultUser();
        var siteId = Guid.NewGuid();

        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>())
                 .Returns(MinorProfile(siteId));
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>())
               .Returns((SiteLogin?)null);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            Command(IndividualRole.Guardian.Id, callerAuthId: caller.AuthProviderId!, siteId: siteId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotAuthorized, result.Error!.Code);
    }

    // ── Business rules ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SiteNotFound_ReturnsSiteNotFoundError()
    {
        var caller = TestHelpers.AdultUser();
        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _profiles.GetBySiteIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                 .Returns((IndividualProfile?)null);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            Command(IndividualRole.Guardian.Id, callerAuthId: caller.AuthProviderId!),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.SiteNotFound, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_GuardianRoleOnAdultProfile_ReturnsGuardianCannotRegisterAdultError()
    {
        var caller  = TestHelpers.AdultUser();
        var siteId  = Guid.NewGuid();
        var profile = AdultProfile(siteId);
        var login   = SiteLogin.CreateAthlete(caller.Id, siteId);

        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            Command(IndividualRole.Guardian.Id, callerAuthId: caller.AuthProviderId!, siteId: siteId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.GuardianCannotRegisterAdult, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_AlreadyPendingInvite_ReturnsInvitationAlreadyPendingError()
    {
        var caller  = TestHelpers.AdultUser();
        var siteId  = Guid.NewGuid();
        var profile = MinorProfile(siteId);
        var login   = SiteLogin.CreateAthlete(caller.Id, siteId);

        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);
        _tokens.HasPendingInviteAsync(siteId, "invitee@test.com", IndividualRole.Guardian.Id, Arg.Any<CancellationToken>())
               .Returns(true);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            Command(IndividualRole.Guardian.Id, callerAuthId: caller.AuthProviderId!, siteId: siteId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvitationAlreadyPending, result.Error!.Code);
    }

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidGuardianInviteOnMinorProfile_IssuesTokenAndSendsEmail()
    {
        var caller  = TestHelpers.AdultUser();
        var siteId  = Guid.NewGuid();
        var profile = MinorProfile(siteId);
        var login   = SiteLogin.CreateAthlete(caller.Id, siteId);

        _users.GetByAuthProviderIdAsync(caller.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(caller);
        _profiles.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(profile);
        _logins.GetActiveLoginAsync(caller.Id, siteId, Arg.Any<CancellationToken>()).Returns(login);
        _tokens.HasPendingInviteAsync(siteId, "invitee@test.com", IndividualRole.Guardian.Id, Arg.Any<CancellationToken>())
               .Returns(false);

        var handler = BuildHandler();
        var result  = await handler.Handle(
            Command(IndividualRole.Guardian.Id, callerAuthId: caller.AuthProviderId!, siteId: siteId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _tokens.Received(1).Add(Arg.Any<NextAtlet.Domain.Entities.Identity.ActionToken>());
        await _email.Received(1).SendInviteAsync("invitee@test.com", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
