using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.AthleteProfile;

public sealed class VisibilityStates : Enumeration
{
    public static readonly VisibilityStates Public = new()
    {
        Id = "public",
        Title = new LocalizedText { Da = "Offentlig", En = "Public" },
        Description = new LocalizedText { Da = "Synlig for alle og opdagelig af sponsorer og klubber", En = "Visible to everyone and discoverable by sponsors and clubs" }
    };

    public static readonly VisibilityStates Private = new()
    {
        Id = "private",
        Title = new LocalizedText { Da = "Privat", En = "Private" },
        Description = new LocalizedText { Da = "Skjult for offentligheden og klubvitrinier", En = "Hidden from public view and club showcases" }
    };

    public static IReadOnlyCollection<VisibilityStates> All => [Public, Private];

    public static VisibilityStates FromId(string id) =>
        All.FirstOrDefault(v => v.Id == id)
        ?? throw new ArgumentException($"Unknown visibility state: '{id}'");
}
