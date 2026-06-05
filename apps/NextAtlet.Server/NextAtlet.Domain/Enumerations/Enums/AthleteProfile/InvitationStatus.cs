namespace NextAtlet.Domain.Enumerations.Enums.AthleteProfile
{
    /// <summary>
    /// Lifecycle of an <see cref="NextAtlet.Domain.Entities.Athlete.Invitation"/>.
    /// Only <see cref="Pending"/> rows are actionable; the rest are terminal and retained for audit.
    /// </summary>
    public enum InvitationStatus
    {
        Pending,  // issued, awaiting acceptance
        Accepted, // claimed — a ProfileLogin was materialized
        Expired,  // passed ExpiresUtc without being accepted
        Revoked,  // withdrawn by an inviter (future feature)
    }
}
