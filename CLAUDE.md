# NextAtlet — Global Context (CLAUDE.md)

Auto-loaded by Claude Code. Contains every load-bearing architectural decision from the spec docs.
Read the numbered docs only when working on that specific area (links noted per section).

---

## Stack

| Layer | Choice |
|-------|--------|
| Backend | ASP.NET Core Web API + EF Core (`NextAtletDbContext`), .NET 10 |
| DB | PostgreSQL (`jsonb` for section payloads + value objects) |
| Frontend | Next.js (App Router) — `@auth0/nextjs-auth0`, React Query, Zustand, Tailwind v4, Radix |
| Media | Blob storage + CDN (Azure Blob / S3); refs only in DB — *pipeline not built yet* |
| Auth | **Auth0 (OIDC)**, dual-scheme (JWT bearer + cookie); supports **multiple linked logins per site** |

---

## Implementation status (what's actually built)

The numbered docs are a forward-looking **spec**; much is designed but not yet built. Current reality:

**Built:** Auth0 dual-scheme auth + just-in-time `UserProvisioner`; the two-gate registration (self + guardian + org/club) via MediatR/CQRS over repositories + `IUnitOfWork`; `Site`/`IndividualProfile`/`OrganizationProfile`/`SiteSnapshot` schema; `SiteLogin` multi-login; `ActionToken` flow (invite / consent / org-email-verification) with a strategy registry; `GuardianConsent` (GDPR audit); control model (`PermissionResolver` + `ControlMode`, transfer-control + collaboration); `GET /api/Me` decision gate; `GET /api/sites` public paged listing; club registry (scrape Danish clubs + officials, CVR lookup); error-codes pipeline (`Result<T>` + `DomainException` → `ApiError`); dev data seeder. Frontend: marketing landing page, the onboarding wizard (self/guardian/complete), Auth0 proxy + decision gate.

**Not built yet (designed only):** publish flow + public **render** contract / `PublicContractProjector`; section validation strategies wired to the editor; theme picker; tier gating; **all billing** (`Plan`/`PlanPrice`/`Subscription`/`Purchase`, Stripe, `BillingService`); `PerkResolver` (stub only); media pipeline; **memberships/affiliation** (entity scaffold, no commands); **change-request workflow** (entity scaffold, no commands); mentoring; versioning/history; subdomains. `Membership` and `ChangeRequest` entities exist but diverge from the doc schemas below and have no handlers.

---

## Non-negotiables (never change without a doc revision)

1. **Config-as-data.** Backend emits no HTML. Athlete/club sites are data + theme manifest; Next.js renders. (`01`, `02`)
2. **Additive perk layer, never replace.** `EffectiveCapability(feature) = max(SelfTier, ActiveClubPerks)`. A club can never lower an athlete's own tier. (`04`)
3. **Published public data contract is the only outbound athlete projection.** `ToPublicContract(profile)` is the single serialization path. Never drafts, never private fields. (`03`)
4. **One profile + linked roles; 18 = guardian boundary.** `IsMinor` is computed from `DateOfBirth` at request time — never stored as a flag. (`03`)
5. **Generic time-bounded memberships; history always retained.** Ending a membership sets `Inactive`; the row stays. (`02`)

---

## Identity & permissions model

