namespace NextAtlet.Domain.Enumerations.Enums.AthleteProfile
{
    /// <summary>
    /// Who controls a profile — a stored, explicit fact on <see cref="NextAtlet.Domain.Entities.Athlete.AthleteProfile"/>,
    /// never derived from age at runtime and never auto-mutated. The controlling party always has
    /// FullControl; the "Shared" variants additionally let the other party edit the draft (+ media)
    /// but never publish, approve, or transfer. Changed only via the transfer-control / collaboration
    /// endpoints. These four values express every supported arrangement.
    /// </summary>
    public enum ControlMode
    {
        AthleteControlled,        // athlete: FullControl  | guardian: ReadOnly
        GuardianControlled,       // guardian: FullControl | athlete: ReadOnly
        AthleteControlledShared,  // athlete: FullControl  | guardian: EditOnly
        GuardianControlledShared  // guardian: FullControl | athlete: EditOnly
    }
}
