using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Membership;

public sealed class ChangeRequestStatus : Enumeration
{
    public static readonly ChangeRequestStatus Pending = new()
    {
        Id = "pending",
        Title = new LocalizedText { Da = "Afventer", En = "Pending" },
        Description = new LocalizedText { Da = "Afventer gennemgang af atleten eller forældre/værge", En = "Awaiting review by the athlete or guardian" }
    };

    public static readonly ChangeRequestStatus Approved = new()
    {
        Id = "approved",
        Title = new LocalizedText { Da = "Godkendt", En = "Approved" },
        Description = new LocalizedText { Da = "Accepteret — foreslåede sektioner er flettet ind i kladden", En = "Accepted — proposed sections have been merged into the draft" }
    };

    public static readonly ChangeRequestStatus Rejected = new()
    {
        Id = "rejected",
        Title = new LocalizedText { Da = "Afvist", En = "Rejected" },
        Description = new LocalizedText { Da = "Afvist af atleten eller forældre/værge. Ingen ændringer blev anvendt", En = "Declined by the athlete or guardian. No changes were applied" }
    };

    public static readonly ChangeRequestStatus Withdrawn = new()
    {
        Id = "withdrawn",
        Title = new LocalizedText { Da = "Trukket tilbage", En = "Withdrawn" },
        Description = new LocalizedText { Da = "Tilbagetrukket af den foreslående organisation, inden det blev gennemgået", En = "Retracted by the proposing organization before it was reviewed" }
    };

    public static IReadOnlyCollection<ChangeRequestStatus> All => [Pending, Approved, Rejected, Withdrawn];

    public static ChangeRequestStatus FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown change request status: '{id}'");
}
