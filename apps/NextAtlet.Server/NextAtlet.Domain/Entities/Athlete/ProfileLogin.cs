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

    /// <summary>
    /// Creates a guardian login for a minor profile. <paramref name="active"/> = false (default) is an
    /// invite the guardian must accept (self-minor flow); true means the caller IS the guardian and has
    /// consented by creating the child's profile (guardian-creates-child flow).
    /// </summary>
    public static ProfileLogin CreateGuardian(Guid userId, AthleteProfile profile, bool active = false)
    {
        if (!profile.IsMinor)
            throw new InvalidOperationException(
                "A guardian login can only be created for a minor athlete profile.");

        return new()
        {
            UserId = userId,
            AthleteProfileId = profile.Id,
            RoleId = ProfileRole.Guardian.Id,
            Status = active ? ProfileLoginStatus.Active : ProfileLoginStatus.Pending,
            Permissions = GuardianPermissions.Defaults()
        };
    }

    /// <summary>
    /// The invited guardian accepts: the login becomes Active (they can now publish/approve).
    /// Idempotent for an already-active login; only valid on a Guardian login.
    /// </summary>
    public void Accept()
    {
        if (RoleId != ProfileRole.Guardian.Id)
            throw new InvalidOperationException("Only a guardian login can be accepted.");

        Status = ProfileLoginStatus.Active;
    }
}

