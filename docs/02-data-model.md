# 02 · Data Model

**Depends on:** `00-overview.md`, `01-architecture.md`. **Pairs with:** `03-accounts-and-permissions.md`, `04-tiers-and-features.md`.

Relational core (EF Core over PostgreSQL) with **structured columns for queryable fields** and **`jsonb` columns for flexible content payloads**. This hybrid avoids over-normalizing every section into its own table while keeping anything you filter/join on as a real column.

> Convention: all PKs are `uuid`. All tables carry `CreatedUtc` / `UpdatedUtc` (omitted below for brevity). Billing/money is modeled in §9.

---

## 1. Entity map

```
User (login)
   │  many-to-many via ProfileLogin (role)
   ▼
AthleteProfile ──1:1(draft)+1:1(published)──► SiteConfig ──► Theme
   │                                              │ Layout (jsonb: sections[])
   │ many-to-many over time                       │ references MediaAsset by id
   ▼ via Membership                               ▼
Organization (OrganizationType)              MediaAsset (blob/CDN ref)
   │  multi-user via OrganizationLogin (role)
   │  athlete slots (from active subscription)
   ▼
ClubPageConfig (draft/published) ── featuredAthletes references AthleteProfile (published contract only)

ChangeRequest (club proposal) ── TargetProfileId ──► AthleteProfile
   (lives outside draft/published; on approval merges into the athlete's DRAFT — §5b, 03 §3b)

Billing (§9) — subscriber is an AthleteProfile OR an Organization:
   Plan ──1:*──► PlanPrice          (interval × currency variants)
   Plan ──1:*──► PlanCapability     (Level 2 only)
   Subscription ──► PlanPrice       (recurring; sets denormalized tier fields)
   Purchase                         (one-time: photoshoots, video edits)
```

---

## 2. Identity & profile

### `User`
The login credential. One real person *may* hold one login used across roles, or distinct logins.

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| Email | varchar unique | |
| AuthProviderId | varchar | external IdP subject, if used |

### `AthleteProfile`
One profile = one athlete. The owned, B2C core.

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| Slug | varchar unique | URL identity, e.g. `maria-jensen`; reserved-word checked |
| DisplayName | varchar | |
| Sport | varchar | `judo` at launch |
| DateOfBirth | date | **source of truth** for minor/adult status |
| SelfTier | enum | **denormalized**, derived from the active `Subscription` (§9); not authoritative. The athlete's own tier — never overwritten by club perks. |
| DefaultLocale | enum | `da` / `en` (bilingual; see §7) |
| VisibilityState | enum | `Public` / `Private` — gates the public + club-showcase contract |

> **Minor status is computed, never stored.** `IsMinor` = `DateOfBirth` vs. current date, evaluated at request time. A stored boolean would go stale the day the athlete turns 18; the guardian-gating logic (`03`) must always recompute from `DateOfBirth`.

### `ProfileLogin` (join: User ↔ AthleteProfile, with role)
Implements **one profile + multiple linked roles** (`03`).

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| UserId | uuid FK | |
| AthleteProfileId | uuid FK | |
| Role | enum | `AthleteOwner`, `Guardian` |
| Permissions | jsonb | guardian edit/publish/approval config (`03`) |
| Status | enum | `Active` / `Revoked` |

> Unique constraint on (`UserId`, `AthleteProfileId`). A minor profile must have ≥1 active `Guardian` login.

---

## 3. Site configuration (the auto-generation engine)

### `SiteConfig`
Two rows per athlete recommended: one `Draft`, one `Published`. (For rollback/history on higher tiers, add `SiteConfigHistory` — see open questions in `07`.)

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| AthleteProfileId | uuid FK | |
| State | enum | `Draft` / `Published` |
| ThemeId | uuid FK | |
| ThemeVersion | int | pin to a theme version for render stability |
| Layout | jsonb | ordered sections + per-section data (§4) |
| GlobalSettings | jsonb | colors/fonts/accents — only slots the effective capability allows |
| Version | int | optimistic concurrency + cache key |
| PublishedUtc | timestamp null | |

