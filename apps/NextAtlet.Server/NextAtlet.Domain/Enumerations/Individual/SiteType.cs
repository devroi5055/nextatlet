using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Individual;

public sealed class SiteType : Enumeration
{
    public static readonly SiteType Individual = new()
    {
        Id = "individual",
        Title = new LocalizedText { Da = "Individuel", En = "Individual" },
        Description = new LocalizedText { Da = "Personlig side for en enkeltperson", En = "Personal site for an individual" }
    };

    public static readonly SiteType Organization = new()
    {
        Id = "organization",
        Title = new LocalizedText { Da = "Organisation", En = "Organization" },
        Description = new LocalizedText { Da = "Organisations Profil", En = "Organization Profile" }
    };

    public static IReadOnlyCollection<SiteType> All => [Individual, Organization];

    public static SiteType FromId(string id) =>
        All.FirstOrDefault(v => v.Id == id)
        ?? throw new ArgumentException($"Unknown site type: '{id}'");
}
