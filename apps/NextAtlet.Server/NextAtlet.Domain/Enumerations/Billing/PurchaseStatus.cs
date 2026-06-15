using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Billing;

/// <summary>Internal payment state.</summary>
public sealed class PurchaseStatus : Enumeration
{
    public static readonly PurchaseStatus Pending = new()
    {
        Id = "pending",
        Title = new LocalizedText { Da = "Afventer", En = "Pending" },
        Description = new LocalizedText { Da = "Betaling afventer behandling", En = "Payment pending processing" }
    };

    public static readonly PurchaseStatus Paid = new()
    {
        Id = "paid",
        Title = new LocalizedText { Da = "Betalt", En = "Paid" },
        Description = new LocalizedText { Da = "Betaling gennemført", En = "Payment completed" }
    };

    public static readonly PurchaseStatus Refunded = new()
    {
        Id = "refunded",
        Title = new LocalizedText { Da = "Refunderet", En = "Refunded" },
        Description = new LocalizedText { Da = "Betaling er refunderet", En = "Payment has been refunded" }
    };

    public static IReadOnlyCollection<PurchaseStatus> All => [Pending, Paid, Refunded];

    public static PurchaseStatus FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown purchase status: '{id}'");
}
