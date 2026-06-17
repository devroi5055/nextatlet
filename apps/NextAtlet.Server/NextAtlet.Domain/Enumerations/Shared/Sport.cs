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
    public static readonly Sport JuiJitsu = new()
    {
        Id = "jui-jitsu",
        Title = new LocalizedText { Da = "Jui-Jitsu", En = "Jui-Jitsu" },
        Description = new LocalizedText { Da = "Olympisk kampsport", En = "Olympic combat sport" }
    };

    public static IReadOnlyCollection<Sport> All => [Judo, JuiJitsu];

    public static Sport FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown sport: '{id}'");
}
