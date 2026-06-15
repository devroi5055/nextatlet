using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Organization;

public sealed class VerificationMethod : Enumeration
{
    public static readonly VerificationMethod Manual = new()
    {
        Id = "manuel",
        Title = new LocalizedText { Da = "Manuel", En = "Manual" },
        Description = new LocalizedText { Da = "Manuel verificering", En = "Manual verification" }
    };

    public static readonly VerificationMethod MitId = new()
    {
        Id = "mit_id",
        Title = new LocalizedText { Da = "MitID", En = "MitID" },
        Description = new LocalizedText { Da = "Verificering via MitID", En = "Verification via MitID" }
    };

    public static readonly VerificationMethod CVR = new()
    {
        Id = "cvr",
        Title = new LocalizedText { Da = "CVR", En = "CVR" },
        Description = new LocalizedText { Da = "Verificering via CVR", En = "Verification via CVR" }
    };

    public static readonly VerificationMethod Email = new()
    {
        Id = "email",
        Title = new LocalizedText { Da = "Email", En = "Email" },
        Description = new LocalizedText { Da = "Verificering via email", En = "Verification via email" }
    };

    public static readonly VerificationMethod Phone = new()
    {
        Id = "phone",
        Title = new LocalizedText { Da = "Telefon", En = "Phone" },
        Description = new LocalizedText { Da = "Verificering via telefonopkald", En = "Verification via phone call" }
    };

    public static IReadOnlyCollection<VerificationMethod> All => [Manual, MitId, CVR, Email, Phone];

    public static VerificationMethod FromId(string id) =>
        All.FirstOrDefault(v => v.Id == id)
        ?? throw new ArgumentException($"Unknown verification method: '{id}'");
}
