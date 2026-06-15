using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Organization;

public sealed class OrganizationType : Enumeration
{
    public static readonly OrganizationType Club = new()
    {
        Id = "club",
        Title = new LocalizedText { Da = "Klub", En = "Club" },
        Description = new LocalizedText { Da = "En lokal eller regional sportsklub med en aktiv liste", En = "A local or regional sports club with an active roster" }
    };

    public static readonly OrganizationType NationalTeam = new()
    {
        Id = "national_team",
        Title = new LocalizedText { Da = "Landshold", En = "National Team" },
        Description = new LocalizedText { Da = "Serveradministreret landshold. Tildeles kun af NextAtlet-administratorer", En = "Server-managed national team entity. Assigned by NextAtlet admins only" }
    };

    public static readonly OrganizationType Academy = new()
    {
        Id = "academy",
        Title = new LocalizedText { Da = "Akademi", En = "Academy" },
        Description = new LocalizedText { Da = "Et dedikeret træningsakademi", En = "A dedicated training academy" }
    };

    public static readonly OrganizationType TrainingCenter = new()
    {
        Id = "training_center",
        Title = new LocalizedText { Da = "Træningscenter", En = "Training Center" },
        Description = new LocalizedText { Da = "Et træningscenter som atleter er tilknyttet", En = "A training facility athletes are affiliated with" }
    };

    public static readonly OrganizationType SchoolTeam = new()
    {
        Id = "school_team",
        Title = new LocalizedText { Da = "Skolehold", En = "School Team" },
        Description = new LocalizedText { Da = "Et skolebaseret hold eller sportsprogram", En = "A school-based team or sports program" }
    };

    public static IReadOnlyCollection<OrganizationType> All => [Club, NationalTeam, Academy, TrainingCenter, SchoolTeam];

    public static OrganizationType FromId(string id) =>
        All.FirstOrDefault(t => t.Id == id)
        ?? throw new ArgumentException($"Unknown organization type: '{id}'");
}
