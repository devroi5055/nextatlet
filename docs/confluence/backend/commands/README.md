# Backend Commands & Queries

Every write in NextAtlet is a **command** and every read is a **query**. Both are C# records implementing `IRequest<T>`; their handler lives right next to them under [`NextAtlet.Application/Features/**`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features). Controllers contain no logic — they just `_sender.Send(...)` through MediatR.

## How to read these pages

Each page follows the same template: **What it does → Request → Response → How it works → Validation & error codes → Dependencies → Transaction behaviour → Side effects → Gotchas → Related.**

A few conventions that apply everywhere, so we don't repeat them on every page:

- **Auth:** unless a page says `[AllowAnonymous]`, the endpoint requires an authenticated user (a global fallback policy enforces this). Identity comes from the JWT claims, never the request body.
- **Error mapping:** a handler returns `Result<T>`. On failure the [`ResultFilter`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Filters/ResultFilter.cs) turns it into **HTTP 400** with `ApiError { errorCode, parameters }` — *always 400*, whatever the semantic category. A thrown exception becomes **HTTP 500** `internal_error`.
- **UnitOfWork:** handlers call `IUnitOfWork.SaveChangesAsync()` exactly once; there is no explicit transaction API. Emails are always sent **after** commit and are best-effort (a send failure never fails the request).

## Index

| Command / Query | Endpoint | Purpose | Page |
|-----------------|----------|---------|------|
| `RegisterIndividualSiteSelfCommand` | `POST /api/IndividualSites/self-register` | An athlete registers their own site | [self-register](./register-individual-site-self.md) |
| `RegisterIndividualSiteGuardianCommand` | `POST /api/IndividualSites/guardian-register` | A guardian registers a child's site | [guardian-register](./register-individual-site-guardian.md) |
| *(shared base)* | — | Slug rules, default snapshot, consent decision | [registration base](./individual-site-registration-base.md) |
| `RegisterOrganizationSiteCommand` | `POST /api/OrganizationSites/club-register` | Register a club/organization site | [club-register](./register-organization-site.md) |
| `InviteToProfileCommand` | `POST /api/IndividualSites/{id}/invite` | Invite a co-owner or guardian to a site | [invite-to-profile](./invite-to-profile.md) |
| `TransferControlCommand` | `POST /api/IndividualSites/{id}/transfer-control` | Move control between athlete and guardian | [transfer-control](./transfer-control.md) |
| `SetCollaborationCommand` | `POST /api/IndividualSites/{id}/collaboration` | Toggle shared editing | [set-collaboration](./set-collaboration.md) |
| `AcceptActionTokenCommand` | `POST /api/action-tokens/{id}/accept` | Accept an emailed action link | [accept-action-token](./accept-action-token.md) |
| *(strategies)* | — | Consent / Invitation / OrgVerification strategies | [action-token-strategies](./action-token-strategies.md) |
| `SendOfficialEmailVerificationCommand` | `POST /api/OrganizationSites/send-offical-email-verification` | Email a club official to verify an org | [send-official-email-verification](./send-official-email-verification.md) |
| `GetCurrentUserQuery` | `GET /api/Me` | The decision-gate "who am I" query | [get-current-user](./get-current-user.md) |
| `GetSitesQuery` | `GET /api/sites` | Public paged site listing | [get-sites](./get-sites.md) |
| `GetDraftAthleteSiteSnapshotQuery` | `GET /api/IndividualSites/{id}/config/draft` | Read a site's draft content | [get-draft-site-snapshot](./get-draft-site-snapshot.md) |
| `ScrapeClubsCommand` | `POST /api/clubs/scrape` | Scrape the DJU club registry | [scrape-clubs](./scrape-clubs.md) |
| `AddSportsCommand` / `RemoveSportsCommand` | `PUT /api/clubs/add-sports` · `remove-sports` | Edit a club's sports | [add-and-remove-sports](./add-and-remove-sports.md) |
| `ListClubOfficialsCommand` | *(no HTTP route)* | List a club's officials | [list-club-officials](./list-club-officials.md) |

## ⚠️ Security note

Several endpoints have **known authorization gaps** — anonymous club mutation, unauthenticated draft reads, and an org-verification flow that lets any authenticated user verify an arbitrary organization. These are documented in the **Gotchas** section of each affected page and summarised in [`docs/06-features-and-problems.md`](https://github.com/devroi5055/nextatlet/blob/main/docs/06-features-and-problems.md). They must be closed before production.
