namespace NextAtlet.Domain.Enumerations.Enums.AthleteProfile
{
    /// <summary>
    /// HOW guardian consent was verified (a GDPR-required fact). MVP: the guardian authenticated via
    /// Auth0 and confirmed — stronger than a bare self-declared checkbox. MitID is the natural
    /// hard-assurance upgrade for the Danish audience.
    /// </summary>
    public enum ConsentMethod
    {
        VerifiedEmail // guardian authenticated via Auth0 + confirmed
        // Future: MitId, SmsToken, ...
    }
}
