namespace NextAtlet.Domain.Enumerations.Enums.AthleteProfile
{
    /// <summary>
    /// The guardian-consent gate on a profile (GDPR Art. 8). Orthogonal to VisibilityState — it only
    /// governs whether the profile may go public, never the public/private choice itself.
    /// </summary>
    public enum ConsentState
    {
        NotRequired,            // self-consenting age, or guardian-registered (guardian present)
        PendingGuardianConsent, // under self-consent age, awaiting guardian verification — draft-editable, cannot go public
        Consented               // guardian verified — publish gate lifted
    }
}
