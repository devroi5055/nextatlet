using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Athlete;

public class ProfileLogin : AuditableEntity
{
    public required Guid UserId { get; set; }
    public required Guid AthleteProfileId { get; set; }
    public required string RoleId { get; set; }
    public required ProfileLoginStatus Status { get; set; }

    /// <summary>
    /// Guardian-only. Null for AthleteOwner logins.
    /// Configures what the guardian may do on this profile.
    /// </summary>
    public GuardianPermissions? Permissions { get; set; }

    // Navigation — non-nullable to match non-nullable FKs
    public User User { get; set; } = default!;
    public AthleteProfile AthleteProfile { get; set; } = default!;

    // Factory methods — enforce correct defaults per role
    public static ProfileLogin CreateOwner(Guid userId, Guid profileId) => new()
    {
        UserId = userId,
        AthleteProfileId = profileId,
        RoleId = ProfileRole.AthleteOwner.Id,
        Status = ProfileLoginStatus.Active,
        Permissions = null
    };

    public static ProfileLogin CreateGuardian(Guid userId, AthleteProfile profile)
    {
        if (!profile.IsMinor)
            throw new InvalidOperationException(
                "A guardian login can only be created for a minor athlete profile.");

        return new()
        {
            UserId = userId,
            AthleteProfileId = profile.Id,
            RoleId = ProfileRole.Guardian.Id,
            Status = ProfileLoginStatus.Pending,
            Permissions = GuardianPermissions.Defaults()
        };
    }
}

