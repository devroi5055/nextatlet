using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.AthleteProfile;

/// <summary>
/// Who controls a profile — a stored, explicit fact, never derived from age and never auto-mutated.
/// Changed only via the transfer-control / collaboration endpoints.
/// </summary>
public sealed class ControlMode : Enumeration
{
    public static readonly ControlMode AthleteControlled = new()
    {
        Id = "athlete_controlled",
        Title = new LocalizedText { Da = "Atletkontrolleret", En = "Athlete controlled" },
        Description = new LocalizedText { Da = "Atlet: fuld kontrol | Forældre/Værge: kun læsning", En = "Athlete: full control | Guardian: read only" }
    };

    public static readonly ControlMode GuardianControlled = new()
    {
        Id = "guardian_controlled",
        Title = new LocalizedText { Da = "Forældre-/værgekontrolleret", En = "Guardian controlled" },
        Description = new LocalizedText { Da = "Forældre/Værge: fuld kontrol | Atlet: kun læsning", En = "Guardian: full control | Athlete: read only" }
    };

    public static readonly ControlMode AthleteControlledShared = new()
    {
        Id = "athlete_controlled_shared",
        Title = new LocalizedText { Da = "Atletkontrolleret (delt)", En = "Athlete controlled (shared)" },
        Description = new LocalizedText { Da = "Atlet: fuld kontrol | Forældre/Værge: kan redigere kladde", En = "Athlete: full control | Guardian: may edit draft" }
    };

    public static readonly ControlMode GuardianControlledShared = new()
    {
        Id = "guardian_controlled_shared",
        Title = new LocalizedText { Da = "Forældre-/værgekontrolleret (delt)", En = "Guardian controlled (shared)" },
        Description = new LocalizedText { Da = "Forældre/Værge: fuld kontrol | Atlet: kan redigere kladde", En = "Guardian: full control | Athlete: may edit draft" }
    };

    public static IReadOnlyCollection<ControlMode> All => [AthleteControlled, GuardianControlled, AthleteControlledShared, GuardianControlledShared];

    public static ControlMode FromId(string id) =>
        All.FirstOrDefault(c => c.Id == id)
        ?? throw new ArgumentException($"Unknown control mode: '{id}'");
}
