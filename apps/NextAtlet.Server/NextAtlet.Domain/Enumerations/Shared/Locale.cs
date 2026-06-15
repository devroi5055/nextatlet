using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Shared;

public sealed class Locale : Enumeration
{
    public static readonly Locale Da = new()
    {
        Id = "da",
        Title = new LocalizedText { Da = "Dansk", En = "Danish" },
        Description = new LocalizedText { Da = "Dansk", En = "Danish" }
    };

    public static readonly Locale En = new()
    {
        Id = "en",
        Title = new LocalizedText { Da = "Engelsk", En = "English" },
        Description = new LocalizedText { Da = "Engelsk", En = "English" }
    };

    public static IReadOnlyCollection<Locale> All => [Da, En];

    public static Locale FromId(string id) =>
        All.FirstOrDefault(l => l.Id == id)
        ?? throw new ArgumentException($"Unknown locale: '{id}'");
}