### `Theme`

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| Name | varchar | |
| Version | int | |
| MinimumCapability | jsonb | the `FeatureKey`(s) that unlock it — same capability vocabulary as §9.3 (`04`) |
| Manifest | jsonb | supported section types, color/font slots, constraints — the render contract (`01` §4) |
| PreviewImageUrl | varchar | for the theme picker |
| IsActive | bool | |

### `MediaAsset`
Bytes live in blob/CDN; the DB holds the reference. **Owned by the athlete** even when capture was funded by a club (`04`).

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| AthleteProfileId | uuid FK | the athlete the media belongs to |
| Type | enum | `Image` / `Video` |
| Origin | enum | `SelfUpload` / `AdminUpload` / `ClubFundedShoot` |
| IsClubBranding | bool | if true, may revert to club on membership end (the narrow exception) |
| StorageKey | varchar | content-hashed blob/CDN key |
| Width / Height | int | responsive layout |
| AltText | varchar | accessibility + SEO |

> Default: media stays with the athlete on club exit. Only `IsClubBranding = true` assets are club-retained. Capture funded ≠ identity owned.

---

## 4. Organizations (B2B)

### `Organization`

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| Type | enum | `Club`, `NationalTeam`, `Academy`, `TrainingCenter`, `SchoolTeam` |
| Slug | varchar unique | |
| DisplayName | varchar | |
| IsServerManaged | bool | `true` for `NationalTeam` at launch (internal-admin only) |
| SubscriptionTier | enum | **denormalized**, derived from the active `Subscription` (§9); not authoritative. Null for server-managed. |
| AthleteSlotCount | int | **denormalized** from the active subscription's `Plan.AthleteSlotCount` (§9.1); not authoritative. The slot-limit check (§8) reads this cached value. |
| VisibilityState | enum | `Public` / `Private` |

### `OrganizationLogin` (join: User ↔ Organization, with role)
Multi-user access for clubs.

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| UserId | uuid FK | |
| OrganizationId | uuid FK | |
| Role | enum | `ClubAdmin`, `ClubEditor` (reserved later: `ClubViewer`, `Coach`, `Photographer`) |
| Status | enum | `Active` / `Revoked` |

Role capabilities are defined in `03`.

### `ClubPageConfig`
Mirrors the `SiteConfig` draft/published engine exactly (same columns), with org-specific section types (`clubHero`, `featuredAthletes`, `clubResults`, …).

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| OrganizationId | uuid FK | |
| State | enum | `Draft` / `Published` |
| ThemeId | uuid FK | |
| ThemeVersion | int | pin to a theme version for render stability (as `SiteConfig`) |
| Layout | jsonb | sections; `featuredAthletes` holds athlete **references**, not copies |
| GlobalSettings | jsonb | colors/fonts/accents |
| Version | int | optimistic concurrency + cache key |
| PublishedUtc | timestamp null | |

> The `featuredAthletes` section stores `AthleteProfile` ids only. Rendering resolves each against that athlete's **published public data contract** (`01` §4, `03`). No duplication; an athlete editing their profile updates everywhere it is shown.

---

## 5. Memberships (the affiliation graph)

Generic, time-bounded, many-to-many over time. Replaces any club-specific link table.

### `Membership`

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| AthleteProfileId | uuid FK | |
| OrganizationId | uuid FK | |
| Role | varchar | athlete's role within the org (e.g. `competitor`, `member`) |
| StartDate | date | |
| EndDate | date null | null = ongoing |
| Status | enum | `Active` / `Inactive` |
| OccupiesSlot | bool | true when consuming one of the org's athlete slots (`04`) |

**Derived "primary" contexts** (computed, not stored unless you cache them):

