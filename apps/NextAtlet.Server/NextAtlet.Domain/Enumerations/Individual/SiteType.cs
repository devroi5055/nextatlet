using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.AthleteProfile;

public sealed class SiteProfiles : Enumeration
{
    public static readonly SiteProfiles Athlete = new()
    {
        Id = "athlete",
        Title = new LocalizedText { Da = "Atlet", En = "athlete" },
        Description = new LocalizedText { Da = "Personlig Atlet Side", En = "Personal Athlete Site" }
    };

    public static readonly SiteProfiles Organization = new()
    {
        Id = "organization",
        Title = new LocalizedText { Da = "Organisation", En = "Organization" },
        Description = new LocalizedText { Da = "Organisations Profil", En = "Organization Profile" }
    };

    public static IReadOnlyCollection<SiteProfiles> All => [Athlete, Organization];

    public static SiteProfiles FromId(string id) =>
        All.FirstOrDefault(v => v.Id == id)
        ?? throw new ArgumentException($"Unknown Site Profile: '{id}'");
}
