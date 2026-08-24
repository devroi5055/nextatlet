# 04 · Tiers & Features

> **Status: NOT IMPLEMENTED.** This document describes intent only. There is **no billing, no plan table, no capability resolution, and no Stripe** in the codebase. Tiers exist solely as descriptive Domain enumerations. Read this as a design sketch, not a description of working code.

## What actually exists in code

- Six **Billing enumeration** classes in [`NextAtlet.Domain/Enumerations/Billing/`](../apps/NextAtlet.Server/NextAtlet.Domain/Enumerations/Billing): `AthleteTier`, `OrganizationTier`, `BillingInterval`, `PlanAudience`, `PurchaseStatus`, `SubscriptionStatus`. These are just id + bilingual description text.
- **Only `OrganizationTier.Free.Id` is ever used** (as a DB default). Every other billing enumeration has zero references outside Domain.
- `IndividualProfile.SelfTierId` and `OrganizationProfile.OrganizationTierId` are columns, but `SelfTierId` is **never written**.
- [`PerkResolver`](../apps/NextAtlet.Server/NextAtlet.Application/Features/Capabilities/PerkResolver.cs) and [`ResolveCapabilitiesCommand`](../apps/NextAtlet.Server/NextAtlet.Application/Features/Capabilities/ResolveCapabilitiesCommand.cs) are **100% commented out**.
- `PlanCapabilities.cs` and `ThemeCapabilityRequirement` are dead/commented.

## Intended design (for reference, not built)

### Athlete tiers (B2C)

`Free` → `Plus` → `Pro`. Intended to gate which sections are editable and which themes are selectable. `AthleteTier` descriptions sketch: Free = simple public page; Plus = extended customization, gallery, mentoring guides, photoshoot discounts; Pro = full customization, video, 1:1 mentoring, included photoshoots.

### Club subscriptions (B2B)

`Club Free` → `Club Plus` → `Club Pro`. Intended to grant athlete slots + a perk layer. `OrganizationTier` descriptions sketch: Free = showcase page, limited slots; Plus = extended slots, analytics; Pro = max slots, recruitment dashboards, funded photoshoots.

### The additive perk layer (design principle)

The intended rule was `EffectiveCapability(feature) = max(SelfTier, ActiveClubPerks)` — a club can never *lower* an athlete's own tier, only add. This is unimplemented (`PerkResolver` is commented out and depends on `ISubscriptionRepository`/`IMembershipRepository`, which don't exist).

## What would need to be built

1. `Plan` / `PlanPrice` (append-only) / `Subscription` / `Purchase` entities + migrations.
2. A billing service (Stripe webhooks → denormalized tier fields). MobilePay would run *through* Stripe.
3. `PerkResolver` + a `FeatureKeys` catalog + membership queries.
4. Wiring tier/perk checks into the (also-unbuilt) draft-edit and theme-picker paths.

See the feature status board in [`06-features-and-problems.md`](06-features-and-problems.md).
