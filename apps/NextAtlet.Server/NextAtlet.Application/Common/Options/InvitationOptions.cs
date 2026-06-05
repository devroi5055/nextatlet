namespace NextAtlet.Application.Common.Options;

/// <summary>
/// Tunables for the invitation lifecycle. Bound from the "Invitations" config section; sensible
/// defaults apply when unset.
/// </summary>
public class InvitationOptions
{
    public const string SectionName = "Invitations";

    /// <summary>How long a Pending invitation stays claimable. Default 7 days.</summary>
    public int ExpiryDays { get; set; } = 7;

    /// <summary>
    /// How long resolved (non-Pending) invitations are retained before a cleanup job may hard-delete
    /// them. Pending rows are never auto-deleted. The sweeper itself is deferred (see plan §10); this
    /// window is the policy it will honor. Default 90 days.
    /// </summary>
    public int RetentionDays { get; set; } = 90;
}
