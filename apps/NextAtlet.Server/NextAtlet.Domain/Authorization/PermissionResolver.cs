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
    public ProfilePermissions Resolve(ProfileLogin login, AthleteProfile profile)
    {
        var isOwner = login.RoleId == ProfileRole.AthleteOwner.Id;
        var isGuardian = login.RoleId == ProfileRole.Guardian.Id;

        return profile.ControlMode switch
        {
            // controller side — always full; other side observes
            ControlMode.AthleteControlled        => isOwner ? ProfilePermissions.FullControl : isGuardian ? ProfilePermissions.ReadOnly : ProfilePermissions.None,
            ControlMode.GuardianControlled       => isGuardian ? ProfilePermissions.FullControl : isOwner ? ProfilePermissions.ReadOnly : ProfilePermissions.None,

            // shared variants — controller full; other side may edit the draft (+ media), never the senior acts
            ControlMode.AthleteControlledShared  => isOwner ? ProfilePermissions.FullControl : isGuardian ? ProfilePermissions.EditOnly : ProfilePermissions.None,
            ControlMode.GuardianControlledShared => isGuardian ? ProfilePermissions.FullControl : isOwner ? ProfilePermissions.EditOnly : ProfilePermissions.None,

            _ => ProfilePermissions.None
        };
    }

    /// <summary>
    /// "Is this login the controlling party?" — used by transfer-control + collaboration and by /me.
    /// The Shared variant of a side still belongs to that side's controller.
    /// </summary>
    public bool IsController(ProfileLogin login, AthleteProfile profile)
    {
        var isOwner = login.RoleId == ProfileRole.AthleteOwner.Id;
        var isGuardian = login.RoleId == ProfileRole.Guardian.Id;

        return profile.ControlMode switch
        {
            ControlMode.AthleteControlled or ControlMode.AthleteControlledShared => isOwner,
            ControlMode.GuardianControlled or ControlMode.GuardianControlledShared => isGuardian,
            _ => false
        };
    }
}
