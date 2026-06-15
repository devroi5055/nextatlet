using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Organization;

public sealed class OrganizationLoginStatus : Enumeration
{
    public static readonly OrganizationLoginStatus Active = new()
    {
        Id = "active",
        Title = new LocalizedText { Da = "Aktiv", En = "Active" },
        Description = new LocalizedText { Da = "Login er aktivt og gyldigt", En = "Login is active and valid" }
    };

    public static readonly OrganizationLoginStatus Revoked = new()
    {
        Id = "revoked",
        Title = new LocalizedText { Da = "Tilbagetrukket", En = "Revoked" },
        Description = new LocalizedText { Da = "Adgang er tilbagetrukket", En = "Access has been revoked" }
    };

    public static IReadOnlyCollection<OrganizationLoginStatus> All => [Active, Revoked];

    public static OrganizationLoginStatus FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown organization login status: '{id}'");
}
