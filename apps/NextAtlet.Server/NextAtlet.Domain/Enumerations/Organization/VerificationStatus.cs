using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Organization;

public sealed class VerificationStatus : Enumeration
{
    public static readonly VerificationStatus Pending = new()
    {
        Id = "pending",
        Title = new LocalizedText { Da = "Afventer", En = "Pending" },
        Description = new LocalizedText { Da = "Verifikationsanmodning afventer gennemgang", En = "Verification request is pending review" }
    };

    public static readonly VerificationStatus Verified = new()
    {
        Id = "verified",
        Title = new LocalizedText { Da = "Verificeret", En = "Verified" },
        Description = new LocalizedText { Da = "Organisation er verificeret", En = "Organization has been verified" }
    };

    public static readonly VerificationStatus Rejected = new()
    {
        Id = "rejected",
        Title = new LocalizedText { Da = "Afvist", En = "Rejected" },
        Description = new LocalizedText { Da = "Verifikationsanmodning er afvist", En = "Verification request has been rejected" }
    };

    public static IReadOnlyCollection<VerificationStatus> All => [Pending, Verified, Rejected];

    public static VerificationStatus FromId(string id) =>
        All.FirstOrDefault(v => v.Id == id)
        ?? throw new ArgumentException($"Unknown verification status: '{id}'");
}
