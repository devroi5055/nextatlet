using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Application.Tests.Shared.TestData;
using NextAtlet.Domain.Entities.Consent;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests.ActionTokens.Strategies;

/// <summary>
/// The Consent action-token strategy: on accept it writes the GuardianConsent audit row (the four GDPR
/// facts) — unless consent already exists, in which case it's a no-op failure. Auth is required.
/// </summary>
public class ConsentStrategyTests
{
    private readonly IGuardianConsentRepository _consents = Substitute.For<IGuardianConsentRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();

    private ConsentStrategy BuildStrategy()
    {
        _clock.UtcNow.Returns(new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        return new ConsentStrategy(new UserProvisioner(_users, _clock), _clock, _consents);
    }

    private static ActionToken ConsentToken(Guid siteId, string termsVersion = "2026-01") =>
        ActionToken.Issue(
            ActionTokenType.Consent.Id, siteId,
            new ConsentPayload { Email = "guardian@test.local", TermsVersion = termsVersion },
            DateTime.UtcNow.AddDays(7));

    [Fact]
    public void Metadata_IsConsent_AndAuthRequired()
    {
        var strategy = BuildStrategy();
        Assert.Equal(ActionTokenType.Consent, strategy.ActionTokenType);
        Assert.True(strategy.authRequired);
    }

    [Fact]
    public async Task ExecuteAsync_NullActor_Throws()
    {
        var strategy = BuildStrategy();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            strategy.ExecuteAsync(ConsentToken(Guid.NewGuid()), null, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_ConsentAlreadyExists_ReturnsConsentNotNeeded_AndRecordsNothing()
    {
        var siteId = Guid.NewGuid();
        var strategy = BuildStrategy();
        _consents.ExistsForProfileAsync(siteId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await strategy.ExecuteAsync(
            ConsentToken(siteId), Users.AnAuthenticatedUser(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ConsentNotNeeded, result.Error!.Code);
        _consents.DidNotReceive().Add(Arg.Any<GuardianConsent>());
    }

    [Fact]
    public async Task ExecuteAsync_PendingConsent_RecordsGuardianConsentWithFourGdprFacts()
    {
        var siteId = Guid.NewGuid();
        var guardian = Users.AnAuthenticatedUser();
        var strategy = BuildStrategy();
        _consents.ExistsForProfileAsync(siteId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await strategy.ExecuteAsync(
            ConsentToken(siteId, "2026-01"), guardian, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _consents.Received(1).Add(Arg.Is<GuardianConsent>(c =>
            c.SiteId == siteId &&                       // which profile
            c.GuardianUserId == guardian.Id &&          // who
            c.MethodId == ConsentMethods.Email.Id &&    // how
            c.TermsVersion == "2026-01"));              // what
    }
}
