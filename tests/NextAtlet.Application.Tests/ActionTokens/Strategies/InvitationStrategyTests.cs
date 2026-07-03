using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Tests.Shared.TestData;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests.ActionTokens.Strategies;

/// <summary>
/// The Invitation action-token strategy: on accept it grants the invited role by creating an active
/// SiteLogin for the authenticated invitee, after validating the role is legal for the site type.
/// </summary>
public class InvitationStrategyTests
{
    private readonly ISiteRepository _sites = Substitute.For<ISiteRepository>();
    private readonly ISiteLoginRepository _logins = Substitute.For<ISiteLoginRepository>();

    private InvitationStrategy BuildStrategy() => new(_sites, _logins);

    private static ActionToken InviteToken(Guid siteId, string roleId) =>
        ActionToken.Issue(
            ActionTokenType.Invitation.Id, siteId,
            new InvitePayload { Email = "invitee@test.local", RoleId = roleId },
            DateTime.UtcNow.AddDays(7));

    private static Site IndividualSite() => new()
    {
        Slug = "kid",
        DisplayName = "Kid",
        SiteTypeId = SiteType.Individual.Id
    };

    [Fact]
    public void Metadata_IsInvitation_AndAuthRequired()
    {
        var strategy = BuildStrategy();
        Assert.Equal(ActionTokenType.Invitation, strategy.ActionTokenType);
        Assert.True(strategy.authRequired);
    }

    [Fact]
    public async Task ExecuteAsync_NullActor_Throws()
    {
        var strategy = BuildStrategy();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            strategy.ExecuteAsync(InviteToken(Guid.NewGuid(), IndividualRole.Guardian.Id), null, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_TargetSiteMissing_Throws()
    {
        var siteId = Guid.NewGuid();
        _sites.GetByIdAsync(siteId, Arg.Any<CancellationToken>()).Returns((Site?)null);

        var strategy = BuildStrategy();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            strategy.ExecuteAsync(InviteToken(siteId, IndividualRole.Guardian.Id), Users.AnAuthenticatedUser(), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_UnknownRole_Throws()
    {
        var siteId = Guid.NewGuid();
        _sites.GetByIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(IndividualSite());

        var strategy = BuildStrategy();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            strategy.ExecuteAsync(InviteToken(siteId, "not_a_real_role"), Users.AnAuthenticatedUser(), CancellationToken.None));

        _logins.DidNotReceive().Add(Arg.Any<SiteLogin>());
    }

    [Fact]
    public async Task ExecuteAsync_ValidGuardianInvite_AddsActiveSiteLogin()
    {
        var siteId = Guid.NewGuid();
        var invitee = Users.AnAuthenticatedUser();
        var site = IndividualSite();
        _sites.GetByIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(site);

        var result = await BuildStrategy().ExecuteAsync(
            InviteToken(siteId, IndividualRole.Guardian.Id), invitee, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _logins.Received(1).Add(Arg.Is<SiteLogin>(l =>
            l.UserId == invitee.Id &&
            l.SiteId == site.Id &&
            l.SiteRoleId == IndividualRole.Guardian.Id &&
            l.StatusId == ProfileLoginStatus.Active.Id));
    }
}
