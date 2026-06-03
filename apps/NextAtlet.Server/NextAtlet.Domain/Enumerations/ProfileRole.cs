using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class ProfileRole : Enumeration
{
    public static readonly ProfileRole AthleteOwner = new()
    {
        Id = "athlete_owner",
        Title = "Athlete",
        Description = "Owns the profile. Capability scales with effective tier and perks"
    };

    public static readonly ProfileRole Guardian = new()
    {
        Id = "guardian",
        Title = "Guardian",
        Description = "Parent or legal guardian linked to a minor's profile. Holds publish and approval authority"
    };

    public static IReadOnlyCollection<ProfileRole> All => [AthleteOwner, Guardian];

    public static ProfileRole FromId(string id) =>
        All.FirstOrDefault(r => r.Id == id)
        ?? throw new ArgumentException($"Unknown profile role: '{id}'");
}