| Primary | Source | Rule |
|---------|--------|------|
| **Display primary** | active `Club` membership | at most **one active Club** at a time → drives club page placement + which **club perks apply** |
| **Prestige primary** | active `NationalTeam` membership | server-managed; surfaces as a **badge** on the club page; rich NT context stays on the NT entity, shown sparingly |
| **Training-context primary** | active `Academy`/`TrainingCenter` | optional |

**History is always retained.** Ending a membership sets `EndDate` + `Status = Inactive`; the row stays. This is what lets a club (and sponsors/recruiters) see who has passed through, while the athlete can move to another club and pick up that club's perks.

---

## 5b. Change requests (club → athlete proposals)

A club may **propose** edits to an affiliated athlete's profile but can never write to it directly. Each proposal is a `ChangeRequest` that lives **outside** both the athlete's draft and published `SiteConfig` until the profile side (guardian for a minor, athlete for an adult) resolves it. The full workflow and the two-gate model (approve → draft, then a separate publish) are in `03` §3b.

### `ChangeRequest`

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| TargetProfileId | uuid FK | the `AthleteProfile` the change is proposed for |
| ProposingOrganizationId | uuid FK | the org that proposed it |
| ProposedByUserId | uuid FK | the specific club user — audit trail |
| ProposedSections | jsonb | **snapshot** of the affected section(s) in proposed form (not a field-level patch) |
| Status | enum | `Pending` / `Approved` / `Rejected` / `Withdrawn` |
| ResolvedByUserId | uuid FK null | the guardian/athlete who approved or rejected |
| ResolvedUtc | timestamp null | |

> **Snapshot, not patch:** storing the proposed section(s) outright lets the approver see exactly what they are accepting; the "before" is the current draft at review time, so no separate before-snapshot is stored (it would only go stale). On **approval**, `ProposedSections` is merged into the athlete's **draft** — never straight to published. Publishing remains the athlete/guardian's separate action.
>
> `Withdrawn` lets a club retract a still-`Pending` proposal. `ProposedByUserId` / `ResolvedByUserId` record *which person* on each side acted — traceability matters for a workflow touching minors' public presence.

---

## 6. Perk resolution (additive layer)

Perks are **not** a column on the athlete that gets overwritten. Capability is **resolved at request time** by layering the active Club subscription's plan on top of the athlete's own plan. `SelfTier`/`SubscriptionTier` are just denormalized pointers to those plans (§9).

```
EffectiveCapability(feature) =
    max( capability_from(self plan, feature),         // plan behind the athlete's active Subscription; falls back to Free
         capability_from(active club plan, feature) ) // perks from the active Club subscription's plan
```

- No active club / club ends → the club layer evaporates; athlete falls back to their own plan.
- A club can never *lower* the athlete's own capability. Resolution is a per-feature OR/max, never a replace.
- Implementation lives in a **PerkResolver** service (`07`); persisted state never conflates the two sources.
- "Capability from plan" reads code-defined values keyed by `Plan.Key` (Level 1) or `PlanCapability` rows (Level 2) — see §9.3. Both use the same `FeatureKey` vocabulary.

Full feature→capability mapping is in `04`.

---

## 7. Bilingual content

Bake locale in now rather than retrofit. Two viable shapes — pick one and apply consistently:

- **Per-field locale maps** inside section `data`: `{ "headline": { "da": "...", "en": "..." } }`. Flexible, more client logic.
- **Locale-scoped section variants**: a `locale` key per section. Simpler rendering, more duplication.

Recommendation: **per-field locale maps** for short text fields; fall back to `AthleteProfile.DefaultLocale` when a translation is absent.

---

## 8. Validation & integrity rules (enforced server-side)

