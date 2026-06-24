using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Application.Tests.Shared.TestData;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Organization;

namespace NextAtlet.Application.Tests.ActionTokens.Strategies;

/// <summary>
/// The org-email-verification strategy: on accept it flips the organization to Verified and records an
/// OrgVerification audit (method = email, the registry address it was sent to, optional logged-in user).
/// Auth is NOT required — the emailed link itself is the authority.
/// </summary>
public class OrgEmailVerificationStrategyTests
{
    private readonly IOrganizationProfileRepository _orgs = Substitute.For<IOrganizationProfileRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();

    private static readonly DateTime Now = new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    private OrgEmailVerificationStrategy BuildStrategy()
    {
        _clock.UtcNow.Returns(Now);
        return new OrgEmailVerificationStrategy(_orgs, new UserProvisioner(_users, _clock), _clock);
    }

    private static ActionToken VerificationToken(Guid siteId, string email = "chair@club.dk") =>
        ActionToken.Issue(
            ActionTokenType.OrgEmailVerification.Id, siteId,
            new OrgEmailVerificationPayload { ClubOfficialId = Guid.NewGuid(), UserId = null, Email = email },
            DateTime.UtcNow.AddDays(7));

    private static OrganizationProfile OrgProfile(Guid siteId) => new()
    {
        SiteId = siteId,
        OrganizationTypeId = OrganizationType.Club.Id
    };

    [Fact]
    public void Metadata_IsOrgEmailVerification_AndAuthNotRequired()
    {
        var strategy = BuildStrategy();
        Assert.Equal(ActionTokenType.OrgEmailVerification, strategy.ActionTokenType);
        Assert.False(strategy.authRequired);
    }

    [Fact]
    public async Task ExecuteAsync_OrganizationNotFound_ReturnsOrganizationProfileNotFound()
    {
        var siteId = Guid.NewGuid();
        _orgs.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns((OrganizationProfile?)null);

        var result = await BuildStrategy().ExecuteAsync(VerificationToken(siteId), null, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.OrganizationProfileNotFound, result.Error!.Code);
    }

    [Fact]
    public async Task ExecuteAsync_AnonymousAccept_VerifiesOrgAndRecordsEmailAudit()
    {
        var siteId = Guid.NewGuid();
        var org = OrgProfile(siteId);
        _orgs.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(org);

        var result = await BuildStrategy().ExecuteAsync(
            VerificationToken(siteId, "chair@club.dk"), actorUser: null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(VerificationStatus.Verified.Id, org.VerificationStatusId);
        Assert.NotNull(org.Verification);
        Assert.Null(org.Verification!.VerifiedByUserId);                 // no logged-in user
        Assert.Equal("chair@club.dk", org.Verification.VerifiedByEmail); // the registry address
        Assert.Equal(VerificationMethod.Email.Id, org.Verification.MethodId);
        Assert.Equal(Now, org.Verification.VerifiedUtc);
    }

    [Fact]
    public async Task ExecuteAsync_AuthenticatedAccept_StampsVerifyingUser()
    {
        var siteId = Guid.NewGuid();
        var org = OrgProfile(siteId);
        var actor = Users.AnAuthenticatedUser();
        _orgs.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(org);

        var result = await BuildStrategy().ExecuteAsync(VerificationToken(siteId), actor, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(actor.Id, org.Verification!.VerifiedByUserId);
    }
}
