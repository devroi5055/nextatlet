namespace NextAtlet.Domain.Enumerations.Enums.Billing
{
    /// <summary>Driven by Stripe webhooks. Internal billing state machine.</summary>
    public enum SubscriptionStatus
    {
        Trialing,
        Active,
        PastDue,
        Canceled,
        Expired
    }
}
