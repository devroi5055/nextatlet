using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Billing;

/// <summary>Driven by Stripe webhooks. Internal billing state machine.</summary>
public sealed class SubscriptionStatus : Enumeration
{
    public static readonly SubscriptionStatus Trialing = new()
    {
        Id = "trialing",
        Title = new LocalizedText { Da = "Prøveperiode", En = "Trialing" },
        Description = new LocalizedText { Da = "Aktiv prøveperiode", En = "Active trial period" }
    };

    public static readonly SubscriptionStatus Active = new()
    {
        Id = "active",
        Title = new LocalizedText { Da = "Aktiv", En = "Active" },
        Description = new LocalizedText { Da = "Abonnement er aktivt og i god stand", En = "Subscription is active and in good standing" }
    };

    public static readonly SubscriptionStatus PastDue = new()
    {
        Id = "past_due",
        Title = new LocalizedText { Da = "Forfald", En = "Past due" },
        Description = new LocalizedText { Da = "Betaling er forfalden", En = "Payment is past due" }
    };

    public static readonly SubscriptionStatus Canceled = new()
    {
        Id = "canceled",
        Title = new LocalizedText { Da = "Annulleret", En = "Canceled" },
        Description = new LocalizedText { Da = "Abonnement er annulleret", En = "Subscription has been canceled" }
    };

    public static readonly SubscriptionStatus Expired = new()
    {
        Id = "expired",
        Title = new LocalizedText { Da = "Udløbet", En = "Expired" },
        Description = new LocalizedText { Da = "Abonnement er udløbet", En = "Subscription has expired" }
    };

    public static IReadOnlyCollection<SubscriptionStatus> All => [Trialing, Active, PastDue, Canceled, Expired];

    public static SubscriptionStatus FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown subscription status: '{id}'");
}
