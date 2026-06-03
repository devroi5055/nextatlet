using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class OrganizationTier : Enumeration
{
    public static readonly OrganizationTier Free = new()
    {
        Id = "free",
        Title = "Free",
        Description = "Club showcase page with a limited number of athlete slots"
    };

    public static readonly OrganizationTier Plus = new()
    {
        Id = "plus",
        Title = "Plus",
        Description = "Extended athlete slots, enhanced analytics, and event tracking"
    };

    public static readonly OrganizationTier Pro = new()
    {
        Id = "pro",
        Title = "Pro",
        Description = "Maximum slots, recruitment dashboards, and funded photoshoots for athletes"
    };

    public static IReadOnlyCollection<OrganizationTier> All => [Free, Plus, Pro];

    public static OrganizationTier FromId(string id) =>
        All.FirstOrDefault(t => t.Id == id)
        ?? throw new ArgumentException($"Unknown organization tier: '{id}'");
}