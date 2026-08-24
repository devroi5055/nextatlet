# 00 · NextAtlet — Overview

> **This doc describes the system as it actually is.** For an onboarding-oriented tour with diagrams, start at [`docs/confluence/README.md`](confluence/README.md).

## What it is

NextAtlet is a platform where **judo athletes in Denmark build a public profile website** to present themselves and attract sponsors. Three audiences: **athletes**, their **guardians** (minors are first-class because of GDPR Art. 8), and **clubs/organizations**.

MVP scope: **judo only, Denmark only, bilingual `da`/`en`** from day one. The data model leaves room to generalise sport and country later.

## The core idea: config-as-data

The backend emits **no HTML**. A site is stored as **structured content (typed sections) + a theme manifest**; the Next.js frontend renders it. This keeps rendering/caching/theming on the frontend and the backend a clean data + authorization service.

## Domain vocabulary

| Term | Meaning |
|------|---------|
| **Site** | The publishable unit. Unique `Slug`, display name, visibility, type (`individual`/`organization`). |
| **IndividualProfile** | Athlete data for an individual Site (sport, DOB, control mode, consent state). |
| **OrganizationProfile** | Club data for an organization Site (slots, tier, verification). |
| **SiteSnapshot** | A content version: jsonb layout (sections) + global settings + a theme. Site points at a current draft + published snapshot. |
| **Theme** | The render contract: a jsonb manifest of colors/fonts/style slots. |
| **SiteLogin** | Grants a User access to a Site in a role (`owner`/`guardian`). |
| **User** | An Auth0 identity, provisioned just-in-time. No password. |
| **ActionToken** | Single-use emailed link (invite / consent / org verification); the row GUID is the secret. |
| **Club / ClubOfficial** | Scraped read-only registry of real Danish clubs, backing org verification. Not the same as OrganizationProfile. |

## Repo layout

```
apps/NextAtlet.Server/   Backend solution (Api, Application, Domain, Infrastructure)
apps/NextAtlet.Client/   Next.js 16 frontend
tests/                   Only NextAtlet.Application.Tests has source
docs/                    These specs (00–08) + docs/confluence/ onboarding docs
infra/                   EMPTY (no infrastructure-as-code)
```

## Built vs. not built

**Built:** Auth0 dual-scheme auth + JIT user provisioning; two-path registration (self/guardian) with age gating; the Site/IndividualProfile/OrganizationProfile/SiteSnapshot schema; SiteLogin + PermissionResolver; the ActionToken flow (invite/consent/org-verification) with a strategy registry; GuardianConsent audit; control transfer + collaboration; `GET /api/Me`; `GET /api/sites`; the scraped club registry; the `Result<T>` error pipeline; frontend marketing page + onboarding wizard + Auth0 gate.

**NOT built (designed only):** public render endpoint + Next.js renderer; publish flow + ISR/CDN; the draft-edit write path (deleted in a refactor); theme picker; **all billing** (`PerkResolver`/`ResolveCapabilitiesCommand` are commented out — no Plan/Subscription/Purchase, no Stripe); media pipeline (`MediaAsset` schema-only); memberships (`Membership` schema-only); change-request workflow (`ChangeRequest` schema-only); mentoring; versioning; subdomains.

See [`06-features-and-problems.md`](06-features-and-problems.md) for the full status board and known problems.

## Tech stack (short)

.NET 10 · ASP.NET Core + MediatR 13 · EF Core 9 + PostgreSQL · Auth0 · Resend email · Next.js 16 + React 19 · Tailwind v4 · next-intl v4 · React Query · Zustand. Full detail in [`01-architecture.md`](01-architecture.md).