- Every save validates the `Layout` payload against each section type's schema **and** against the profile's **effective capability** (own plan + active club perks). Never trust client-claimed tier.
- Free-text fields are **sanitized** before publish (public XSS surface).
- Slug uniqueness + reserved words (`admin`, `api`, `about`, …) for both profiles and organizations.
- Minor/adult status is recomputed from `DateOfBirth` (never read from a stored flag); a minor `AthleteProfile` requires ≥1 active `Guardian` `ProfileLogin`.
- An organization cannot affiliate more slot-occupying athletes than its (denormalized) `AthleteSlotCount`.
- Optimistic concurrency via `Version` on configs.
- Club showcase resolution must read **published + public** athlete data only; a `Private`/unpublished athlete renders as a placeholder.
- **Change requests (§5b):** a `ChangeRequest` may only be created by a user with an active `OrganizationLogin` on the `ProposingOrganizationId`, and only against an athlete the org currently has an active `Membership` with. On `Approved`, `ProposedSections` is merged into the target's **draft** only — never the published config. Only the resolving authority (guardian for a minor, athlete for an adult, per `03` §3) may set `Approved`/`Rejected`; only the proposing org may set `Withdrawn`, and only while `Pending`.
- **Billing (§9):** `Subscription` and `Purchase` each reference **exactly one** subscriber — `AthleteProfileId` XOR `OrganizationId` (CHECK constraint). `Subscription.Status` and period fields are written only by the `BillingService` from provider webhooks, never inferred client-side. `Subscription.PlanVersion` is pinned at create time and never mutated in place.
- **Catalog edits never auto-apply to existing subscribers (§9.1):** `PlanPrice` is **append-only** (change a price by adding a new variant + retiring the old via `IsActive`), and `Plan` is edited **in place** with capabilities grandfathered via the pinned `Subscription.PlanVersion` + the code-defined `(Key, Version)` capability history. Reducing a limit/capability on an in-use plan is a deliberate migration, not a routine edit.

---

## 9. Billing, Subscriptions & Plans

> **Source-of-truth rule:** `AthleteProfile.SelfTier` and `Organization.SubscriptionTier` are **denormalized "current tier" read fields** for cheap lookups (the `PerkResolver` reads them). They are **derived, not authoritative** — updated whenever the account's active `Subscription` changes. Authoritative billing state lives in the tables below.

### 9.1 `Plan` — the tier catalog (identity only, no money)

The source of truth for *what tiers exist*. Holds **no pricing** — pricing lives in `PlanPrice` (§9.2) so the same plan can have multiple intervals/currencies without duplication. Code references plans by stable `Key`, never by row id.

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| Key | varchar unique | stable code identifier, e.g. `athlete_plus`, `club_pro` |
| Audience | enum | `Athlete` / `Organization` |
| DisplayName | varchar | |
| AthleteSlotCount | int null | org plans only; source for the denormalized `Organization.AthleteSlotCount` |
| Version | int | current capability generation; copied into `Subscription.PlanVersion` at subscribe time. See the grandfathering note below — optional until you actually revise live plans. |
| IsActive | bool | hide retired plans from signup without deleting (existing subscribers keep the plan) |

> **Level 1 (recommended now):** capabilities ("what Plus unlocks") stay in code, keyed by `Plan.Key`. This table plus `PlanPrice` enables runtime changes to **price, currency, interval, label, slot count, availability**.

