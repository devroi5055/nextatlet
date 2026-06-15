using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Shared;

public sealed class AgeBand : Enumeration
{
    public static readonly AgeBand BelowMinimum = new()
    {
        Id = "below_minimum",
        Title = new LocalizedText { Da = "Under minimumsalder", En = "Below minimum age" },
        Description = new LocalizedText { Da = "Under den absolutte minimumsalder (< 13)", En = "Below the absolute minimum age (< 13)" }
    };

    public static readonly AgeBand YoungMinor = new()
    {
        Id = "young_minor",
        Title = new LocalizedText { Da = "Ung mindreårig", En = "Young minor" },
        Description = new LocalizedText { Da = "13–15 år", En = "Age 13–15" }
    };

    public static readonly AgeBand OlderMinor = new()
    {
        Id = "older_minor",
        Title = new LocalizedText { Da = "Ældre mindreårig", En = "Older minor" },
        Description = new LocalizedText { Da = "16–17 år", En = "Age 16–17" }
    };

    public static readonly AgeBand Adult = new()
    {
        Id = "adult",
        Title = new LocalizedText { Da = "Voksen", En = "Adult" },
        Description = new LocalizedText { Da = "18 år eller derover", En = "Age 18 or above" }
    };

    public static IReadOnlyCollection<AgeBand> All => [BelowMinimum, YoungMinor, OlderMinor, Adult];

    public static AgeBand FromId(string id) =>
        All.FirstOrDefault(b => b.Id == id)
        ?? throw new ArgumentException($"Unknown age band: '{id}'");
}
