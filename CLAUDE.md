# NextAtlet — Global Context (CLAUDE.md)

Auto-loaded by Claude Code. Contains every load-bearing architectural decision from the spec docs.
Read the numbered docs only when working on that specific area (links noted per section).

---

## Stack

| Layer | Choice |
|-------|--------|
| Backend | ASP.NET Core Web API + EF Core |
| DB | PostgreSQL (`jsonb` for section payloads) |
| Frontend | Next.js (editor = SPA-style; public site = SSR/ISR) |
| Media | Blob storage + CDN (Azure Blob / S3); refs only in DB |
| Auth | ASP.NET Core Identity or external IdP; **must support multiple linked logins per profile** |

---

## Non-negotiables (never change without a doc revision)

1. **Config-as-data.** Backend emits no HTML. Athlete/club sites are data + theme manifest; Next.js renders. (`01`, `02`)
2. **Additive perk layer, never replace.** `EffectiveCapability(feature) = max(SelfTier, ActiveClubPerks)`. A club can never lower an athlete's own tier. (`04`)
3. **Published public data contract is the only outbound athlete projection.** `ToPublicContract(profile)` is the single serialization path. Never drafts, never private fields. (`03`)
4. **One profile + linked roles; 18 = guardian boundary.** `IsMinor` is computed from `DateOfBirth` at request time — never stored as a flag. (`03`)
5. **Generic time-bounded memberships; history always retained.** Ending a membership sets `Inactive`; the row stays. (`02`)

---

## Identity & permissions model

- **`AthleteProfile`** — one per athlete; B2C core.
- **`ProfileLogin`** — join of `User ↔ AthleteProfile` with role (`AthleteOwner` | `Guardian`) + `Permissions` jsonb.
- Minor profile **must** have ≥1 active `Guardian` login at all times.
- **`OrganizationLogin`** — `User ↔ Organization` with role (`ClubAdmin` | `ClubEditor`; others reserved).
- All authorization expressed as composable **Specifications** (`CanEditContent`, `CanPublish`, `CanManageBilling`, etc.) — no scattered `if/role` checks.
- Guardian permission defaults: guardian holds `canPublish` + `canApproveChanges`; athlete may edit/propose, not publish.

---

## Read/write path split (CQRS-lite)

| Path | Data | Cache |
|------|------|-------|
| Editor (authenticated) | Full draft config + tier/perk schema for *this* profile | Never cached |
| Public (anonymous) | Published public contract only — sanitized, CDN-resolved media, theme manifest | ISR + CDN; invalidate on publish |

Club pages reference athletes by id; resolved at render against published contract. If athlete is `Private`/unpublished → graceful placeholder, never a leak or a break.

---

## Key entities (schema summary)

**`AthleteProfile`** — `Slug`, `DateOfBirth`, `SelfTier` (denormalized, not authoritative), `VisibilityState` (`Public`/`Private`).

**`SiteConfig`** — two rows per athlete (`Draft` + `Published`). Columns: `ThemeId`, `ThemeVersion`, `Layout` (jsonb: ordered sections), `GlobalSettings` (jsonb), `Version` (optimistic concurrency + cache key).

**`MediaAsset`** — `Origin` (`SelfUpload`/`AdminUpload`/`ClubFundedShoot`), `IsClubBranding`. Media stays with athlete on club exit; only `IsClubBranding = true` assets are club-retained.

**`Organization`** — `Type` enum: `Club | NationalTeam | Academy | TrainingCenter | SchoolTeam`. `IsServerManaged = true` for NationalTeam (internal-admin only). `SubscriptionTier` + `AthleteSlotCount` are denormalized.

**`Membership`** — `AthleteProfileId`, `OrganizationId`, `StartDate`, `EndDate`, `Status` (`Active`/`Inactive`), `OccupiesSlot`. At most **one active Club** at a time (display primary). NationalTeam = prestige badge only, not a perk source.

**`ChangeRequest`** — club → athlete proposal. `ProposedSections` (jsonb snapshot), `Status` (`Pending`/`Approved`/`Rejected`/`Withdrawn`). Approval merges into **draft only** (gate 1); athlete/guardian publishes separately (gate 2). Club can never write to a profile directly.

**Billing** — `Plan` (identity, `Key` is stable code ref) → `PlanPrice` (interval × currency, append-only) → `Subscription` (pins `PlanVersion`; `AthleteProfileId` XOR `OrganizationId`). `Purchase` for one-time items. `SelfTier`/`SubscriptionTier` are denormalized read-fields updated by `BillingService` on Stripe webhooks.

---

## Rendering contract

```
Layout.sections[]  +  Theme manifest  +  Resolved CDN media
        ↓
Next.js SectionComponentRegistry  →  rendered page
```

One React component per section type. Theme manifest declares supported section types + color/font slots. A theme that omits `video` hides it in the editor automatically.

---

## Core services

