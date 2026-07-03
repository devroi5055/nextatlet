# 07 · Patterns, Build Order & Open Questions

**Depends on:** all preceding documents. This is the implementation guidance and the running decision log.

---

## 1. Design patterns

| Pattern | Where | Why |
|---------|-------|-----|
| **Strategy** | section validation & rendering contract | one strategy per section type; add a type → add a strategy, no `switch` sprawl |
| **Factory / Registry** | `SectionTypeRegistry`, `ActionTokenStrategyRegistry` | maps `"hero"` → schema + validator + (frontend) component contract; same idea for club section types. `ActionTokenStrategyRegistry` maps `ActionTokenType` → `IActionTokenStrategy` — same dict-keyed pattern; strategies are Scoped (touch repos/UoW). |
| **`IActionTokenStrategy`** | ActionToken accept dispatch | Interface: `ActionTokenType` (which kind this handles), `authRequired: bool` (enforced by the handler before dispatch), `ExecuteAsync(token, actor, ct): Task<Result>` (type-specific side effect). Implementations: `ConsentStrategy` (authRequired = true, records `GuardianConsent`), `InvitationStrategy` (authRequired = true, creates Active `SiteLogin`), `OrgEmailVerificationStrategy` (authRequired = false, marks org Verified). |
| **Permission resolver** *(built — replaces the originally-planned Specification pattern)* | profile ownership / edit / publish authority | `PermissionResolver.Resolve(SiteLogin, IndividualProfile) → SitePermissions` derives a capability preset from `ControlMode` + role; handlers add natural "caller holds an active login" checks. No `CanEditField`/`CanManageBilling` Specification classes exist. Tier/perk gating is deferred with billing. |
| **CQRS via MediatR** | application commands/queries | `IRequest`/`IRequestHandler` dispatched via `ISender`; handlers orchestrate repositories + services; thin controllers; pipeline behaviors for cross-cutting concerns |
| **Repository + Unit of Work** | all DB access | handlers depend on repository interfaces (in Application); EF implementations + `EfUnitOfWork` in Infrastructure; one `SaveChangesAsync` per request, commit timing owned by the handler |
| **Read/write path split** | editor (write) vs public (read) paths | separate, sanitized, cacheable public read model from the editable model (`01` §3) — orthogonal to the MediatR dispatch mechanism |
| **Builder** | public render payload | compose config + theme manifest + resolved media into one self-contained response |
| **Decorator / Middleware** | caching + sanitization on public path | wrap public render without touching core logic |
| **DTO + mapping** | API boundaries | never expose EF entities; map to DTOs so internal schema can evolve |
| **Options pattern** | tier/perk definitions, theme config | `IOptions<…>` for testable, injectable config |
| **Dedicated service: `PerkResolver`** | effective-capability resolution | single place that computes `max(SelfTier, ActiveClubPerks)` per feature (`02` §6, `04`) — never persist conflated state |
| **Dedicated service: `PublicContractProjector`** | the only path athlete data leaves to public/club | `ToPublicContract(profile)` — enforces the privacy boundary in one place (`03` §4) |

> Resist over-engineering. No event sourcing, no microservice-per-concept, no full DDD aggregates at this stage. **Strategy (action tokens / sections) + a `PermissionResolver` for authz + CQRS via MediatR over repositories + the read/write path split** carry the system today; the two focused services (`PerkResolver`, `PublicContractProjector`) are planned with billing and the public render path. Repositories are intention-revealing per aggregate (`GetBySlugAsync`, `GetCurrentDraftBySiteIdAsync`), **not** a generic `IRepository<T>` framework.

### Layering & dependency direction

```
Api ──► Application ◄── Infrastructure
          │                  │
          └──────► Domain ◄──┘
```

Clean Architecture: abstractions (MediatR handlers, repository interfaces, `IUnitOfWork`, service interfaces) live in **Application**; EF Core implementations (`NextAtletDbContext`, repositories, `EfUnitOfWork`) live in **Infrastructure**, which references Application. Handlers never see `DbContext`. This inverts the original `Application → Infrastructure` reference (see `docs/08` ADR). MediatR is v13 Community; the choice is reversible (MediatR 12 MIT, `martinothamar/Mediator`, or a hand-rolled `ISender`) since every feature is just an `IRequestHandler`.

### Error handling — three mechanisms, strict lanes

**Chosen: Model A (error codes + global handler)** with three distinct mechanisms, each with a strict lane. The backend emits stable **error codes**, never localized strings; the frontend resolves codes → `da`/`en` text.

**Three mechanisms, strict lanes:**