- **`Site`** — shared identity envelope (`SiteTypeId`: `individual` | `organization`); holds `Slug`, `DisplayName`, `VisibilityStateId`, `VerificationStatusId`, `DefaultLocaleId`, and FKs to current draft/published `SiteSnapshot`.
- **`IndividualProfile`** — per-athlete metadata (1:1 with an individual `Site`); B2C core. (Was `AthleteProfile`.)
- **`SiteLogin`** — join of `User ↔ Site` with `SiteRoleId` + optional `Permissions` jsonb (`LoginPermissions`). Individual roles: `owner` | `guardian`. Organization roles: `club_admin` | `club_editor`. (Was `ProfileLogin` / `OrganizationLogin` — now one unified table.) A site may carry {Owner+Guardian}, {Owner only}, or {Guardian only}.
- **`User.AuthProviderId` is nullable** in schema, but Users are **provisioned just-in-time** on first authentication (`UserProvisioner`, matched by `sub` only) — a pending invite is an `ActionToken` row, never a ghost user. `IsClaimed` (computed) once a `sub` is linked.
- Minor profile gets its guardian **atomically**: in the guardian-creates-child flow the caller is attached as an Active `guardian` `SiteLogin` by construction; in the self-minor flow (<16) a consent **`ActionToken`** is issued + emailed (no login until the guardian accepts). Publishing a minor needs `ConsentStateId != pending_guardian_consent`.
- **Two registration commands** (caller → `owner` vs caller → `guardian`) share `IndividualSiteRegistrationHandlerBase.CreateIndividualProfileCoreAsync`; identity comes from token claims (`User.GetAuthProviderId()`/`GetEmail()`), never the body. See `docs/03` §1, `docs/05`.
- **Authorization is `PermissionResolver` (domain), not Specifications.** `PermissionResolver.Resolve(SiteLogin, IndividualProfile)` returns a `SitePermissions` preset (`CanEditContent`/`CanPublish`/`CanApproveChanges`/`CanManageMedia`/`CanManageMemberships`) from the profile's **`ControlMode`** + the login's role. Handlers also do natural "caller must hold an active login on this site" checks. There are **no** `CanEditContent`/`CanManageBilling` Specification classes (despite older doc text).
- **`ControlMode`** (stored, explicit, never age-derived): `athlete_controlled` | `guardian_controlled` | `*_shared`. `transfer-control` and `collaboration` (shared editing) endpoints flip it. Consent gate is the orthogonal **`ConsentState`** (`not_required` | `pending_guardian_consent` | `consented`).
- **Auth = Auth0 (OIDC), dual-scheme:** JWT bearer (Swagger/services) + cookie (`nextatlet.session`, Next.js) behind a `smart` policy scheme; authenticated-by-default fallback policy; `[AllowAnonymous]` to opt out. See `docs/07` (Authentication).

---

## Read/write path split

| Path | Data | Cache |
|------|------|-------|
| Editor (authenticated) | Full draft config + tier/perk schema for *this* profile | Never cached |
| Public (anonymous) | Published public contract only — sanitized, CDN-resolved media, theme manifest | ISR + CDN; invalidate on publish |

The editor write path is implemented as **CQRS via MediatR**: controllers dispatch `IRequest` commands/queries through `ISender`; handlers orchestrate repositories + domain services and commit once via `IUnitOfWork`. The editor-vs-public separation above is orthogonal — it governs caching and the public contract, not the dispatch mechanism.

## Layering & dependency direction

```
Api ──► Application ◄── Infrastructure
          │                  │
          └──────► Domain ◄──┘
```

- **Application** owns the contracts: MediatR `IRequest`/handlers, repository interfaces, `IUnitOfWork`, `ISanitizationService`/`ISectionTypeRegistry`. No EF here.
- **Infrastructure** implements them over EF Core (`NextAtletDbContext`, repositories, `EfUnitOfWork`) and references Application.
- Handlers are **orchestrators**: they never touch `DbContext` — they read/write via repository interfaces and call `IUnitOfWork.SaveChangesAsync()` once.
- **MediatR** is pinned to v13 Community (free under the org-revenue threshold). Handler/`IRequest` code is portable to MediatR 12 (MIT) or a hand-rolled `ISender` if licensing policy changes — see `docs/08`.

## Error handling (Model A — error codes)

Backend emits **error codes, never localized strings**; the frontend resolves `da`/`en`. Two boundary mechanisms produce the same `ApiError { errorCode, parameters }` shape: handlers return `Result<T>` (unwrapped by a global `ResultFilter`) or throw `DomainException(code)`; both map to a **specific 4xx** (the `ErrorCodes` catalog now carries fine-grained 400/403/404/409/422 semantics — no longer a coarse "400 for everything"). System failures hit `GlobalExceptionHandler` → generic `500` `internal_error` (no leak). Don't classify a system/seed failure (e.g. missing Classic theme) as a `DomainException`. Details: `docs/01` (contract), `docs/07`.

