using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Domain.Entities.Athlete;

/// <summary>
/// A pending, audited offer to link a person (by email) to a profile in a given role. The single,
/// secure home for all invites — guardian invites issued during minor self-registration, and any
/// invite created via the invite endpoint. Lives in its own table (not overloaded onto User or
/// ProfileLogin) precisely because it carries expiry, status, and an audit trail — same reasoning as
/// ChangeRequest living outside SiteConfig.
///
/// The row <see cref="AuditableEntity.Id"/> (a v4 GUID, 122 bits of randomness) is used directly as
/// the URL identifier in the accept link — cryptographically sufficient, no separate token needed.
/// The materialized credential (a <see cref="ProfileLogin"/>) is created only at accept time, so a
/// revoked/expired invite never leaves a dangling login.
/// </summary>
public class Invitation : AuditableEntity
{
    /// <summary>FK → AthleteProfile this invitation grants access to.</summary>
    public required Guid TargetProfileId { get; set; }

    /// <summary>The <see cref="NextAtlet.Domain.Enumerations.ProfileRole"/> id to grant on acceptance.</summary>
    public required string RoleId { get; set; }

    /// <summary>The email the invite is addressed to; matched against the claimer's token at accept time.</summary>
    public required string Email { get; set; }

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    public required DateTime ExpiresUtc { get; set; }

    /// <summary>FK → User who issued the invite (audit trail; matters for minor guardianships).</summary>
    public required Guid InvitedByUserId { get; set; }

    public DateTime? AcceptedUtc { get; set; }

    // Navigation
    public AthleteProfile TargetProfile { get; set; } = default!;
    public User InvitedBy { get; set; } = default!;

    /// <summary>A Pending invitation whose window has elapsed (checked at accept time).</summary>
    public bool IsExpired => Status == InvitationStatus.Pending && ExpiresUtc < DateTime.UtcNow;

    public static Invitation Issue(Guid targetProfileId, string email, string roleId, Guid invitedByUserId, DateTime expiresUtc) => new()
    {
        TargetProfileId = targetProfileId,
        Email = email,
        RoleId = roleId,
        InvitedByUserId = invitedByUserId,
        ExpiresUtc = expiresUtc,
        Status = InvitationStatus.Pending
    };

    /// <summary>Marks the invitation claimed. Only valid on a Pending invitation.</summary>
    public void Accept()
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Only a pending invitation can be accepted.");

        Status = InvitationStatus.Accepted;
        AcceptedUtc = DateTime.UtcNow;
    }
}
