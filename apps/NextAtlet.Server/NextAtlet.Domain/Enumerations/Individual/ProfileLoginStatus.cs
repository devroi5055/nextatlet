using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Individual;

public sealed class ProfileLoginStatus : Enumeration
{
    public static readonly ProfileLoginStatus Pending = new()
    {
        Id = "pending",
        Title = new LocalizedText { Da = "Afventer", En = "Pending" },
        Description = new LocalizedText { Da = "Forældre/Værge inviteret men ikke accepteret endnu", En = "Guardian invited but not yet accepted" }
    };

    public static readonly ProfileLoginStatus Active = new()
    {
        Id = "active",
        Title = new LocalizedText { Da = "Aktiv", En = "Active" },
        Description = new LocalizedText { Da = "Login er aktivt og gyldigt", En = "Login is active and valid" }
    };

    public static readonly ProfileLoginStatus Revoked = new()
    {
        Id = "revoked",
        Title = new LocalizedText { Da = "Tilbagetrukket", En = "Revoked" },
        Description = new LocalizedText { Da = "Adgang er tilbagetrukket", En = "Access has been revoked" }
    };

    public static IReadOnlyCollection<ProfileLoginStatus> All => [Pending, Active, Revoked];

    public static ProfileLoginStatus FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown profile login status: '{id}'");
}
