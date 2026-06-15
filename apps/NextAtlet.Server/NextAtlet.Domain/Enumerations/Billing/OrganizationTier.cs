using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Billing;

public sealed class OrganizationTier : Enumeration
{
    public static readonly OrganizationTier Free = new()
    {
        Id = "free",
        Title = new LocalizedText { Da = "Gratis", En = "Free" },
        Description = new LocalizedText { Da = "Klubvitrineside med et begrænset antal atletpladser", En = "Club showcase page with a limited number of athlete slots" }
    };

    public static readonly OrganizationTier Plus = new()
    {
        Id = "plus",
        Title = new LocalizedText { Da = "Plus", En = "Plus" },
        Description = new LocalizedText { Da = "Udvidede atletpladser, forbedret analyse og event-tracking", En = "Extended athlete slots, enhanced analytics, and event tracking" }
    };

    public static readonly OrganizationTier Pro = new()
    {
        Id = "pro",
        Title = new LocalizedText { Da = "Pro", En = "Pro" },
        Description = new LocalizedText { Da = "Maksimale pladser, rekrutteringsdashboards og finansierede fotosessioner for atleter", En = "Maximum slots, recruitment dashboards, and funded photoshoots for athletes" }
    };

    public static IReadOnlyCollection<OrganizationTier> All => [Free, Plus, Pro];

    public static OrganizationTier FromId(string id) =>
        All.FirstOrDefault(t => t.Id == id)
        ?? throw new ArgumentException($"Unknown organization tier: '{id}'");
}
