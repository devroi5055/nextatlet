using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.AthleteProfile;

public sealed class VisibilityState : Enumeration
{
    public static readonly VisibilityState Public = new()
    {
        Id = "public",
        Title = new LocalizedText { Da = "Offentlig", En = "Public" },
        Description = new LocalizedText { Da = "Synlig for alle og opdagelig af sponsorer og klubber", En = "Visible to everyone and discoverable by sponsors and clubs" }
    };

    public static readonly VisibilityState Private = new()
    {
        Id = "private",
        Title = new LocalizedText { Da = "Privat", En = "Private" },
        Description = new LocalizedText { Da = "Skjult for offentligheden og klubvitrinier", En = "Hidden from public view and club showcases" }
    };

    public static IReadOnlyCollection<VisibilityState> All => [Public, Private];

    public static VisibilityState FromId(string id) =>
        All.FirstOrDefault(v => v.Id == id)
        ?? throw new ArgumentException($"Unknown visibility state: '{id}'");
}
