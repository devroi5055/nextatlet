using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Identity;

namespace NextAtlet.Domain.Entities.Identity;

/// <summary>
/// A single-use, expiring token that authorizes completing an action via an emailed link — accepting an
/// invite, giving guardian consent, or verifying an organization by email. The one home for all three
/// link-bearing flows: one table, one accept lifecycle, dispatch by <see cref="Type"/>.
///
/// The row <see cref="AuditableEntity.Id"/> (a v4 GUID, 122 bits of randomness) IS the token — used
/// directly as the link key, cryptographically sufficient, no separate secret needed. Named
/// <i>ActionToken</i> (not AccessToken) to avoid colliding with OAuth/JWT auth tokens. Completion is
/// recorded by <see cref="AcceptedUtc"/> presence — no Status enum until a flow needs to distinguish
/// withdrawal/rejection from pending.
/// </summary>
public class ActionToken : AuditableEntity
{
    /// <summary>Which action this token authorizes — selects the accept-time behavior.
    ///
    /// <para>takes a <see cref="ActionTokenType"/> ID </para> 
    /// </summary>
    public required string TypeId { get; set; }

    /// <summary>FK → the Site this token acts on (the invited/consented/verified site).</summary>
    public required Guid TargetSiteId { get; set; }

    public required DateTime ExpiresUtc { get; set; }

    /// <summary>Set once on completion. Null = pending, non-null = accepted (single-use).</summary>
    public DateTime? AcceptedUtc { get; set; }

    /// <summary>Typed, polymorphic per-<see cref="Type"/> data (jsonb). Never a loose dictionary.</summary>
    public required ActionTokenPayload Payload { get; set; }

    /// <summary>A pending token whose window has elapsed (checked at accept time — no background sweeper).</summary>
    public bool IsExpired => AcceptedUtc is null && ExpiresUtc < DateTime.UtcNow;

    /// <summary>Still claimable — issued and not yet accepted.</summary>
    public bool IsPending => AcceptedUtc is null;

    public static ActionToken Issue(string typeId, Guid targetSiteId, ActionTokenPayload payload, DateTime expiresUtc) => new()
    {
        TypeId = typeId,
        TargetSiteId = targetSiteId,
        Payload = payload,
        ExpiresUtc = expiresUtc
    };

    /// <summary>Marks the token completed. Only valid while pending.</summary>
    public void Accept(DateTime nowUtc)
    {
        if (AcceptedUtc is not null)
            throw new InvalidOperationException("Only a pending action token can be accepted.");

        AcceptedUtc = nowUtc;
    }
}