Club pages reference athletes by id; resolved at render against published contract. If athlete is `Private`/unpublished → graceful placeholder, never a leak or a break.

---

## Key entities (schema summary)

Enumerations are stored as **string IDs** (`*Id` columns, e.g. `ControlModeId`, `SportId`); the value-object base carries a bilingual `LocalizedText` Title/Description.

**`Site`** — `Slug`, `DisplayName`, `SiteTypeId`, `VisibilityStateId` (`public`/`private`), `VerificationStatusId`, `DefaultLocaleId`, `CurrentDraftSnapshotId`, `CurrentPublishedSnapshotId`.

**`IndividualProfile`** (was `AthleteProfile`) — `SiteId`, `SportId` (`judo`), `DateOfBirth` (`DateOnly`), `ControlModeId`, `ConsentStateId`, `SelfTierId` (denormalized, **null until billing**). `IsMinor(now)` computed. (Code TODOs flag `ConsentStateId`/`SelfTierId` as possibly-removable.)

**`SiteSnapshot`** (was `SiteConfig`) — **immutable** (`CreatedOnly`). Columns: `SiteId`, `ThemeId`, `Layout` (jsonb sections), `GlobalSettings` (jsonb), `PublishedUtc`. Draft vs published = which FK on `Site` points at it. **No `Version` / `ThemeVersion` column** (optimistic-concurrency `Version` was removed; the draft-edit endpoint that used it is currently disabled).

**`Theme`** — `Name`, `Manifest` (jsonb `ThemeManifest`: supported sections + slots), `PreviewImageUrl`, `RetiredUtc` (`IRetirable`). No `Version`/`MinimumCapability`/`IsActive` columns.

**`MediaAsset`** *(schema only; no pipeline yet)* — XOR owner `AthleteSiteId` | `OrganizationId`, `TypeId`, `OriginId` (`self_upload`/`admin_upload`/`club_funded_shoot`/`organization_upload`), `IsClubBranding`, `StorageKey`.

**`OrganizationProfile`** (was `Organization`) — `SiteId`, `OrganizationTypeId` (`club`/`national_team`/`academy`/`training_center`/`school_team`), `IsServerManaged`, `AthleteSlotCount`, `OrganizationTierId`, `VerificationStatusId` (+ `OrgVerification` value object).

**`ActionToken`** — single-use expiring token (the row `Id` IS the link key). `TypeId` (`invitation`/`consent`/`org_email_verification`), `TargetSiteId`, `ExpiresUtc`, `AcceptedUtc`, polymorphic `Payload`. Accepted via `POST /api/action-tokens/{id}/accept` → strategy registry.

**`GuardianConsent`** — immutable GDPR Art. 8 audit (`SiteId`, `GuardianUserId`, `MethodId`, `TermsVersion`, `CreatedUtc`).

**`Club` / `ClubOfficial`** — imported club registry (DJU scrape + CVR lookup) that backs org email-verification. Not in the older docs.

**`Membership`** *(scaffold; no commands)* — `IndividualProfileId`, `OrganizationId`, `RoleId`, `EndDate`, `statusId`, `OccupiesSlot`. Intended: one active Club at a time (display primary); NationalTeam = badge only.

**`ChangeRequest`** *(scaffold; no workflow commands)* — current fields: `TargetProfileId`, `ProposingOrganizationId`, `ProposedByUserId`, `ProposedLayout` (jsonb), `Theme`, `ThemeVersion`, `IsActive`. The designed `Status`/`ResolvedBy`/two-gate approval (docs `02`/`03`) is **not implemented**.

**Billing** *(designed, not built — no tables, no Stripe)* — planned `Plan` → `PlanPrice` (append-only) → `Subscription` (XOR subscriber) + `Purchase`. Tiers currently exist only as the `AthleteTier`/`OrganizationTier` enumerations behind the denormalized `SelfTierId`/`OrganizationTierId` fields.

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