1. **`InvalidOperationException` / `InvariantViolationException`** — impossible state, broken invariant → **500**, logged, no user message. For conditions that should never happen given correct application logic: an authenticated user with no DB row, a token type with no registered strategy, a token targeting a missing site (impossible with ON DELETE CASCADE). Not `ArgumentNullException` (that is bad-null-input, not a failed lookup).

2. **`DomainException(errorCode)`** → **4xx**, user-facing, halts to the API boundary. The workhorse for expected business-rule rejections: underage, not authorized, expired token, club not verified. Errors bubble naturally to the global handler; callers rarely need to branch on them.

3. **`Result<T>` / `Result`** → caller acts on the outcome locally (branches, falls back, combines). Narrow use: validation collecting multiple errors, a few branchable queries. Can fail-fast; `IsFailure` is the branch point.

**Repository contract:** repositories return `T?` (null = not found), not `Result`. The handler interprets null — throw `DomainException` if the entity must exist; branch if absent is valid.

**Frontend coherence:** both `DomainException` and `Result.Failure` produce the same `ApiError` JSON at the boundary (via `ResultFilter` and `GlobalExceptionHandler`), so there is no frontend-coherence argument for preferring one over the other for single-rejection flows. Prefer `DomainException` for business rejections that propagate — no threading needed.

One `ErrorCodes` source of truth (codes carry their HTTP status: 400/403/404/409/422); one `ApiError` response shape (`01`). The frontend resolves codes to `da`/`en` text. Mechanisms: a global `ResultFilter` (unwraps `Result<T>`) + a `GlobalExceptionHandler` (`DomainException` + unhandled).

### Authentication — dual-scheme (cookie + bearer)

Auth0 (OIDC) is the identity provider. The API registers **two authentication schemes** plus a `smart` policy scheme that forwards per request:

- **Bearer** — `Authorization: Bearer <jwt>` (Swagger, service-to-service). Validated against the Auth0 issuer/audience (RS256, keys via JWKS).
- **Cookie** — the production/Next.js session (httpOnly, `SameSite=Strict`).
- **`smart`** `AddPolicyScheme` — `ForwardDefaultSelector` routes to bearer if an `Authorization: Bearer` header is present, else cookie. Endpoints carry a plain `[Authorize]`; a global **fallback policy** makes everything authenticated unless it opts out with `[AllowAnonymous]` (the OpenAPI doc does).

Authentication and domain registration are **two gates**: Gate 1 = Auth0 credential (age-blind); Gate 2 = profile registration behind `[Authorize]`. The caller's `sub`/`email` are read from the validated principal via scheme-agnostic `ClaimsPrincipalExtensions` (`GetAuthProviderId()` / `GetEmail()`, email via the `https://nextatlet.dk/email` namespaced claim) — **never** the request body. Auth0 tenant: `nextatlet-dev.eu.auth0.com`, audience `https://api.nextatlet.dk`. (CSRF on the cookie `POST`s and which app issues the production cookie remain open items.)

### Naming: commands by intent (registration split)

Commands are named for the **use-case**, not CRUD. Athlete creation is two intent-named commands sharing a private core (`IndividualSiteRegistrationHandlerBase.CreateIndividualProfileCoreAsync`): `RegisterIndividualSiteSelfCommand` (caller → `IndividualRole.owner`) and `RegisterIndividualSiteGuardianCommand` (caller → `IndividualRole.guardian`, child login deferred). The shared base owns slug validation + `IndividualProfile` + default draft `SiteSnapshot` + user get-or-create (`UserProvisioner`); each handler owns only its login-attachment + rules. Org registration follows the same shape (`RegisterOrganizationSiteCommand`). New onboarding variants extend the base, not copy it.

---

## 2. Things to keep stable (don't churn these)

These are load-bearing; changing them ripples everywhere:

1. **Config-as-data** (sections + theme manifest; backend emits no HTML). (`01`, `02`)
2. **Additive perk layer, never replace.** (`04`)
3. **Published public data contract as the only outbound athlete projection.** (`03`)
4. **One profile + linked roles; 18 as the guardian boundary.** (`03`)
5. **Generic time-bounded memberships with derived primaries; history retained.** (`02`)

Everything else (tier numbers, theme counts, exact section types, storage vendor) is negotiable.

---

## 3. Incremental build order

Each step is shippable and additive — later steps don't rewrite earlier ones. Status: ✅ done · 🟡 partial · ⬜ not started.

