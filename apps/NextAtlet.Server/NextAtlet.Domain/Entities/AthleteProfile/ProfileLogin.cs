using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.AthleteProfile;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Athlete;

public class ProfileLogin : AuditableEntity
{
    public required Guid UserId { get; set; }
    public required Guid AthleteProfileId { get; set; }
    public required string RoleId { get; set; }
    public required string StatusId { get; set; }

    /// <summary>
    /// Legacy per-login permission blob. Retained on the table but always null and never read —
    /// permissions are resolved at request time from <c>ControlMode</c> + role by the PermissionResolver.
    /// See the ControlMode plan §7 / §13 (only repopulate if arbitrary per-login grants ever become a real need).
    /// </summary>
    public GuardianPermissions? Permissions { get; set; }

    // Navigation — non-nullable to match non-nullable FKs
    public User User { get; set; } = default!;
    public AthleteSite AthleteSite { get; set; } = default!;

    // Factory methods — set role + status only. A login is created Active; permissions are derived,
    // never stored. (Pending state for an unaccepted invite now lives on the Invitation, not here.)
    public static ProfileLogin CreateOwner(Guid userId, Guid profileId) => new()
    {
        UserId = userId,
        AthleteProfileId = profileId,
        RoleId = ProfileRole.AthleteOwner.Id,
        StatusId = ProfileLoginStatus.Active.Id,
        Permissions = null
    };

    public static ProfileLogin CreateGuardian(Guid userId, Guid profileId) => new()
    {
        UserId = userId,
        AthleteProfileId = profileId,
        RoleId = ProfileRole.Guardian.Id,
        StatusId = ProfileLoginStatus.Active.Id,
        Permissions = null
    };
}

