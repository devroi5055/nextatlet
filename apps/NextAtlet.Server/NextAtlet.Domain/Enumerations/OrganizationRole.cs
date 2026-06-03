using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class OrganizationRole : Enumeration
{
    public static readonly OrganizationRole ClubAdmin = new()
    {
        Id = "club_admin",
        Title = "Admin",
        Description = "Manages billing, subscription, users, and all club settings"
    };

    public static readonly OrganizationRole ClubEditor = new()
    {
        Id = "club_editor",
        Title = "Editor",
        Description = "Edits club page content and manages featured athletes. Cannot touch billing or users"
    };

    // Reserved — document but do not instantiate until built
    // ClubViewer, Coach, Photographer

    public static IReadOnlyCollection<OrganizationRole> All => [ClubAdmin, ClubEditor];

    public static OrganizationRole FromId(string id) =>
        All.FirstOrDefault(r => r.Id == id)
        ?? throw new ArgumentException($"Unknown organization role: '{id}'");
}