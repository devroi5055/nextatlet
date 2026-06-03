using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class Locale : Enumeration
{
    public static readonly Locale Da = new()
    {
        Id = "da",
        Title = "Dansk",
        Description = "Danish"
    };

    public static readonly Locale En = new()
    {
        Id = "en",
        Title = "English",
        Description = "English"
    };

    public static IReadOnlyCollection<Locale> All => [Da, En];

    public static Locale FromId(string id) =>
        All.FirstOrDefault(l => l.Id == id)
        ?? throw new ArgumentException($"Unknown locale: '{id}'");
}