# 00 · NextAtlet — Overview

**Audience:** developers (internal). This set of documents is the architectural starting point for the NextAtlet platform. It is a living spec — adjust freely. The parts worth keeping stable are flagged in `07-patterns-and-build-order.md`.

**Document set**

| File | Purpose |
|------|---------|
| `00-overview.md` | Vision, business model, glossary (this file) |
| `01-architecture.md` | Tech stack, system topology, rendering contract |
| `02-data-model.md` | Database structure |
| `03-accounts-and-permissions.md` | Identity, roles, guardian model, approval flows |
| `04-tiers-and-features.md` | Athlete tiers, club subscriptions, additive perk layer |
| `05-signup-and-onboarding.md` | Signup flows per tier, onboarding, media-later |
| `06-features-and-problems.md` | Feature → problem → why |
| `07-patterns-and-build-order.md` | Design patterns, incremental build order, open questions |
| `08-adr-cqrs-mediatr-and-layering.md` | ADR — CQRS/MediatR, repository/UoW, layering |

> **Naming note (post-refactor).** The codebase renamed several core types since the first draft: `AthleteProfile` → **`IndividualProfile`**, `SiteConfig` → **`Site` + `SiteSnapshot`** (split), `ProfileLogin`/`OrganizationLogin` → one unified **`SiteLogin`**, `Organization` → **`OrganizationProfile`**. The glossary and docs below use the current names; `CLAUDE.md` carries a per-section implemented-vs-planned status.

---

## 1. Vision

NextAtlet increases the **digital presence of young athletes** and helps them attract sponsorship. It starts narrowly — **youth judo in Denmark** — where the community is tight, the competition circuit is active, and parents already invest in their children's athletic development.

The product is not just a tool; it is a **service**. Three pillars:

1. **Auto-generated athlete websites** — an athlete (or guardian) produces a public profile/portfolio with no code, on a hosted, searchable platform.
2. **Professional photography (and video)** — captured by internal photographers, usable on the profile and privately.
3. **Mentoring, guides, and network** — including the eventual ability to present athletes to sponsors.

## 2. Business model — B2C primary, B2B hybrid

NextAtlet is **B2C-primary**: the athlete owns their profile. This is the load-bearing principle. Everything else is built so it never undermines individual athlete ownership or individual athlete revenue.

It is **also B2B**: organizations (clubs and others) can register, manage their own pages, and showcase affiliated athletes — making the platform a hybrid.

**The non-negotiable rule that ties the two together:** an organization can enrich an athlete's experience (perks, photoshoots, analytics) but can **never replace, downgrade, or bypass** the athlete's own subscription. Organization perks are always an **additive, scoped layer** on top of what the athlete owns. (Full reasoning in `04-tiers-and-features.md`.)

### Why hybrid is worth the complexity

- Clubs are a **distribution channel**: one club onboarding brings many athletes.
- Clubs add **value to the athlete** (funded photoshoots, recruitment exposure) without taking ownership.
- Athlete affiliation history adds **value to the club** (sponsors and recruiters can see who has passed through), while keeping the athlete in control of their own profile.

## 3. Who the platform serves

| Actor | Relationship to platform |
|-------|--------------------------|
| **Athlete** | Owns a profile. Primary customer. May be a minor (guardian-gated) or adult. |
| **Guardian** | Linked login on a minor athlete's profile with configurable edit/approval rights. |
| **Organization** | Club, National Team, Academy, Training Center, or School Team. Showcases affiliated athletes; some types are subscription-paying (B2B). |
| **Organization staff** | Multiple users per organization with roles (admin/editor). |
| **NextAtlet internal admin** | Manages server-controlled entities (e.g. National Teams), media uploads on behalf of athletes, support. |
| **Sponsor (future)** | Discovers and connects to athletes; eventual marketplace. |

## 4. Glossary

Defined once here; used consistently across all documents.

| Term | Meaning |
|------|---------|
| **Site** | The shared identity envelope (`SiteType` = `individual` \| `organization`) holding slug, display name, visibility, and the draft/published snapshot pointers. |
| **Profile** | The per-subject metadata on a `Site`: `IndividualProfile` (one athlete) or `OrganizationProfile` (one org). |
| **Linked login / Identity** | A `SiteLogin` — a credential (`User`) attached to a `Site` with a **role**. A site can have several (e.g. owner + guardian). |
| **Owner** (`IndividualRole.owner`) | The role representing the athlete themselves. |
| **Guardian** | A linked role on a minor's profile with configurable edit/publish/approval permissions. |
| **Organization** | A B2B entity of a given `OrganizationType`. |
| **OrganizationType** | One of: `Club`, `NationalTeam`, `Academy`, `TrainingCenter`, `SchoolTeam`. |
| **Membership** | A time-bounded link between an athlete and an organization (`role`, `start`, `end`, `status`). Many-to-many over time. |
| **Active membership** | A membership whose status is currently active. An athlete may have at most **one active Club membership** at a time. |
| **Display primary** | The athlete's current primary **Club** — drives which club page shows them and which club perks apply. |
| **Prestige primary** | The athlete's **National Team** affiliation — server-managed, surfaced as a badge. |
| **Training-context primary** | The athlete's Academy / Training Center affiliation — optional. |
| **SiteSnapshot** | The site stored as data (not HTML). Two per `Site` — draft + published — pointed at by `Site.CurrentDraftSnapshotId` / `CurrentPublishedSnapshotId`. Immutable once written. (Formerly `SiteConfig`.) |
| **Section** | A typed content block inside a `SiteSnapshot.Layout` (hero, bio, results, gallery, sponsors, video…). |
| **Theme** | A named, versioned visual template the frontend knows how to render. |
| **Published public data contract** | The sanitized, published subset of an athlete profile. The **only** data organizations may consume. |
| **Perk layer** | The additive, scoped set of capabilities granted by an active club subscription. Reverts when the membership ends. |
| **Athlete slot** | A unit of a club subscription representing one athlete the club may affiliate and grant perks to. |
| **Tier** | An athlete's own subscription level (Free and up). Independent of, and never replaced by, club perks. |

## 5. MVP boundary

- **Sport:** judo only at launch (`Sport` field generalizes later).
- **Geography:** Denmark; design for Danish/English bilingual from the start (see `02-data-model.md`).
- **National Teams:** created and assigned **only by NextAtlet internal admins**. Federation self-service is a documented future extension, not built now.
- **Sponsor marketplace:** out of MVP; the data model leaves room for it.
- First real test case: the founder's cousin (an internationally competing Danish judoka), with the founder's aunt as a founding domain partner and distribution channel.
