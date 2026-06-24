using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests;

public class GetCurrentUserQueryHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly IUserRepository              _users    = Substitute.For<IUserRepository>();
    private readonly IIndividualProfileRepository _profiles = Substitute.For<IIndividualProfileRepository>();
    private readonly ISiteRepository              _sites    = Substitute.For<ISiteRepository>();
    private readonly ISiteLoginRepository         _logins   = Substitute.For<ISiteLoginRepository>();
    private readonly IActionTokenRepository       _tokens   = Substitute.For<IActionTokenRepository>();

    private GetCurrentUserQueryHandler BuildHandler() =>
        new(_users, _profiles, _sites, _logins, _tokens, new PermissionResolver());

    private static GetCurrentUserQuery Query(string authId = "auth0|user", string email = "user@test.com") =>
        new(authId, email);

    private static IndividualProfile MinorProfile(Guid siteId) => new()
    {
        SiteId         = siteId,
        DateOfBirth    = new DateOnly(2012, 1, 1),
        ConsentStateId = ConsentStates.PendingGuardianConsent.Id,
        ControlModeId  = ControlModes.AthleteControlled.Id
    };

    private static IndividualProfile AdultProfile(Guid siteId) => new()
    {
        SiteId         = siteId,
        DateOfBirth    = new DateOnly(2000, 1, 1),
        ConsentStateId = ConsentStates.NotRequired.Id,
        ControlModeId  = ControlModes.AthleteControlled.Id
    };

    // ── No User row yet ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoUserNoPendingInvites_ReturnsUnregisteredNullRole()
    {
        _users.GetByAuthProviderIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);
        _tokens.CountPendingInvitesByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(0);

        var result = await BuildHandler().Handle(Query(), CancellationToken.None);

        Assert.False(result.Registered);
        Assert.Null(result.Role);
        Assert.Equal(0, result.PendingGuardianInvites);
    }

    [Fact]
    public async Task Handle_NoUserButPendingInvites_ReturnsGuardianRole()
    {
        _users.GetByAuthProviderIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);
        _tokens.CountPendingInvitesByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(2);

        var result = await BuildHandler().Handle(Query(), CancellationToken.None);

        Assert.False(result.Registered);
        Assert.Equal(IndividualRole.Guardian.Id, result.Role);
        Assert.Equal(2, result.PendingGuardianInvites);
    }

    // ── User exists but no owned site ─────────────────────────────────────────

    [Fact]
    public async Task Handle_UserExistsNoOwnedSite_NoGuardedSites_ReturnsUnregistered()
    {
        var user = TestHelpers.AdultUser();
        _users.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(user);
        _tokens.CountPendingInvitesByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(0);
        _sites.GetOwnedByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
              .Returns((Site?)null);
        _logins.GetActiveGuardianSiteIdsByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
               .Returns(new List<Guid>());

        var result = await BuildHandler().Handle(Query(user.AuthProviderId!), CancellationToken.None);

        Assert.False(result.Registered);
        Assert.Null(result.Role);
        Assert.Empty(result.GuardedProfileIds);
    }

    [Fact]
    public async Task Handle_UserExistsWithGuardedSites_ReturnsGuardianRole()
    {
        var user    = TestHelpers.AdultUser();
        var childId = Guid.NewGuid();
        _users.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(user);
        _tokens.CountPendingInvitesByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(0);
        _sites.GetOwnedByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
              .Returns((Site?)null);
        _logins.GetActiveGuardianSiteIdsByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
               .Returns(new List<Guid> { childId });

        var result = await BuildHandler().Handle(Query(user.AuthProviderId!), CancellationToken.None);

        Assert.False(result.Registered);
        Assert.Equal(IndividualRole.Guardian.Id, result.Role);
        Assert.Contains(childId, result.GuardedProfileIds);
    }

    // ── User exists with owned site ───────────────────────────────────────────

    [Fact]
    public async Task Handle_UserWithOwnedSiteButMissingProfile_ThrowsDomainException()
    {
        var user = TestHelpers.AdultUser();
        var site = TestHelpers.IndividualSite();
        _users.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(user);
        _tokens.CountPendingInvitesByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(0);
        _sites.GetOwnedByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
              .Returns(site);
        _logins.GetActiveGuardianSiteIdsByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
               .Returns(new List<Guid>());
        _profiles.GetBySiteIdAsync(site.Id, Arg.Any<CancellationToken>())
                 .Returns((IndividualProfile?)null);

        await Assert.ThrowsAsync<DomainException>(() =>
            BuildHandler().Handle(Query(user.AuthProviderId!), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RegisteredOwner_AthleteControlled_IsInControlAndCanEdit()
    {
        var user    = TestHelpers.AdultUser();
        var site    = TestHelpers.IndividualSite();
        var profile = AdultProfile(site.Id);
        var login   = SiteLogin.CreateAthlete(user.Id, site.Id);

        _users.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(user);
        _tokens.CountPendingInvitesByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(0);
        _sites.GetOwnedByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
              .Returns(site);
        _logins.GetActiveGuardianSiteIdsByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
               .Returns(new List<Guid>());
        _profiles.GetBySiteIdAsync(site.Id, Arg.Any<CancellationToken>())
                 .Returns(profile);
        _logins.GetActiveLoginAsync(user.Id, site.Id, Arg.Any<CancellationToken>())
               .Returns(login);

        var result = await BuildHandler().Handle(Query(user.AuthProviderId!), CancellationToken.None);

        Assert.True(result.Registered);
        Assert.Equal(IndividualRole.Owner.Id, result.Role);
        Assert.Equal(profile.Id, result.ProfileId);
        Assert.True(result.IsInControl);
        Assert.True(result.CanEdit);
    }

    [Fact]
    public async Task Handle_RegisteredOwner_GuardianControlled_NotInControlCannotEdit()
    {
        var user    = TestHelpers.AdultUser();
        var site    = TestHelpers.IndividualSite();
        var profile = new IndividualProfile
        {
            SiteId         = site.Id,
            DateOfBirth    = new DateOnly(2000, 1, 1),
            ConsentStateId = ConsentStates.NotRequired.Id,
            ControlModeId  = ControlModes.GuardianControlled.Id
        };
        var ownerLogin = SiteLogin.CreateAthlete(user.Id, site.Id);

        _users.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>())
              .Returns(user);
        _tokens.CountPendingInvitesByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(0);
        _sites.GetOwnedByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
              .Returns(site);
        _logins.GetActiveGuardianSiteIdsByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
               .Returns(new List<Guid>());
        _profiles.GetBySiteIdAsync(site.Id, Arg.Any<CancellationToken>())
                 .Returns(profile);
        _logins.GetActiveLoginAsync(user.Id, site.Id, Arg.Any<CancellationToken>())
               .Returns(ownerLogin);

        var result = await BuildHandler().Handle(Query(user.AuthProviderId!), CancellationToken.None);

        Assert.True(result.Registered);
        Assert.False(result.IsInControl); // guardian has control
        Assert.False(result.CanEdit);     // guardian controlled, not shared
    }
}