| Service | Responsibility |
|---------|---------------|
| **`PerkResolver`** | Computes `max(selfPlan, activeClubPlan)` per `FeatureKey` at request time. Never persists the conflated result. |
| **`PublicContractProjector`** | `ToPublicContract(profile)` — the **only** path athlete data leaves to public/club surface. |
| **`BillingService`** | Consumes Stripe webhooks → updates `Subscription.Status`, `CurrentPeriod*`, and refreshes denormalized `SelfTier`/`SubscriptionTier`/`AthleteSlotCount`. |

---

## Design patterns in use

`Strategy` (section validation/rendering) · `Factory/Registry` (`SectionTypeRegistry`) · `Specification` (tier/perk/role gating) · `CQRS-lite` (editor vs public path) · `Builder` (public render payload) · `Decorator/Middleware` (caching + sanitization on public path) · `DTO + mapping` (never expose EF entities) · `Options pattern` (tier/perk/theme config).

---

## Validation rules (server-side, always)

- Section payloads validated against section schema **and** effective capability — never trust client-claimed tier.
- Free-text sanitized before publish (XSS surface).
- `IsMinor` recomputed from `DateOfBirth` every request.
- Slug uniqueness + reserved-word check (`admin`, `api`, `about`, …).
- Org cannot affiliate more slot-occupying athletes than `AthleteSlotCount`.
- `ChangeRequest` only creatable by a user with active `OrganizationLogin` on an org that has an active `Membership` with the target athlete.
- `PlanPrice` is append-only — never mutate a price row; insert new + retire old with `IsActive = false`.
- Club showcase resolution reads **published + public** only; `Private`/unpublished = placeholder.

---

## Athlete tiers (B2C) — structure only, numbers TBD

`Free` → `Plus` → `Pro`. Tier gates: which sections are editable + which themes are selectable. Pricing: recurring subscription + one-time add-ons (photoshoots, video edits) coexist.

## Club subscriptions (B2B) — structure only, numbers TBD

`Club Free` → `Club Plus` → `Club Pro`. Grants athlete slots + perk layer. Athlete must have their own profile before affiliation. One active Club membership at a time.

---

## MVP boundaries

- Sport: **judo only** (`Sport` field generalizes later).
- Geography: **Denmark**; bilingual `da`/`en` from day one (per-field locale maps inside section data, fallback to `DefaultLocale`).
- NationalTeam: **internal-admin created/assigned only** — no self-serve.
- Sponsor marketplace: **out of MVP** (data model leaves room).
- No free-form custom HTML sections (XSS + quality — hard boundary).

---

## Build order (abbreviated)

1. Profiles + SiteConfig + auth + guardian linking
2. Public render endpoint + Next.js renderer → **first real milestone**
3. Publish flow + ISR/CDN + cache invalidation
4. Section registry + Strategy validators; add `results`, `gallery`
5. Theme manifest system + theme picker
6. Athlete self-tiers + Specifications + billing (`Plan`/`PlanPrice`/`Subscription`)
7. Media pipeline (upload → blob → thumbnails)
8. Organizations + multi-user roles + club page engine
9. Memberships + derived primaries + history retention
10. `PerkResolver` + additive perk layer + club subscriptions
11. Club showcases via published-contract references
12. Change-request / approval workflow
13. Mentoring content + photoshoot booking
14. Versioning/history (higher tiers)
15. Bilingual polish + custom subdomains (last — DNS/cert complexity)

---

## Open decisions (do not assume resolved)

| # | Question | Blocks step |
|---|----------|-------------|
| 1 | One-row-per-state vs `SiteConfigHistory` for rollback? | 3 / 14 |
| 2 | Final tier prices + slot/session counts? | 6 / 10 |
| 3 | Club downgrade below roster size — block or mark overflow inactive? | 10 |
| 4 | Academy/TrainingCenter/SchoolTeam: self-serve vs admin-created? | 8 |
| 5 | Level 1 (code) vs Level 2 (`PlanCapability` rows) for feature gating? | 6 |
| 6 | MobilePay support alongside Stripe (Danish parent-payer audience)? | 6b |
| 7 | Proration / mid-period upgrade policy? | 6b |
| 8 | Data retention if athlete deletes profile while club-affiliated? | 8 / 11 |
| 9 | Photoshoot scheduling system (photographer calendars, locations)? | 13 |

---

## Where to read more

| Topic | Doc |
|-------|-----|
| Full rendering contract + caching strategy | `01-architecture.md` |
| Complete schema (all columns, constraints, billing tables) | `02-data-model.md` |
| Guardian permissions, change-request workflow, privacy boundary | `03-accounts-and-permissions.md` |
| Tier/perk tables, edge cases, perk layer contents | `04-tiers-and-features.md` |
| Signup flows, onboarding checklist, club signup | `05-signup-and-onboarding.md` |
| Feature → problem → why this design | `06-features-and-problems.md` |
| All design patterns, full build order, open questions | `07-patterns-and-build-order.md` |