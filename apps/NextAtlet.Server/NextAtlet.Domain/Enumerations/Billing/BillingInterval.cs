using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Billing;

/// <summary>Billing cycle. Not shown as a standalone user-facing label.</summary>
public sealed class BillingInterval : Enumeration
{
    public static readonly BillingInterval None = new()
    {
        Id = "none",
        Title = new LocalizedText { Da = "Ingen", En = "None" },
        Description = new LocalizedText { Da = "Gratis plan — ingen betalingscyklus", En = "Free plan — no billing cycle" }
    };

    public static readonly BillingInterval Monthly = new()
    {
        Id = "monthly",
        Title = new LocalizedText { Da = "Månedlig", En = "Monthly" },
        Description = new LocalizedText { Da = "Faktureres månedligt", En = "Billed monthly" }
    };

    public static readonly BillingInterval Yearly = new()
    {
        Id = "yearly",
        Title = new LocalizedText { Da = "Årlig", En = "Yearly" },
        Description = new LocalizedText { Da = "Faktureres årligt", En = "Billed yearly" }
    };

    public static IReadOnlyCollection<BillingInterval> All => [None, Monthly, Yearly];

    public static BillingInterval FromId(string id) =>
        All.FirstOrDefault(i => i.Id == id)
        ?? throw new ArgumentException($"Unknown billing interval: '{id}'");
}
