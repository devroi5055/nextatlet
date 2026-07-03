using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Domain.Tests;

public class PermissionResolverTests
{
    private readonly PermissionResolver _resolver = new();

    private static IndividualProfile ProfileWith(string controlModeId) => new()
    {
        SiteId         = Guid.NewGuid(),
        DateOfBirth    = new DateOnly(2000, 1, 1),
        ConsentStateId = ConsentStates.NotRequired.Id,
        ControlModeId  = controlModeId
    };

    private static SiteLogin OwnerLogin()    => SiteLogin.CreateAthlete(Guid.NewGuid(), Guid.NewGuid());
    private static SiteLogin GuardianLogin() => SiteLogin.CreateGuardian(Guid.NewGuid(), Guid.NewGuid());

    // ── AthleteControlled ────────────────────────────────────────────────────

    [Fact]
    public void AthleteControlled_Owner_GetsFullControl()
    {
        var p = ProfileWith(ControlModes.AthleteControlled.Id);
        Assert.Equal(SitePermissions.FullControl, _resolver.Resolve(OwnerLogin(), p));
    }

    [Fact]
    public void AthleteControlled_Guardian_GetsReadOnly()
    {
        var p = ProfileWith(ControlModes.AthleteControlled.Id);
        Assert.Equal(SitePermissions.ReadOnly, _resolver.Resolve(GuardianLogin(), p));
    }

    // ── GuardianControlled ───────────────────────────────────────────────────

    [Fact]
    public void GuardianControlled_Guardian_GetsFullControl()
    {
        var p = ProfileWith(ControlModes.GuardianControlled.Id);
        Assert.Equal(SitePermissions.FullControl, _resolver.Resolve(GuardianLogin(), p));
    }

    [Fact]
    public void GuardianControlled_Owner_GetsReadOnly()
    {
        var p = ProfileWith(ControlModes.GuardianControlled.Id);
        Assert.Equal(SitePermissions.ReadOnly, _resolver.Resolve(OwnerLogin(), p));
    }

    // ── AthleteControlledShared ──────────────────────────────────────────────

    [Fact]
    public void AthleteControlledShared_Owner_GetsFullControl()
    {
        var p = ProfileWith(ControlModes.AthleteControlledShared.Id);
        Assert.Equal(SitePermissions.FullControl, _resolver.Resolve(OwnerLogin(), p));
    }

    [Fact]
    public void AthleteControlledShared_Guardian_GetsEditOnly()
    {
        var p = ProfileWith(ControlModes.AthleteControlledShared.Id);
        Assert.Equal(SitePermissions.EditOnly, _resolver.Resolve(GuardianLogin(), p));
    }

    // ── GuardianControlledShared ─────────────────────────────────────────────

    [Fact]
    public void GuardianControlledShared_Guardian_GetsFullControl()
    {
        var p = ProfileWith(ControlModes.GuardianControlledShared.Id);
        Assert.Equal(SitePermissions.FullControl, _resolver.Resolve(GuardianLogin(), p));
    }

    [Fact]
    public void GuardianControlledShared_Owner_GetsEditOnly()
    {
        var p = ProfileWith(ControlModes.GuardianControlledShared.Id);
        Assert.Equal(SitePermissions.EditOnly, _resolver.Resolve(OwnerLogin(), p));
    }

    // ── Unknown mode ─────────────────────────────────────────────────────────

    [Fact]
    public void UnknownControlMode_ReturnsNone()
    {
        var p = ProfileWith("unknown_mode");
        Assert.Equal(SitePermissions.None, _resolver.Resolve(OwnerLogin(), p));
    }

    // ── IsController ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("athlete_controlled")]
    [InlineData("athlete_controlled_shared")]
    public void IsController_Owner_WhenAthleteControlled(string mode)
    {
        var p = ProfileWith(mode);
        Assert.True(_resolver.IsController(OwnerLogin(), p));
        Assert.False(_resolver.IsController(GuardianLogin(), p));
    }

    [Theory]
    [InlineData("guardian_controlled")]
    [InlineData("guardian_controlled_shared")]
    public void IsController_Guardian_WhenGuardianControlled(string mode)
    {
        var p = ProfileWith(mode);
        Assert.True(_resolver.IsController(GuardianLogin(), p));
        Assert.False(_resolver.IsController(OwnerLogin(), p));
    }

    // ── SitePermissions flags ─────────────────────────────────────────────────

    [Fact]
    public void EditOnly_CanEdit_CannotPublish()
    {
        Assert.True(SitePermissions.EditOnly.CanEditContent);
        Assert.False(SitePermissions.EditOnly.CanPublish);
        Assert.False(SitePermissions.EditOnly.CanApproveChanges);
    }

    [Fact]
    public void FullControl_AllFlagsTrue()
    {
        Assert.True(SitePermissions.FullControl.CanEditContent);
        Assert.True(SitePermissions.FullControl.CanPublish);
        Assert.True(SitePermissions.FullControl.CanApproveChanges);
        Assert.True(SitePermissions.FullControl.CanManageMedia);
        Assert.True(SitePermissions.FullControl.CanManageMemberships);
    }
}
