using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Billing;

public sealed class AthleteTier : Enumeration
{
    public static readonly AthleteTier Free = new()
    {
        Id = "free",
        Title = new LocalizedText { Da = "Gratis", En = "Free" },
        Description = new LocalizedText { Da = "En simpel offentlig profilside med kernektioner", En = "A simple public profile page with core sections" }
    };

    public static readonly AthleteTier Plus = new()
    {
        Id = "plus",
        Title = new LocalizedText { Da = "Plus", En = "Plus" },
        Description = new LocalizedText { Da = "Udvidet tilpasning, galleri, mentorguider og fotosessionrabatter", En = "Extended customization, gallery, mentoring guides, and photoshoot discounts" }
    };

    public static readonly AthleteTier Pro = new()
    {
        Id = "pro",
        Title = new LocalizedText { Da = "Pro", En = "Pro" },
        Description = new LocalizedText { Da = "Fuld tilpasning, video, 1:1-mentoring og inkluderede fotosessioner", En = "Full customization, video, 1:1 mentoring, and included photoshoot sessions" }
    };

    public static IReadOnlyCollection<AthleteTier> All => [Free, Plus, Pro];

    public static AthleteTier FromId(string id) =>
        All.FirstOrDefault(t => t.Id == id)
        ?? throw new ArgumentException($"Unknown athlete tier: '{id}'");
}
