using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.AthleteProfile;

/// <summary>
/// Lifecycle of an Invitation. Only Pending rows are actionable; the rest are terminal and retained for audit.
/// </summary>
public sealed class InvitationStatus : Enumeration
{
    public static readonly InvitationStatus Pending = new()
    {
        Id = "pending",
        Title = new LocalizedText { Da = "Afventer", En = "Pending" },
        Description = new LocalizedText { Da = "Udstedt, afventer accept", En = "Issued, awaiting acceptance" }
    };

    public static readonly InvitationStatus Accepted = new()
    {
        Id = "accepted",
        Title = new LocalizedText { Da = "Accepteret", En = "Accepted" },
        Description = new LocalizedText { Da = "Krævet — et ProfileLogin er oprettet", En = "Claimed — a ProfileLogin was materialized" }
    };

    public static readonly InvitationStatus Expired = new()
    {
        Id = "expired",
        Title = new LocalizedText { Da = "Udløbet", En = "Expired" },
        Description = new LocalizedText { Da = "Passerede udløbsdatoen uden at blive accepteret", En = "Passed ExpiresUtc without being accepted" }
    };

    public static readonly InvitationStatus Revoked = new()
    {
        Id = "revoked",
        Title = new LocalizedText { Da = "Tilbagetrukket", En = "Revoked" },
        Description = new LocalizedText { Da = "Trukket tilbage af afsender", En = "Withdrawn by an inviter" }
    };

    public static IReadOnlyCollection<InvitationStatus> All => [Pending, Accepted, Expired, Revoked];

    public static InvitationStatus FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown invitation status: '{id}'");
}
