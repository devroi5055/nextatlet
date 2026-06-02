# 01 · Architecture

**Depends on:** `00-overview.md`. **Pairs with:** `02-data-model.md`, `07-patterns-and-build-order.md`.

---

## 1. Tech stack

| Layer | Choice | Notes |
|-------|--------|-------|
| Backend | **.NET (ASP.NET Core Web API)** | Domain logic, config CRUD, validation, auth, rendering contract. |
| ORM | **EF Core** | Hybrid relational + JSON column model (see `02`). |
| Database | **PostgreSQL** (recommended) | `jsonb` is well-suited to the section-as-data model. SQL Server works too. |
| Frontend | **Next.js** | Two surfaces: the **editor** (authenticated SPA-style) and the **public site renderer** (SSR/ISR for SEO). |
| Media storage | **Blob storage + CDN** (Azure Blob / S3 + CloudFront/equivalent) | Bytes never live in the DB; the DB stores references only. |
| Auth | ASP.NET Core Identity or an external IdP (e.g. Auth0/Entra) | Must support **multiple linked logins per profile** (see `03`). |

### Why Next.js specifically

The public athlete and club pages live or die on **SEO and load speed** — discoverability under one domain is a core value proposition. Next.js gives:

- **SSR / ISR** for public pages → crawlable, fast, cacheable per published version.
- **Client-side interactivity** for the editor.
- A natural home for the **section-component registry** (one React component per section type — see §4).

> Public pages should be **statically regenerated on publish** (ISR with on-demand revalidation), not rendered fresh per request. Publishing an athlete or club page triggers revalidation of exactly the affected routes.

## 2. System topology

```
                         ┌───────────────────────────────────────────┐
                         │              .NET Web API                  │
   ┌──────────────┐      │                                            │     ┌──────────────┐
   │ Next.js      │      │  Auth / Identity (multi-login per profile) │     │ PostgreSQL   │
   │ EDITOR       │─────►│  Athlete config CRUD  (draft/publish)      │────►│ profiles,    │
   │ (athletes,   │      │  Organization CRUD    (club pages, roster) │     │ orgs,        │
   │  guardians,  │      │  Membership service   (affiliations)       │     │ memberships, │
   │  club staff) │      │  Tier + Perk resolver (additive layer)     │     │ themes,      │
   └──────────────┘      │  Approval workflow    (change requests)    │     │ media refs   │
                         │  Validation (schema + tier + ownership)    │     └──────────────┘
   ┌──────────────┐      │                                            │     ┌──────────────┐
   │ Next.js      │      │  PUBLIC read endpoints                     │     │ Blob + CDN   │
   │ PUBLIC SITE  │◄─────│  (published public data contract only,     │◄────│ images,      │
   │ (visitors)   │      │   cached, sanitized)                       │     │ video        │
   └──────────────┘      └───────────────────────────────────────────┘     └──────────────┘
```

## 3. Read/write path separation (CQRS-lite)

Two fundamentally different read paths. Do **not** share a model between them.

1. **Editor path** (authenticated): returns the full editable draft config **plus** a schema describing what *this profile's tier + active perks* may edit. Never cached.
2. **Public path** (anonymous): returns only the **published public data contract** — sanitized, with resolved CDN media URLs and the theme manifest. Aggressively cached; invalidated on publish.

The same separation applies to organizations: club staff edit a draft club page; visitors see the published one.

**Draft vs Published is a hard rule.** Editing never mutates what the public (or an affiliated club page) sees until publish. This matters doubly for the B2B side: club pages consume the athlete's **published** contract only — never drafts, never private fields (see `03`).

## 4. The rendering contract (backend ↔ Next.js)

The backend **never emits HTML**. The athlete site is *configuration as data*; the frontend renders it. The contract has three parts:

1. **Layout** — the published ordered list of typed sections + each section's data.
2. **Theme manifest** — which section types the theme supports, its color/font slots and constraints.
3. **Resolved media** — fully-resolved CDN URLs for referenced assets.

```
 published Layout.sections[]
        │  (ordered, typed)
        ▼
 Next.js SectionComponentRegistry      Theme tokens (from manifest)
   "hero"    → <HeroSection/>                 │
   "bio"     → <BioSection/>          ────────┤ applied as CSS variables /
   "results" → <ResultsSection/>              │ design tokens per theme
   "gallery" → <GallerySection/>              │
        │                                     │
        ▼                                     ▼
        └──────────────► rendered public page ◄──────────
```

Because the contract is data + manifest (not markup), the frontend is replaceable and the theme set grows without backend changes. A theme that doesn't support `video` simply omits it from its manifest, and the editor won't offer it.

### Club page rendering reuses the same engine

A club page is itself a SiteConfig-like document with its own section types (e.g. `clubHero`, `featuredAthletes`, `clubResults`). The **`featuredAthletes` section does not duplicate athlete data** — it holds references to athlete profiles, and at render time the public read endpoint resolves each reference against that athlete's **published public data contract**. If an athlete unpublishes or privatizes, the showcase degrades to a graceful placeholder rather than breaking or leaking (see `02` and `03`).

## 5. Caching

- Public athlete/club pages → ISR + CDN, keyed by slug + published version. **Invalidate on publish** by bumping the published version (cache key changes naturally).
- A club page that live-references athletes must also revalidate when a **featured athlete republishes** — track the dependency so the club route is revalidated too.
- Editor/draft endpoints → never cached.
- Media → immutable, content-hashed URLs → cache forever at the CDN.

## 6. Boundaries deliberately left out of MVP

- Sponsor marketplace (data model leaves room; no endpoints yet).
- Federation self-service for National Teams (internal-admin-managed only at launch).
- Fully free-form custom HTML sections (reopens XSS + quality problems the engine exists to prevent — see open questions in `07`).