> **Editing the catalog without tripping existing subscribers (the chosen approach):** catalog edits **never auto-apply** to anyone who has already subscribed. Two rules make that safe, and neither requires a schema change — both are enforced in `BillingService`:
>
> - **`PlanPrice` is append-only (§9.2):** to change a price, insert a new variant row and retire the old one with `IsActive = false`. Existing `Subscription`s keep pointing at their original `PlanPrice`, so their charge never moves.
> - **`Plan` is edited in place** — a single row per `Key`, `UPDATE` + bump `Version`. The *meaning* of each version (the capability set) lives in **code keyed by `(Key, Version)`**; a subscriber's pinned `Subscription.PlanVersion` resolves against that historical entry, so editing the plan does not change what they already have. **Do not delete an old version's entry from code while any subscription is still pinned to it.**
>
> **Consideration — what an in-place `Plan` edit loses:** an `UPDATE` overwrites the row's other columns (`DisplayName`, `AthleteSlotCount`, …) and the prior values are **not** retained in the DB. This is acceptable only because the version-sensitive ones are already preserved elsewhere — **capabilities** in the code map above, and **`AthleteSlotCount`** denormalized onto `Organization` at subscribe time (§4). It only bites if you **reduce** a limit or capability on a plan that has live subscribers; treat that as a deliberate, explicit migration — never a routine edit. If full row-level grandfathering ever becomes a real need, graduate to versioned `PlanCapability` rows (Level 2, §9.3) or an append-only `Plan` — do not build that machinery on spec.
>
> **On the `Version` columns:** because Level 1 keeps the version history in code, `Plan.Version` / `Subscription.PlanVersion` are *inert* until you first revise a live plan's capabilities **and** need old subscribers frozen. If launching free-only or single-tier, you may defer both and reintroduce them at the first breaking change (the cost is a one-time backfill of pins onto existing subscriptions). Keeping them from day 1 is cheap insurance against that backfill — the recommendation is to leave them in.

### 9.2 `PlanPrice` — one row per purchasable variant of a plan

Splitting price out of `Plan` removes the duplication that arises when the same plan is offered at, say, monthly **and** yearly, or in `DKK` **and** `EUR`. Each combination is one row; the plan identity stays single.

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| PlanId | uuid FK | → `Plan` |
| BillingInterval | enum | `Monthly` / `Yearly` / `None` |
| Price | decimal | null/0 for Free |
| Currency | varchar | e.g. `DKK`, `EUR` |
| IsActive | bool | retire a variant without touching the plan |

> No Stripe price reference is stored here. At checkout you pass amount + currency + interval to Stripe directly; `PlanPrice` already holds everything needed to construct the session. The Stripe Price object is effectively a mirror of this row, so a back-reference would be redundant.
>
> **Append-only — never mutate a `PlanPrice` row's `Price`.** To change a price, insert a new variant row and retire the old one via `IsActive = false`. Existing subscriptions keep referencing their original row (even when inactive), so a customer is never silently charged more or less than what they signed up for. `IsActive` only hides a variant from *new* signups; it does not evict existing subscribers. This mirrors Stripe, whose `Price` objects are themselves immutable.

### 9.3 `PlanCapability` — optional, enables runtime *feature* changes (Level 2)

Add only when you want non-devs to change what a tier unlocks. Feature keys are a shared vocabulary the code also knows.

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| PlanId | uuid FK | → `Plan` |
| FeatureKey | varchar | e.g. `themes.max`, `sections.video`, `analytics.level`, `video.embed`, `video.hosted`  |
| Value | varchar/jsonb | e.g. `8`, `true`, `full` |

> The **same `FeatureKey` vocabulary** powers the club perk layer (`04` §4) and theme gating (`Theme.MinimumCapability`, §3), so `PerkResolver` can compute `max(selfPlanCapabilities, activeClubPerkCapabilities)` per key uniformly.

### 9.4 `Subscription` — recurring billing entity (authoritative)

Subscriber is **either** an athlete **or** an organization: two nullable FKs + a check constraint (exactly one set). Cleaner than a polymorphic `SubscriberType` + `SubscriberId` because it keeps real foreign keys.

A subscription points at a specific `PlanPrice` (the exact variant purchased — e.g. `athlete_plus` / yearly / DKK), not just a `Plan`. The plan is reached via `PlanPrice` → `Plan`; `PlanVersion` is stored explicitly to freeze the plan version at subscribe time.

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| AthleteProfileId | uuid FK null | exactly one of these two non-null (CHECK) |
| OrganizationId | uuid FK null | |
| PlanPriceId | uuid FK | the exact variant subscribed to → `PlanPrice` |
| PlanVersion | int | **pin** the plan version at subscribe time → grandfathering |
| Status | enum | `Trialing` / `Active` / `PastDue` / `Canceled` / `Expired` |
| CurrentPeriodStart | timestamp | |
| CurrentPeriodEnd | timestamp | |
| CancelAtPeriodEnd | bool | |
| StripeCustomerId | varchar | Stripe customer ref (`cus_…`) — for billing portal / refunds |
| StripeSubscriptionId | varchar | Stripe subscription ref (`sub_…`) — to correlate incoming webhooks |

