using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class VisibilityState : Enumeration
{
    public static readonly VisibilityState Public = new()
    {
        Id = "public",
        Title = "Public",
        Description = "Visible to everyone and discoverable by sponsors and clubs"
    };

    public static readonly VisibilityState Private = new()
    {
        Id = "private",
        Title = "Private",
        Description = "Hidden from public view and club showcases"
    };

    public static IReadOnlyCollection<VisibilityState> All => [Public, Private];

    public static VisibilityState FromId(string id) =>
        All.FirstOrDefault(v => v.Id == id)
        ?? throw new ArgumentException($"Unknown visibility state: '{id}'");
}