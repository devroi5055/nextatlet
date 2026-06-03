using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class AthleteTier : Enumeration
{
    public static readonly AthleteTier Free = new()
    {
        Id = "free",
        Title = "Free",
        Description = "A simple public profile page with core sections"
    };

    public static readonly AthleteTier Plus = new()
    {
        Id = "plus",
        Title = "Plus",
        Description = "Extended customization, gallery, mentoring guides, and photoshoot discounts"
    };

    public static readonly AthleteTier Pro = new()
    {
        Id = "pro",
        Title = "Pro",
        Description = "Full customization, video, 1:1 mentoring, and included photoshoot sessions"
    };

    public static IReadOnlyCollection<AthleteTier> All => [Free, Plus, Pro];

    public static AthleteTier FromId(string id) =>
        All.FirstOrDefault(t => t.Id == id)
        ?? throw new ArgumentException($"Unknown athlete tier: '{id}'");
}