using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;


/// <summary>
/// Enumeration of supported sports. Extend as new sports are added.
/// </summary>
public sealed class Sport : Enumeration
{
    public static readonly Sport Judo = new()
    {
        Id = "judo",
        Title = "Judo",
        Description = "Olympic combat sport"
    };

    public static IReadOnlyCollection<Sport> All => [Judo];

    public static Sport FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown sport: {id}");
}