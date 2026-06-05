using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Every (role × ControlMode) cell of the permission model. No age-band input; nothing stored per login.
/// </summary>
public class PermissionResolverTests
{
    private static readonly PermissionResolver Resolver = new();

    private static ProfileLogin Login(string roleId) => new()
    {
        UserId = Guid.NewGuid(),
        AthleteProfileId = Guid.NewGuid(),
        RoleId = roleId,
        Status = ProfileLoginStatus.Active
    };

    private static AthleteProfile Profile(ControlMode mode) => new()
    {
        Slug = "x",
        DisplayName = "X",
        DateOfBirth = new DateOnly(2010, 1, 1),
        ControlMode = mode
    };

    private static ProfileLogin Owner => Login(ProfileRole.AthleteOwner.Id);
    private static ProfileLogin Guardian => Login(ProfileRole.Guardian.Id);

    [Theory]
    // controller side — FullControl
    [InlineData(ControlMode.AthleteControlled, true)]
    [InlineData(ControlMode.AthleteControlledShared, true)]
    // non-controller, non-shared — ReadOnly; shared — EditOnly (neither is FullControl)
    [InlineData(ControlMode.GuardianControlled, false)]
    [InlineData(ControlMode.GuardianControlledShared, false)]
    public void Owner_full_control_only_when_athlete_controls(ControlMode mode, bool expectFull)
        => Assert.Equal(expectFull, Resolver.Resolve(Owner, Profile(mode)) == ProfilePermissions.FullControl);

    [Theory]
    [InlineData(ControlMode.GuardianControlled, true)]
    [InlineData(ControlMode.GuardianControlledShared, true)]
    [InlineData(ControlMode.AthleteControlled, false)]
    [InlineData(ControlMode.AthleteControlledShared, false)]
    public void Guardian_full_control_only_when_guardian_controls(ControlMode mode, bool expectFull)
        => Assert.Equal(expectFull, Resolver.Resolve(Guardian, Profile(mode)) == ProfilePermissions.FullControl);

    [Fact]
    public void Controller_gets_full_control_each_mode()
    {
        Assert.Equal(ProfilePermissions.FullControl, Resolver.Resolve(Owner, Profile(ControlMode.AthleteControlled)));
        Assert.Equal(ProfilePermissions.FullControl, Resolver.Resolve(Owner, Profile(ControlMode.AthleteControlledShared)));
        Assert.Equal(ProfilePermissions.FullControl, Resolver.Resolve(Guardian, Profile(ControlMode.GuardianControlled)));
        Assert.Equal(ProfilePermissions.FullControl, Resolver.Resolve(Guardian, Profile(ControlMode.GuardianControlledShared)));
    }

    [Fact]
    public void Non_controller_is_readonly_in_non_shared_modes()
    {
        Assert.Equal(ProfilePermissions.ReadOnly, Resolver.Resolve(Guardian, Profile(ControlMode.AthleteControlled)));
        Assert.Equal(ProfilePermissions.ReadOnly, Resolver.Resolve(Owner, Profile(ControlMode.GuardianControlled)));
    }

    [Fact]
    public void Non_controller_is_editonly_in_shared_modes()
    {
        var guardianShared = Resolver.Resolve(Guardian, Profile(ControlMode.AthleteControlledShared));
        var athleteShared = Resolver.Resolve(Owner, Profile(ControlMode.GuardianControlledShared));

        Assert.Equal(ProfilePermissions.EditOnly, guardianShared);
        Assert.Equal(ProfilePermissions.EditOnly, athleteShared);

        // EditOnly = collaborate on the draft (+ media) but never the senior acts.
        Assert.True(guardianShared.CanEditContent);
        Assert.True(guardianShared.CanManageMedia);
        Assert.False(guardianShared.CanPublish);
        Assert.False(guardianShared.CanApproveChanges);
        Assert.False(guardianShared.CanManageMemberships);
    }

    [Fact]
    public void Unknown_role_resolves_to_none()
        => Assert.Equal(ProfilePermissions.None, Resolver.Resolve(Login("spectator"), Profile(ControlMode.AthleteControlled)));

    [Theory]
    [InlineData(ControlMode.AthleteControlled, true, false)]
    [InlineData(ControlMode.AthleteControlledShared, true, false)]
    [InlineData(ControlMode.GuardianControlled, false, true)]
    [InlineData(ControlMode.GuardianControlledShared, false, true)]
    public void IsController_tracks_the_controlling_side(ControlMode mode, bool ownerControls, bool guardianControls)
    {
        Assert.Equal(ownerControls, Resolver.IsController(Owner, Profile(mode)));
        Assert.Equal(guardianControls, Resolver.IsController(Guardian, Profile(mode)));
    }
}
