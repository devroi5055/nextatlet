using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

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

        return site.ControlMode switch
        {
            // controller side — always full; other side observes
            ControlMode.AthleteControlled        => isOwner ? SitePermissions.FullControl : isGuardian ? SitePermissions.ReadOnly : SitePermissions.None,
            ControlMode.GuardianControlled       => isGuardian ? SitePermissions.FullControl : isOwner ? SitePermissions.ReadOnly : SitePermissions.None,

            // shared variants — controller full; other side may edit the draft (+ media), never the senior acts
            ControlMode.AthleteControlledShared  => isOwner ? SitePermissions.FullControl : isGuardian ? SitePermissions.EditOnly : SitePermissions.None,
            ControlMode.GuardianControlledShared => isGuardian ? SitePermissions.FullControl : isOwner ? SitePermissions.EditOnly : SitePermissions.None,

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

        return site.ControlMode switch
        {
            ControlMode.AthleteControlled or ControlMode.AthleteControlledShared => isOwner,
            ControlMode.GuardianControlled or ControlMode.GuardianControlledShared => isGuardian,
            _ => false
        };
    }
}
