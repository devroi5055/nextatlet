# NextAtlet — Global Context (CLAUDE.md)

Auto-loaded context for AI assistants. This file describes the codebase **as it actually is today**, verified against source (not the aspirational spec). When in doubt, read the code — and prefer the onboarding docs under [`docs/confluence/`](docs/confluence/README.md), which are the freshest reference.

---

## What NextAtlet is

A platform where **judo athletes in Denmark build a public profile website** to attract sponsors. Audiences: athletes, their guardians (for minors), and clubs. MVP scope: judo only, Denmark only, bilingual `da`/`en`.

**Core idea — config-as-data:** the backend emits no HTML. A site is stored as structured content (typed sections) + a theme manifest; the Next.js frontend renders it.

---

## Stack (verified versions)

| Layer | Choice |
|-------|--------|
| Backend runtime | **.NET 10** (`net10.0`), solution file `NextAtlet.slnx` |
| API | ASP.NET Core Web API + **MediatR 13** (CQRS-lite) |
| ORM / DB | **EF Core 9** + Npgsql / **PostgreSQL** (jsonb for section + theme payloads) |
| Auth | **Auth0** (OIDC); JWT bearer + a vestigial cookie scheme |
| Email | **Resend** (falls back to a logging stub when no API key) |
| Frontend | **Next.js 16** + **React 19**, App Router; `@auth0/nextjs-auth0` v4; React Query; **Zustand** (one store); **Tailwind v4**; **next-intl v4** |

There is **NO** Stripe/billing, no media pipeline, no public render endpoint yet (see "Not built").

---

## Repo layout

```
apps/NextAtlet.Server/
  NextAtlet.Api/            Controllers, Program.cs, filters, GlobalExceptionHandler, auth, dev seeder
  NextAtlet.Application/    MediatR commands/queries, DTOs, repository INTERFACES, Result<T>, options (no EF)
  NextAtlet.Domain/         Entities, smart-enumerations, value objects, PermissionResolver, AgePolicy
  NextAtlet.Infrastructure/ EF Core (NextAtletDbContext), repositories, external services, migrations
apps/NextAtlet.Client/      Next.js frontend (src/proxy.ts is the middleware entry — NOT middleware.ts)
tests/                      Only NextAtlet.Application.Tests has source (~113 xUnit facts)
docs/                       docs/00–08 (updated) + docs/confluence/ (onboarding docs)
infra/                      EMPTY (no IaC)
```

Dependency direction: `Api → Application ← Infrastructure`, both `→ Domain`. Domain references nothing but the framework.

---

## Architectural non-negotiables

1. **Config-as-data.** Backend emits no HTML; sites are data + theme manifest.
2. **CQRS-lite via MediatR.** Every write is a command, every read a query — records implementing `IRequest<T>`; handlers live beside them under `Features/**`. Controllers only `_sender.Send(...)`. **No MediatR pipeline behaviours exist** — validation is hand-rolled in each handler.
3. **Handlers are orchestrators.** They never touch `DbContext`; they use repository interfaces and call `IUnitOfWork.SaveChangesAsync()` **once**. There is no explicit transaction API — atomicity is EF's implicit single-`SaveChanges` transaction.
4. **Identity comes from token claims**, never the request body (`User.GetAuthProviderId()` / `GetEmail()`).
5. **Errors are codes, not strings.** Handlers return `Result<T>`; `ResultFilter` maps success→200/204 and **every failure→400** with `ApiError { errorCode, parameters }`. Unhandled exceptions→500 `internal_error`. (`ApiError.Parameters` is always empty; the 403/404/409/422 groupings in `ErrorCodes.cs` are comments only.)
6. **One profile + linked roles; 18 = guardian boundary.** Minor status is computed from `DateOfBirth`, never stored.

---

## Identity & permissions model

