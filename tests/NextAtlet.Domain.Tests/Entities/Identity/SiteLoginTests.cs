using FluentAssertions;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Domain.Tests.Entities;

public class ProfileLoginTests
{
    [Fact]
    public void CreateAthlete_ProducesActiveAthleteOwnerLogin()
    {
        var login = SiteLogin.CreateAthlete(userId: Guid.NewGuid(), siteId: Guid.NewGuid());

        login.SiteRoleId.Should().Be(IndividualRole.Owner.Id);
        login.StatusId.Should().Be(ProfileLoginStatus.Active.Id);
    }

    [Fact]
    public void CreateGuardian_ProducesActiveGuardianLogin()
    {
        var login = SiteLogin.CreateGuardian(userId: Guid.NewGuid(), siteId: Guid.NewGuid());

        login.SiteRoleId.Should().Be(IndividualRole.Guardian.Id);
        login.StatusId.Should().Be(ProfileLoginStatus.Active.Id);
    }

    [Fact]
    public void CreateAthlete_DoesNotStorePermissions()
    {
        var login = SiteLogin.CreateAthlete(Guid.NewGuid(), Guid.NewGuid());

        login.Permissions.Should().BeNull();
    }

    [Fact]
    public void CreateGuardian_DoesNotStorePermissions()
    {
        var login = SiteLogin.CreateGuardian(Guid.NewGuid(), Guid.NewGuid());

        login.Permissions.Should().BeNull();
    }

    [Fact]
    public void CreateAthlete_LinksTheGivenUserAndSite()
    {
        var userId = Guid.NewGuid();
        var siteId = Guid.NewGuid();

        var login = SiteLogin.CreateAthlete(userId, siteId);

        login.UserId.Should().Be(userId);
        login.SiteId.Should().Be(siteId);
    }
}
