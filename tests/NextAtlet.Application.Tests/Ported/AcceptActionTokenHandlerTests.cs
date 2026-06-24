using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.ActionTokens.Commands;
using NextAtlet.Application.Features.ActionTokens.Strategies;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests;

public class AcceptActionTokenHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────

    private readonly IActionTokenRepository _tokens = Substitute.For<IActionTokenRepository>();
    private readonly IUserRepository        _users  = Substitute.For<IUserRepository>();
    private readonly IClock                 _clock  = Substitute.For<IClock>();

    private AcceptActionTokenCommandHandler BuildHandler(params IActionTokenStrategy[] strategies)
    {
        _clock.UtcNow.Returns(TestHelpers.UtcNow);
        var provisioner = new UserProvisioner(_users, _clock);
        var registry    = new ActionTokenStrategyRegistry(strategies);
        return new AcceptActionTokenCommandHandler(_tokens, registry, provisioner, _clock);
    }

    private IActionTokenStrategy StubStrategy(
        ActionTokenType type,
        bool authRequired,
        Result executeResult)
    {
        var s = Substitute.For<IActionTokenStrategy>();
        s.ActionTokenType.Returns(type);
        s.authRequired.Returns(authRequired);
        s.ExecuteAsync(Arg.Any<ActionToken>(), Arg.Any<User?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(executeResult));
        return s;
    }

    // Expiry is relative to the real clock (ActionToken.IsExpired uses DateTime.UtcNow directly).
    private static ActionToken PendingConsentToken() =>
        ActionToken.Issue(
            ActionTokenType.Consent.Id,
            Guid.NewGuid(),
            new ConsentPayload { Email = "guardian@test.com", TermsVersion = "2026-01" },
            DateTime.UtcNow.AddDays(7));

    private static ActionToken PendingInviteToken() =>
        ActionToken.Issue(
            ActionTokenType.Invitation.Id,
            Guid.NewGuid(),
            new InvitePayload { Email = "invitee@test.com", RoleId = IndividualRole.Guardian.Id },
            DateTime.UtcNow.AddDays(7));

    // ── Token validation ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsNotFoundError()
    {
        var handler = BuildHandler(StubStrategy(ActionTokenType.Consent, true, Result.Success()));
        _tokens.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ActionToken?)null);

        var result = await handler.Handle(
            new AcceptActionTokenCommand(Guid.NewGuid(), "auth0|any"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ActionTokenNotFound, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_TokenExpired_ReturnsExpiredError()
    {
        var expiredToken = ActionToken.Issue(
            ActionTokenType.Consent.Id, Guid.NewGuid(),
            new ConsentPayload { Email = "g@t.com", TermsVersion = "2026-01" },
            DateTime.UtcNow.AddSeconds(-1)); // already past

        var tokenId = Guid.NewGuid();
        _tokens.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns(expiredToken);

        var handler = BuildHandler(StubStrategy(ActionTokenType.Consent, true, Result.Success()));
        var result  = await handler.Handle(
            new AcceptActionTokenCommand(tokenId, "auth0|any"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ActionTokenExpired, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_TokenAlreadyUsed_ReturnsAlreadyUsedError()
    {
        var usedToken = PendingConsentToken();
        usedToken.Accept(DateTime.UtcNow);

        var tokenId = Guid.NewGuid();
        _tokens.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns(usedToken);

        var handler = BuildHandler(StubStrategy(ActionTokenType.Consent, true, Result.Success()));
        var result  = await handler.Handle(
            new AcceptActionTokenCommand(tokenId, "auth0|any"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ActionTokenAlreadyUsed, result.Error!.Code);
    }

    // ── Auth validation ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AuthRequiredButNoProviderId_ReturnsNotAuthorized()
    {
        var token   = PendingConsentToken();
        var tokenId = Guid.NewGuid();
        _tokens.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns(token);

        var handler = BuildHandler(StubStrategy(ActionTokenType.Consent, authRequired: true, Result.Success()));

        // authProviderId = null simulates an anonymous caller
        var result = await handler.Handle(
            new AcceptActionTokenCommand(tokenId, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotAuthorized, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_AuthRequiredButNoUserRow_ThrowsInvalidOperationException()
    {
        // An authenticated sub that has no User in the DB violates the provisioner invariant.
        var token   = PendingConsentToken();
        var tokenId = Guid.NewGuid();
        _tokens.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns(token);
        _users.GetByAuthProviderIdAsync("auth0|ghost", Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var handler = BuildHandler(StubStrategy(ActionTokenType.Consent, authRequired: true, Result.Success()));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new AcceptActionTokenCommand(tokenId, "auth0|ghost"), CancellationToken.None));
    }

    // ── Happy paths ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidConsentToken_AcceptsTokenAndReturnsSuccess()
    {
        var token   = PendingConsentToken();
        var tokenId = Guid.NewGuid();
        var user    = TestHelpers.AdultUser();

        _tokens.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns(token);
        _users.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>()).Returns(user);

        var handler = BuildHandler(StubStrategy(ActionTokenType.Consent, authRequired: true, Result.Success()));
        var result  = await handler.Handle(
            new AcceptActionTokenCommand(tokenId, user.AuthProviderId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(token.AcceptedUtc); // handler stamps the token
    }

    [Fact]
    public async Task Handle_ValidInvitationToken_AcceptsTokenAndReturnsSuccess()
    {
        var token   = PendingInviteToken();
        var tokenId = Guid.NewGuid();
        var user    = TestHelpers.AdultUser();

        _tokens.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns(token);
        _users.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>()).Returns(user);

        var handler = BuildHandler(StubStrategy(ActionTokenType.Invitation, authRequired: true, Result.Success()));
        var result  = await handler.Handle(
            new AcceptActionTokenCommand(tokenId, user.AuthProviderId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(token.AcceptedUtc);
    }

    [Fact]
    public async Task Handle_OrgVerificationToken_NoAuthRequired_AcceptsWithoutUser()
    {
        var token = ActionToken.Issue(
            ActionTokenType.OrgEmailVerification.Id, Guid.NewGuid(),
            new OrgEmailVerificationPayload
            {
                ClubOfficialId = Guid.NewGuid(),
                UserId         = null,
                Email          = "official@club.dk"
            },
            DateTime.UtcNow.AddDays(7));

        var tokenId = Guid.NewGuid();
        _tokens.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns(token);

        var handler = BuildHandler(
            StubStrategy(ActionTokenType.OrgEmailVerification, authRequired: false, Result.Success()));

        var result = await handler.Handle(
            new AcceptActionTokenCommand(tokenId, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(token.AcceptedUtc);
    }

    [Fact]
    public async Task Handle_StrategyReturnsFailure_PropagatesFailureWithoutAccepting()
    {
        var token   = PendingConsentToken();
        var tokenId = Guid.NewGuid();
        var user    = TestHelpers.AdultUser();

        _tokens.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns(token);
        _users.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>()).Returns(user);

        var failure = Error.FromCode(ErrorCodes.ConsentNotNeeded);
        var handler = BuildHandler(
            StubStrategy(ActionTokenType.Consent, authRequired: true, Result.Failure(failure)));

        var result = await handler.Handle(
            new AcceptActionTokenCommand(tokenId, user.AuthProviderId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ConsentNotNeeded, result.Error!.Code);
        Assert.Null(token.AcceptedUtc); // token was NOT stamped on failure
    }
}
