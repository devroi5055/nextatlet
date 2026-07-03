using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Shared;

public sealed class Sport : Enumeration
{
    public static readonly Sport Judo = new()
    {
        Id = "judo",
        Title = new LocalizedText { Da = "Judo", En = "Judo" },
        Description = new LocalizedText { Da = "Olympisk kampsport", En = "Olympic combat sport" }
    };
    public static readonly Sport JiuJitsu = new()
    {
        Id = "jiu_jitsu",
        Title = new LocalizedText { Da = "Jiu-Jitsu", En = "Jiu-Jitsu" },
        Description = new LocalizedText { Da = "Olympisk kampsport", En = "Olympic combat sport" }
    };

    public static IReadOnlyCollection<Sport> All => [Judo, JiuJitsu];

    public static Sport FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown sport: '{id}'");
}
