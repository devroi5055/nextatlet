using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Organization;

public sealed class OrganizationRole : Enumeration
{
    public static readonly OrganizationRole ClubAdmin = new()
    {
        Id = "club_admin",
        Title = new LocalizedText { Da = "Administrator", En = "Admin" },
        Description = new LocalizedText { Da = "Administrerer fakturering, abonnement, brugere og alle klubindstillinger", En = "Manages billing, subscription, users, and all club settings" }
    };

    public static readonly OrganizationRole ClubEditor = new()
    {
        Id = "club_editor",
        Title = new LocalizedText { Da = "Redaktør", En = "Editor" },
        Description = new LocalizedText { Da = "Redigerer klubsideindhold og administrerer fremhævede atleter. Kan ikke berøre fakturering eller brugere", En = "Edits club page content and manages featured athletes. Cannot touch billing or users" }
    };

    // Reserved — ClubViewer, Coach, Photographer

    public static IReadOnlyCollection<OrganizationRole> All => [ClubAdmin, ClubEditor];

    public static OrganizationRole FromId(string id) =>
        All.FirstOrDefault(r => r.Id == id)
        ?? throw new ArgumentException($"Unknown organization role: '{id}'");
}
