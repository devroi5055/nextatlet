using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.AthleteProfile;
using NextAtlet.Domain.Enumerations.AthleteProfile;

namespace NextAtlet.Domain.Authorization;

/// <summary>
/// The single chokepoint for the permission model. Reads <c>ControlMode</c> + role and returns one of
/// three presets — no age-band input, nothing stored per login. All authorization flows through here.
/// </summary>
public class PermissionResolver
{
    public SitePermissions Resolve(ProfileLogin login, AthleteSite site)
    {
        var isOwner = login.RoleId == ProfileRole.AthleteOwner.Id;
        var isGuardian = login.RoleId == ProfileRole.Guardian.Id;

        return site.ControlModeId switch
        {
            "athlete_controlled"         => isOwner    ? SitePermissions.FullControl : isGuardian ? SitePermissions.ReadOnly : SitePermissions.None,
            "guardian_controlled"        => isGuardian ? SitePermissions.FullControl : isOwner    ? SitePermissions.ReadOnly : SitePermissions.None,
            "athlete_controlled_shared"  => isOwner    ? SitePermissions.FullControl : isGuardian ? SitePermissions.EditOnly : SitePermissions.None,
            "guardian_controlled_shared" => isGuardian ? SitePermissions.FullControl : isOwner    ? SitePermissions.EditOnly : SitePermissions.None,
            _ => SitePermissions.None
        };
    }

    /// <summary>
    /// "Is this login the controlling party?" — used by transfer-control + collaboration and by /me.
    /// The Shared variant of a side still belongs to that side's controller.
    /// </summary>
    public bool IsController(ProfileLogin login, AthleteSite site)
    {
        var isOwner = login.RoleId == ProfileRole.AthleteOwner.Id;
        var isGuardian = login.RoleId == ProfileRole.Guardian.Id;

        return site.ControlModeId switch
        {
            "athlete_controlled" or "athlete_controlled_shared"   => isOwner,
            "guardian_controlled" or "guardian_controlled_shared" => isGuardian,
            _ => false
        };
    }
}
