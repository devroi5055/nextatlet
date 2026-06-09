using FluentAssertions;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using System.Globalization;
using Xunit;

namespace NextAtlet.Domain.Tests.Entities;

public class ProfileLoginTests
{
    [Fact]
    public void CreateOwner_ProducesActiveAthleteOwnerLogin()
    {
        var login = ProfileLogin.CreateOwner(userId: Guid.NewGuid(), profileId: Guid.NewGuid());

        login.RoleId.Should().Be(ProfileRole.AthleteOwner.Id);
        login.Status.Should().Be(ProfileLoginStatus.Active);
    }

    [Fact]
    public void CreateGuardian_ProducesActiveGuardianLogin()
    {
        var login = ProfileLogin.CreateGuardian(userId: Guid.NewGuid(), profileId: Guid.NewGuid());

        login.RoleId.Should().Be(ProfileRole.Guardian.Id);
        login.Status.Should().Be(ProfileLoginStatus.Active);
    }

    [Fact]
    public void CreateOwner_DoesNotStorePermissions()
    {
        var login = ProfileLogin.CreateOwner(Guid.NewGuid(), Guid.NewGuid());

        login.Permissions.Should().BeNull();
    }

    [Fact]
    public void CreateGuardian_DoesNotStorePermissions()
    {
        var login = ProfileLogin.CreateGuardian(Guid.NewGuid(), Guid.NewGuid());

        login.Permissions.Should().BeNull();
    }

    [Fact]
    public void CreateOwner_LinksTheGivenUserAndProfile()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        var login = ProfileLogin.CreateOwner(userId, profileId);

        login.UserId.Should().Be(userId);
        login.AthleteProfileId.Should().Be(profileId);
    }
}
    