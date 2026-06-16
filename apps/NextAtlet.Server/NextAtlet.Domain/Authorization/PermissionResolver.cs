using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;

namespace NextAtlet.Domain.Authorization;

/// <summary>
/// The single chokepoint for the permission model. Reads <c>ControlMode</c> + role and returns one of
/// three presets — no age-band input, nothing stored per login. All authorization flows through here.
/// </summary>
public class PermissionResolver
{
    public SitePermissions Resolve(SiteLogin login, AthleteProfile profile)
    {
        var isOwner = login.SiteRoleId == ProfileRoles.AthleteOwner.Id;
        var isGuardian = login.SiteRoleId == ProfileRoles.Guardian.Id;

        return profile.ControlModeId switch
        {
            var id when id == ControlModes.AthleteControlled.Id => isOwner ? SitePermissions.FullControl : isGuardian ? SitePermissions.ReadOnly : SitePermissions.None,
            var id when id == ControlModes.GuardianControlled.Id => isGuardian ? SitePermissions.FullControl : isOwner ? SitePermissions.ReadOnly : SitePermissions.None,
            var id when id == ControlModes.AthleteControlledShared.Id => isOwner ? SitePermissions.FullControl : isGuardian ? SitePermissions.EditOnly : SitePermissions.None,
            var id when id == ControlModes.GuardianControlledShared.Id => isGuardian ? SitePermissions.FullControl : isOwner ? SitePermissions.EditOnly : SitePermissions.None,
            _ => SitePermissions.None
        };
    }

    /// <summary>
    /// "Is this login the controlling party?" — used by transfer-control + collaboration and by /me.
    /// The Shared variant of a side still belongs to that side's controller.
    /// </summary>
    public bool IsController(SiteLogin login, AthleteProfile profile)
    {
        var isOwner = login.SiteRoleId == ProfileRoles.AthleteOwner.Id;
        var isGuardian = login.SiteRoleId == ProfileRoles.Guardian.Id;

        return profile.ControlModeId switch
        {
            var id when id == ControlModes.AthleteControlled.Id || id == ControlModes.AthleteControlledShared.Id => isOwner,
            var id when id == ControlModes.GuardianControlled.Id || id == ControlModes.GuardianControlledShared.Id => isGuardian,
            _ => false
        };
    }
}