> **Grandfathering:** because `PlanVersion` is pinned, editing a plan (new version) does not silently change what existing subscribers have. They keep their pinned version until they explicitly move plans. Capability resolution reads the plan **at the subscription's pinned version**.
>
> **Provider naming:** fields are named `Stripe*` rather than `Provider*` because the MVP builds against Stripe specifically. If a second provider is ever added, rename then — that is a smaller cost than maintaining an unused abstraction now.

### 9.5 `Purchase` — one-time payments (authoritative)

Covers the one-time items the tier model promises (photoshoot bookings, video edits) that subscriptions don't represent. Same subscriber pattern.

| Column | Type | Notes |
|--------|------|-------|
| Id | uuid PK | |
| AthleteProfileId | uuid FK null | exactly one non-null (CHECK) |
| OrganizationId | uuid FK null | |
| ItemType | enum | `PhotoshootBooking`, `VideoEdit`, … |
| Status | enum | `Pending` / `Paid` / `Refunded` |
| Amount | decimal | |
| Currency | varchar | |
| StripePaymentId | varchar | Stripe payment / payment-intent ref |

### 9.6 How the billing pieces relate

```
Plan (identity, versioned)
   │ 1───* PlanPrice (interval × currency variants)
   │ 1───* PlanCapability (Level 2 only)
   │
   └──◄ Subscription.PlanPriceId ──► (pins PlanVersion)
            │ subscriber = athlete OR org (XOR)
            │ Stripe* refs; status driven by webhooks
            ▼
        sets (denormalized) ──► AthleteProfile.SelfTier
                                 Organization.SubscriptionTier  (+ AthleteSlotCount)

PerkResolver reads self-plan capabilities and active-club-plan capabilities,
then max() per FeatureKey  →  EffectiveCapability   (§6, `04` §1, `06`)

Purchase (one-time)  ──►  discrete services (photoshoot, video edit)
```

---

## Appendix · Downstream notes for other documents

These are cross-references, not part of the schema:

- **`04` §2 / §3:** the tier tables are the **human-readable view of `Plan` + `PlanPrice` rows**; placeholder `[…]` values become data. Capabilities are Level-1 (code) until `PlanCapability` is introduced.
- **`07` §1 (patterns):** add a `BillingService` consuming Stripe webhooks → updates `Subscription.Status` and `CurrentPeriod*`, and refreshes the denormalized `SelfTier` / `SubscriptionTier` / `AthleteSlotCount`. Webhooks are the source of truth for status transitions.
- **`07` §3 (build order):** insert between step 6 (self-tiers) and step 10 (perk layer):
  - **6a** — `Plan` + `PlanPrice` (Level 1) + signup reads plans.
  - **6b** — `Subscription` + `BillingService` + Stripe integration.
  - **6c** — `Purchase` for one-time bookings (pairs with media pipeline, step 7, and booking, step 13).
  - *(later)* `PlanCapability` (Level 2) only if runtime feature editing is wanted.
- **`07` §4 (open questions):** strike #2 (prices now live in `Plan`/`PlanPrice`); add: Level 1 vs Level 2 capabilities; payment provider confirmed as Stripe (consider MobilePay for the Danish parent-payer audience); proration / mid-period upgrade policy.

> If launching **free-only first**, ship `Plan` with a single Free row + its `PlanPrice` (or skip until paid tiers land) and defer `Subscription` / `Purchase` until billing goes live — the denormalized tier fields still work in the meantime.