1. ✅ **Profiles + `Site`/`SiteSnapshot` + auth (multi-login).** Auth0 dual-scheme; self/guardian/org registration; `SiteLogin`; consent via `ActionToken`; `ControlMode` + `PermissionResolver`; `GET /api/Me` decision gate. (The draft **edit** endpoint is disabled pending a write-path rebuild; section types `hero`/`bio` exist as `SectionData`.)
2. ⬜ **Public render endpoint + minimal Next.js renderer.** Prove data→render end-to-end with the cousin's profile as the first real case. *(Critical milestone — everything after is addition.)* Only `GET /api/sites` (listing) exists today.
3. ⬜ **Publish flow** (draft → published) + ISR/CDN caching + invalidation on publish.
4. 🟡 **Section registry + Strategy validators;** add `results`, `gallery`. (`ISectionTypeRegistry`/`SanitizationService` exist; not yet wired to an editor write endpoint.)
5. ⬜ **Theme manifest system + theme picker;** add a 2nd and 3rd theme. (One seeded `ClassicTheme`; `Theme.Manifest` is the contract; no picker.)
6. ⬜ **Athlete self-tiers + tier gating** (the originally-planned "Specifications"); wire tier → editable scope + theme access. (Tier enums exist; no gating, no billing.)
7. ⬜ **Media pipeline** (upload → blob/CDN → thumbnails); `gallery`/`video` sections; admin-on-behalf uploads. (`MediaAsset` schema only.)
8. 🟡 **Organizations + multi-user roles** (`club_admin`/`club_editor`); club page engine; club signup. (Org **registration** + email-verification + the imported club registry are built; club **page engine** + staff roles UI are not.)
9. ⬜ **Memberships + derived primaries;** affiliate athletes into slots; history retention. (`Membership` is a scaffold; no commands.)
10. ⬜ **PerkResolver + additive perk layer;** club subscriptions; effective-capability resolution. (`PerkResolver` is a commented stub.)
11. ⬜ **Club showcases** via published-contract references; placeholder on private/unpublished; dependency revalidation.
12. ⬜ **Change-request / approval workflow** (club proposes → guardian/athlete approves). (`ChangeRequest` is a scaffold; no commands.)
13. ⬜ **Mentoring content + 1:1 scheduling;** photoshoot booking.
14. ⬜ **Versioning/history** for rollback (higher tiers).
15. ⬜ **Bilingual polish; custom subdomains** (DNS/cert complexity — last).

> Beyond the numbered plan, two subsystems already exist that the original order didn't anticipate: **control transfer / collaboration** (`transfer-control`, `collaboration` endpoints + `ControlMode` shared variants) and the **club registry** (scrape Danish clubs + officials, CVR lookup) that feeds organization email-verification.

---

## 4. Open questions / decisions to resolve before the relevant step

| # | Question | Blocks step | Leaning |
|---|----------|-------------|---------|
| 1 | One-row-per-state vs `SiteSnapshotHistory` table for rollback? | 3 / 14 | history table if rollback matters to higher tiers |
| 2 | Final tier prices & exact slot/session counts? | 6 / 10 | placeholders in `04` until business decides |
| 3 | Club downgrade below current roster — block, or mark overflow inactive? | 10 | block downgrade until under limit (simpler, fairer) |
| 4 | Self-serve vs admin-created for Academy / TrainingCenter / SchoolTeam? | 8 | admin-created first; revisit per type |
| 5 | Bilingual: per-field locale maps vs section variants? Level 1 (code) vs Level 2 (`PlanCapability` rows) for feature gating? Note: `PlanCapability` ERD previously showed wrong columns (copy-paste of Plan); correct shape is `PlanId + FeatureKey + Value`. | 4 / 6 | per-field maps; Level 1 now, Level 2 only if non-dev editing needed |
| 6 | How "custom" can Pro custom sections get before a dev is needed? | 5 / 6 | hard boundary; **no** free-form HTML (XSS/quality) |
| 7 | Second-guardian / delegate flows beyond the single guardian default? | 1 | model supports it; build when needed |
| 8 | Data ownership/retention if an athlete deletes their profile while club-affiliated? | 8 / 11 | athlete owns; club showcase degrades to placeholder; ToS question |
| 9 | Photoshoot scheduling/logistics system (photographer calendars, locations)? | 13 | out of MVP core; revisit |
| 10 | `ConsentState` stored vs. derived? | 1 | lean derived from `GuardianConsent`-existence + age band — removes the stored field and sync burden |
| 11 | `ConsentNotNeeded`: `Result`/idempotent-success vs `DomainException`? | 1 | current code returns an `Error`; decide if re-consent on an already-consented profile should be a silent success or a rejection |
| 12 | Token cleanup job: what is the retention policy? | post-MVP | purge where `AcceptedUtc IS NOT NULL` OR `ExpiresUtc IS PAST`, with optional grace period for audit |
| 13 | MitID Erhverv as a future org verification method? | 8 | payload `MethodId` already accommodates it; no code needed until the method is concrete |

---

## 5. Document maintenance

When a decision in §4 is made, update the relevant numbered document **and** strike the row here, noting the date/decision. Keep this file as the single index of "what's still undecided."
