using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class OrganizationType : Enumeration
{
    public static readonly OrganizationType Club = new()
    {
        Id = "club",
        Title = "Club",
        Description = "A local or regional sports club with an active roster"
    };

    public static readonly OrganizationType NationalTeam = new()
    {
        Id = "national_team",
        Title = "National Team",
        Description = "Server-managed national team entity. Assigned by NextAtlet admins only"
    };

    public static readonly OrganizationType Academy = new()
    {
        Id = "academy",
        Title = "Academy",
        Description = "A dedicated training academy"
    };

    public static readonly OrganizationType TrainingCenter = new()
    {
        Id = "training_center",
        Title = "Training Center",
        Description = "A training facility athletes are affiliated with"
    };

    public static readonly OrganizationType SchoolTeam = new()
    {
        Id = "school_team",
        Title = "School Team",
        Description = "A school-based team or sports program"
    };

    public static IReadOnlyCollection<OrganizationType> All =>
        [Club, NationalTeam, Academy, TrainingCenter, SchoolTeam];

    public static OrganizationType FromId(string id) =>
        All.FirstOrDefault(t => t.Id == id)
        ?? throw new ArgumentException($"Unknown organization type: '{id}'");
}