- **`Site`** — the publishable unit. `Slug` (globally unique), `DisplayName`, `SiteTypeId` (`individual`|`organization`), `VisibilityStateId`, `DefaultLocaleId`, FKs to current draft/published `SiteSnapshot`.
- **`IndividualProfile`** — athlete data (sport, DOB, `ControlModeId`, `ConsentStateId`). Links to a Site via a bare `SiteId` **with no FK/index**.
- **`OrganizationProfile`** — club data (slot count, tier, verification). Also a bare `SiteId`, no FK.
- **`SiteSnapshot`** — a content version: jsonb `Layout` (sections) + `GlobalSettings` + `ThemeId`. Site points at current draft + published.
- **`SiteLogin`** — User × Site × role (`owner`|`guardian`) × status. One row per (user, site). The `Permissions` jsonb column is dead (always null).
- **`User`** — Auth0 identity, provisioned just-in-time by `UserProvisioner` (matched by `sub` only). No password.
- **`ActionToken`** — single-use expiring token; the row GUID IS the link secret. Types: `invitation` / `consent` / `org_email_verification`, dispatched by a strategy registry on accept.
- **`GuardianConsent`** — GDPR Art. 8 audit row.
- **`Club` / `ClubOfficial`** — scraped read-only registry (DJU portal) backing org email verification. NOT the same as `OrganizationProfile`.

**Authorization** = `PermissionResolver.Resolve(SiteLogin, IndividualProfile)` (Domain) → a `SitePermissions` preset, derived from `ControlModeId` × role. Nothing stored. `ControlModes`: `athlete_controlled` | `guardian_controlled` | `*_shared`. Consent gate (`ConsentStates`) is orthogonal: `not_required` | `pending_guardian_consent` | `consented`.

---

## Data model notes

- Enumerations are the **smart-enumeration pattern** (string `Id` + bilingual `LocalizedText`), stored as `varchar`. **No enum id is enforced by the DB** — no lookup tables, FKs, or CHECKs.
- JSONB value objects (`SiteLayout`, `ThemeManifest`, `ActionTokenPayload`, `GlobalSettings`) are stored via a converter that makes EF see them as opaque `string` — **you cannot LINQ into them**; code that filters by contents materialises rows and filters in memory.
- One seed: the "Classic" `Theme` (`11111111-1111-1111-1111-111111111111`). **Registration fails without it** ("Classic theme not found").
- Full schema: [`docs/02-data-model.md`](docs/02-data-model.md) and [`ERD.mmd`](ERD.mmd).

---

## Commands & endpoints (what's actually wired)

| Endpoint | Handler |
|----------|---------|
| `POST /api/IndividualSites/self-register` | `RegisterIndividualSiteSelfCommand` |
| `POST /api/IndividualSites/guardian-register` | `RegisterIndividualSiteGuardianCommand` |
| `POST /api/IndividualSites/{id}/invite` | `InviteToProfileCommand` |
| `POST /api/IndividualSites/{id}/transfer-control` | `TransferControlCommand` |
| `POST /api/IndividualSites/{id}/collaboration` | `SetCollaborationCommand` |
| `GET /api/IndividualSites/{id}/config/draft` | `GetDraftAthleteSiteSnapshotQuery` |
| `POST /api/OrganizationSites/club-register` | `RegisterOrganizationSiteCommand` |
| `POST /api/OrganizationSites/send-offical-email-verification` | `SendOfficialEmailVerificationCommand` |
| `POST /api/action-tokens/{id}/accept` | `AcceptActionTokenCommand` (+ strategies) |
| `GET /api/Me` | `GetCurrentUserQuery` |
| `GET /api/sites` | `GetSitesQuery` |
| `POST /api/clubs/scrape`, `PUT /api/clubs/{add,remove}-sports` | ClubRegistry (all `[AllowAnonymous]`) |

Per-command detail: [`docs/confluence/backend/commands/`](docs/confluence/backend/commands/README.md).

---

## Build / run / test

