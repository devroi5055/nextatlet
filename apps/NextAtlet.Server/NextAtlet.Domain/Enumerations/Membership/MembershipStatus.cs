using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Membership;

/// <summary>Internal audit/ownership tracking. Not user-facing.</summary>
public sealed class MembershipStatus : Enumeration
{
    public static readonly MembershipStatus Active = new()
    {
        Id = "active",
        Title = new LocalizedText { Da = "Aktiv", En = "Active" },
        Description = new LocalizedText { Da = "Medlemskab er aktivt", En = "Membership is active" }
    };

    public static readonly MembershipStatus Inactive = new()
    {
        Id = "inactive",
        Title = new LocalizedText { Da = "Inaktiv", En = "Inactive" },
        Description = new LocalizedText { Da = "Medlemskab er afsluttet men bevaret til revisionsformål", En = "Membership has ended but is retained for audit" }
    };

    public static IReadOnlyCollection<MembershipStatus> All => [Active, Inactive];

    public static MembershipStatus FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown membership status: '{id}'");
}