| Service | Status | Responsibility |
|---------|--------|---------------|
| **`PermissionResolver`** (domain) | **built** | `Resolve(SiteLogin, IndividualProfile) → SitePermissions` from `ControlMode` + role. The actual authorization chokepoint. |
| **`UserProvisioner`** | **built** | Just-in-time `User` get-or-create on first auth, matched by `sub`. |
| **`ActionTokenStrategyRegistry`** + strategies | **built** | `ConsentStrategy` / `InvitationStrategy` / `OrgEmailVerificationStrategy`, dispatched by token type on accept. |
| **`SectionTypeRegistry`**, **`SanitizationService`** | **built** | Section schema validation + free-text sanitization (defined; not yet wired to a draft-edit endpoint). |
| Club registry (`DjuPortalScraper`, `ClubCanonicalizer`, CVR lookup) | **built** | Source Danish clubs + officials for org verification. |
| **`PerkResolver`** | **planned (stub)** | Would compute `max(selfPlan, activeClubPlan)` per `FeatureKey`. Commented-out stub today. |
| **`PublicContractProjector`** | **planned** | `ToPublicContract(profile)` — the intended single outbound projection. Not built. |
| **`BillingService`** | **planned** | Stripe webhooks → `Subscription` status + denormalized tier fields. No billing exists yet. |

---

## Design patterns in use

`Strategy` (`IActionTokenStrategy` per token type; section validators) · `Factory/Registry` (`SectionTypeRegistry`, `ActionTokenStrategyRegistry`) · `CQRS` via MediatR (`IRequest`/`IRequestHandler`, dispatched via `ISender`) · `Repository` + `Unit of Work` (DB access; intention-revealing interfaces in Application, EF impls in Infrastructure) · `Result<T>` + global filter/handler for errors · read/write path split (editor vs public) · `DTO + mapping` (static mappers; never expose EF entities) · `Options pattern` (`AgeThresholdOptions`, `TermsOptions`, `InvitationOptions`, `EmailOptions`). **Authorization uses a `PermissionResolver` + `ControlMode` presets — not the Specification pattern** the earlier docs describe. `Builder` (public render payload) and the public-path `Decorator/Middleware` are planned with the public render path.

---

## Validation rules (server-side, always)

- Section payloads validated against section schema **and** effective capability — never trust client-claimed tier.
- Free-text sanitized before publish (XSS surface).
- `IsMinor` recomputed from `DateOfBirth` every request.
- Slug uniqueness + reserved-word check (`admin`, `api`, `about`, …).
- *(planned)* Org cannot affiliate more slot-occupying athletes than `AthleteSlotCount`.
- *(planned)* `ChangeRequest` only creatable by a user with an active org `SiteLogin` (`club_admin`/`club_editor`) on an org with an active `Membership` to the target athlete.
- *(planned, billing)* `PlanPrice` is append-only — never mutate a price row; insert new + retire old with `IsActive = false`.
- *(planned, public render)* Club showcase resolution reads **published + public** only; `Private`/unpublished = placeholder.

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

1. Profiles + `Site`/`SiteSnapshot` + auth + guardian linking ✅ *(done)*
2. Public render endpoint + Next.js renderer → **first real milestone** *(next)*
3. Publish flow + ISR/CDN + cache invalidation
4. Section registry + Strategy validators; add `results`, `gallery`
5. Theme manifest system + theme picker
6. Athlete self-tiers + tier gating + billing (`Plan`/`PlanPrice`/`Subscription`) — *not built*
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
| 1 | One-row-per-state vs `SiteSnapshotHistory` for rollback? | 3 / 14 |
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
| Vision, business model, glossary | `00-overview.md` |
| Full rendering contract + caching strategy | `01-architecture.md` |
| Complete schema (all columns, constraints, billing tables) | `02-data-model.md` |
| Guardian permissions, change-request workflow, privacy boundary | `03-accounts-and-permissions.md` |
| Tier/perk tables, edge cases, perk layer contents | `04-tiers-and-features.md` |
| Signup flows, onboarding checklist, club signup | `05-signup-and-onboarding.md` |
| Feature → problem → why this design | `06-features-and-problems.md` |
| All design patterns, full build order, open questions | `07-patterns-and-build-order.md` |
| ADR — CQRS/MediatR, repositories, layering | `08-adr-cqrs-mediatr-and-layering.md` |