```bash
dotnet restore NextAtlet.slnx && dotnet build NextAtlet.slnx
dotnet run --project apps/NextAtlet.Server/NextAtlet.Api    # http://localhost:5278, Swagger at /swagger
dotnet test NextAtlet.slnx
cd apps/NextAtlet.Client && pnpm install && pnpm dev         # http://localhost:3000 (/ → /en)
```

**⚠️ Development startup runs `Database.EnsureDeleted()` then `Database.Migrate()` then seeds — the DB is dropped on EVERY `dotnet run`.** Committed connection string uses port **32768**. Set `Resend:InviteApiKey` via user-secrets for real email (else emails are only logged).

---

## Not built (designed only — do NOT describe as existing)

Public render endpoint + Next.js renderer; publish flow + ISR/CDN; the draft-edit write path (deleted in a refactor — `ISanitizationService`/`ISectionTypeRegistry` are registered but unused); theme picker; **all billing** (`PerkResolver` and `ResolveCapabilitiesCommand` are 100% commented out; no Plan/Subscription/Purchase entities, no Stripe); media pipeline (`MediaAsset` schema-only); memberships (`Membership` schema-only); change-request workflow (`ChangeRequest` schema-only, no `StatusId` column).

---

## Currently broken / risky — don't propagate these

- **Security:** `SendOfficialEmailVerificationCommand` lets any authenticated user verify an arbitrary org (no ownership check, returns token id); `GetDraftAthleteSiteSnapshotQuery` has no authorization; `ClubsController` is fully `[AllowAnonymous]`; invitation/consent strategies never match the accepting user's email; `GetSitesQuery` doesn't enforce visibility.
- **Bugs:** `TransferControlCommand` passes a profile id where a site id is needed (always fails) and `IndividualProfileRepository.GetByIdAsync`/`OrganizationProfileRepository.GetByIdAsync` misuse `FindAsync` (throw at runtime); guardian-registered under-16s get stuck in `pending_guardian_consent`; `SiteSnapshotResponse.Version` is always 0.
- **Infra:** CI (`.github/workflows/dotnet.yml`) pins .NET 8 against net10.0 (can't pass); `infra/` empty.
- **Naming traps (file name ≠ type name):** `IRetireable.cs`→`IRetirable`, `ProfilePermissions.cs`→`SitePermissions`, `GuardianPermissions.cs`→`LoginPermissions`, `IProfileLoginRepository.cs`→`ISiteLoginRepository`, `ICvrHttpService.cs`→`ICvrLookupService`, `ISportCanonicalizer.cs`→`IClubCanonicalizer`, `GetDraftSiteSnapshotQuery.cs`→`GetDraftAthleteSiteSnapshotQuery`, `TransforControlRequest.cs`→`TransferControlRequest`.
- **Frontend:** package manager is **pnpm** (`pnpm-lock.yaml`, `packageManager` field); `pnpm lint` fails (Next 16 removed `next lint`); `pnpm gen:api` would break the API client; `tailwind.config.cjs` is inert (v3 config, v4 runtime); `text-primary-gold` generates no CSS.

---

## Where to read more

| Topic | Doc |
|-------|-----|
| Onboarding home | [`docs/confluence/README.md`](docs/confluence/README.md) |
| Overview / concept | [`docs/00-overview.md`](docs/00-overview.md) |
| Architecture | [`docs/01-architecture.md`](docs/01-architecture.md) |
| Full schema | [`docs/02-data-model.md`](docs/02-data-model.md) |
| Accounts & permissions | [`docs/03-accounts-and-permissions.md`](docs/03-accounts-and-permissions.md) |
| Feature status board & problems | [`docs/06-features-and-problems.md`](docs/06-features-and-problems.md) |
| Conventions & how-to | [`docs/07-patterns-and-build-order.md`](docs/07-patterns-and-build-order.md) |
| CQRS/MediatR ADR | [`docs/08-adr-cqrs-mediatr-and-layering.md`](docs/08-adr-cqrs-mediatr-and-layering.md) |
