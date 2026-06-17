using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Shared;

public sealed class Country : Enumeration
{
    public static readonly Country Denmark = new()
    {
        Id = "denmark",
        Title = new LocalizedText { Da = "Danmark", En = "Denmark" }
    };
    public static readonly Country Sweden = new()
    {
        Id = "sweden",
        Title = new LocalizedText { Da = "Sverige", En = "Sweden" }
    };
    public static readonly Country Germany = new()
    {
        Id = "germany",
        Title = new LocalizedText { Da = "Tyskland", En = "Germany" }
    };
    public static readonly Country Norway = new()
    {
        Id = "norway",
        Title = new LocalizedText { Da = "Norge", En = "Norway" }
    };
    public static readonly Country Netherlands = new()
    {
        Id = "netherlands",
        Title = new LocalizedText { Da = "Holland", En = "Netherlands" }
    };

    public static IReadOnlyCollection<Country> All => [Denmark, Sweden, Germany, Norway, Netherlands];
    public static Country FromId(string id) =>
        All.FirstOrDefault(c => c.Id == id)
        ?? throw new ArgumentException($"Unknown country: '{id}'");
}
