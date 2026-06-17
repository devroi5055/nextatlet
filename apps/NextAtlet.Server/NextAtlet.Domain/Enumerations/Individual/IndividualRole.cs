using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Individual;

public sealed class IndividualRole : Enumeration
{
    public static readonly IndividualRole Owner = new()
    {
        Id = "owner",
        Title = new LocalizedText { Da = "Ejer", En = "Owner" },
        Description = new LocalizedText { Da = "Ejer profilen. Rettigheder skalerer med effektivt tier og perks", En = "Owns the profile. Capability scales with effective tier and perks" }
    };

    public static readonly IndividualRole Guardian = new()
    {
        Id = "guardian",
        Title = new LocalizedText { Da = "Forældre/Værge", En = "Guardian" },
        Description = new LocalizedText { Da = "Forælder eller værge tilknyttet en mindreårigs profil. Har publicerings- og godkendelsesrettigheder", En = "Parent or legal guardian linked to a minor's profile. Holds publish and approval authority" }
    };

    public static IReadOnlyCollection<IndividualRole> All => [Owner, Guardian];

    public static IndividualRole FromId(string id) =>
        All.FirstOrDefault(r => r.Id == id)
        ?? throw new ArgumentException($"Unknown profile role: '{id}'");
}
