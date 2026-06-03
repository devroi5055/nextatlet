# REFACTOR_PLAN — CQRS (MediatR) + Repository pattern

**Status:** plan-before-code. No code changes until this is approved.
**Scope:** `apps/NextAtlet.Server` (the ASP.NET Core solution). The Next.js frontend is untouched.
**Guiding principle:** Follow the existing conventions in the codebase — file/folder layout (`Features/<Area>/Commands|Queries`), naming, the `Domain`/`Infrastructure`/`Application`/`Api` split, DI registration style, and the patterns already documented in `CLAUDE.md` and `docs/07`. Consistency with what's there beats personal preference. **But don't overcomplicate:** add only the abstraction the three current features actually need. No generic repository framework, no speculative interfaces, no patterns "for later." If a piece isn't used by the code we're converting now, it doesn't go in. When in doubt, the simpler option that still respects the conventions wins.

**Goals:**

1. Move from CQRS-lite (hand-rolled `*Command`/`*Query` classes) to CQRS with MediatR (`IRequest`/`IRequestHandler`, dispatched via `ISender`).
2. Introduce the repository pattern + Unit of Work for all DB access. MediatR handlers become **orchestrators** — they coordinate repositories, services, and domain logic; they never touch `DbContext` directly.
3. Update documentation to match.

---

## A note on "the IMediator built into .NET"

There is **no mediator built into .NET**. The `IMediator` / `ISender` abstraction comes from the third-party **MediatR** NuGet package. The only "built-in" piece is `Microsoft.Extensions.DependencyInjection`, which MediatR registers into via `services.AddMediatR(...)` — that is almost certainly what you're picturing. This plan uses MediatR wired through the standard .NET DI container.

One catch worth a decision: **MediatR went commercial on 2 July 2025** (v13+, dual RPL-1.5 / commercial license). It is **free under the Community edition for orgs under $5M gross annual revenue**, non-profits, education, and non-production use; above that it needs a paid license key.

**Recommendation:** start with **MediatR v13 Community** (zero code difference from what you expect, fully documented). If IT Minds / the product owner later crosses the revenue threshold or wants no third-party licensing surface, the handler/`IRequest` code is portable to a free drop-in:

- Pin **MediatR 12.x** (last MIT release) — identical API, frozen.
- Or **`martinothamar/Mediator`** (MIT, source-generated, faster) — near-identical API, minor namespace changes.
- Or a ~40-line hand-rolled `ISender` over DI.

Because every handler we write is just a class implementing `IRequestHandler<TReq,TRes>`, this choice is reversible later and does **not** block the refactor. The plan below is written against MediatR's API.

---

## Current state (verified against the repo)

Four projects under `apps/NextAtlet.Server/`:

| Project | Today's role | Today's references |
|---|---|---|
| `NextAtlet.Domain` | Entities, enums, value objects | none |
| `NextAtlet.Infrastructure` | `NextAtletDbContext`, EF config, `SanitizationService`, `SectionTypeRegistry` | → Domain |
| `NextAtlet.Application` | `CreateAthleteCommand`, `UpdateDraftConfigCommand`, `GetDraftConfigQuery` (plain classes with `ExecuteAsync`) | → Domain, **→ Infrastructure** |
| `NextAtlet.Api` | `AthletesController`, `Program.cs` | → Application, Infrastructure, Domain |

How it works now:

- Controllers inject the concrete command/query classes and call `ExecuteAsync(...)`.
- Each command/query injects `NextAtletDbContext` **directly** and runs EF queries + `SaveChangesAsync()` inline. DB access and orchestration are fused.
- Everything is registered by hand in `Program.cs` (`AddScoped<CreateAthleteCommand>()`, etc.).
- Error handling is per-action `try/catch` in the controller mapping `InvalidOperationException` → 400/404.
- `DbContext` exposes: `Users`, `AthleteProfiles`, `ProfileLogins`, `Themes`, `SiteConfigs`, `MediaAssets`.

**Key architectural smell to fix:** `Application` references `Infrastructure`. The dependency arrow points the wrong way for the repository pattern. We will invert it (Clean Architecture): abstractions live in `Application`, implementations in `Infrastructure`, and `Infrastructure → Application`.

---

## Target architecture

```
Api  ──►  Application  ◄──  Infrastructure
            │                    │
            └────────►  Domain  ◄┘
```

- **Application** owns the contracts: `IRequest`/handlers, repository interfaces (`IAthleteProfileRepository`, …), `IUnitOfWork`, and abstractions for `ISanitizationService` / `ISectionTypeRegistry`. No EF here.
- **Infrastructure** implements those interfaces over EF Core (`DbContext`, repositories, UoW) and references Application.
- **Api** depends on Application (and Infrastructure only at composition root / `Program.cs` for DI wiring).
- A handler reads/writes through repository interfaces, then calls `IUnitOfWork.SaveChangesAsync()` once. It never sees `DbContext`.

### Repository design

- One repository interface per aggregate root we touch: `IAthleteProfileRepository`, `IUserRepository`, `IProfileLoginRepository`, `IThemeRepository`, `ISiteConfigRepository`. (`MediaAsset` deferred until a feature needs it.)
- Methods are intention-revealing, not generic CRUD dumps — e.g. `GetBySlugAsync`, `GetDraftConfigAsync(profileId)`, `AddAsync`, `ExistsBySlugAsync`. This keeps query logic out of handlers and testable.
- `IUnitOfWork.SaveChangesAsync()` wraps `DbContext.SaveChanges` so commit timing stays in the handler (orchestrator), not the repository.
- Repositories return domain entities. DTO projection stays in the handler (commands) — except read-heavy queries, see decision Q-B below.

---

## Phased plan (each phase compiles & is independently reviewable)

### Phase 0 — Safety net (do first)
- [ ] Add a test project `NextAtlet.Application.Tests` (xUnit). There are currently **no tests** — adding characterization tests for the 3 existing endpoints first means the refactor is verifiable, not hopeful.
- [ ] Cover the current behaviour of `CreateAthlete`, `GetDraftConfig`, `UpdateDraftConfig` (happy path + the key throws: duplicate slug, reserved slug, minor-without-guardian, version conflict).
- [ ] Confirm `dotnet build` + `dotnet test` green before touching anything.

### Phase 1 — Invert the dependency direction
- [ ] Add `NextAtlet.Infrastructure → NextAtlet.Application` project reference.
- [ ] Remove `NextAtlet.Application → NextAtlet.Infrastructure` reference.
- [ ] Move/define in **Application**: `Abstractions/Persistence/` (repository + `IUnitOfWork` interfaces), `Abstractions/Services/` (`ISanitizationService`, `ISectionTypeRegistry`).
- [ ] Have the existing Infrastructure services implement the new interfaces (`SanitizationService : ISanitizationService`, etc.).
- [ ] This phase will break the 3 command/query files (they import Infrastructure) — expected; fixed in Phase 3. Use a short-lived branch.

### Phase 2 — Add MediatR + repositories (no behaviour change yet)
- [ ] Add `MediatR` package to `NextAtlet.Application` (v13 Community — see note above).
- [ ] In `Infrastructure`, implement each repository over `NextAtletDbContext` and an `EfUnitOfWork : IUnitOfWork`.
- [ ] Register in `Program.cs`: `AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(<AppMarker>).Assembly))`, plus `AddScoped` for each repository + `IUnitOfWork`, and the service interfaces.

### Phase 3 — Convert the 3 features to MediatR handlers
For each, define a request + handler in `Application/Features/Athletes/...`:

- [ ] `CreateAthleteCommand : IRequest<AthleteProfileDto>` + `CreateAthleteCommandHandler`. Handler orchestrates `IUserRepository`, `IAthleteProfileRepository`, `IProfileLoginRepository`, `IThemeRepository`, `ISiteConfigRepository`, then one `IUnitOfWork.SaveChangesAsync()`. Slug/reserved/minor rules stay in the handler (or move to domain — see Q-C).
- [ ] `UpdateDraftConfigCommand : IRequest<SiteConfigDto>` + handler. Concurrency check, theme lookup, `ValidateLayout`, sanitize, version bump — orchestration only; reads/writes via repositories.
- [ ] `GetDraftConfigQuery : IRequest<SiteConfigDto>` + handler over `ISiteConfigRepository`.
- [ ] Map the DTO building (currently in the controller) into the handlers so controllers stay thin.

### Phase 4 — Thin the controllers + cross-cutting concerns
- [ ] Inject `ISender` into `AthletesController`; each action becomes `return Ok(await _sender.Send(request))` style.
- [ ] Replace per-action `try/catch` with a **MediatR `ValidationBehavior`** (pipeline) + a global exception-handling middleware / `IExceptionHandler` mapping domain exceptions → ProblemDetails. (Optional but recommended; consider FluentValidation here.)
- [ ] Delete the old `*Command`/`*Query` plain classes and their `Program.cs` registrations.

### Phase 5 — Documentation
- [ ] `CLAUDE.md`: change "CQRS-lite" → "CQRS via MediatR"; update the "Design patterns in use" line; update the dependency-direction / layering description; note repository + UnitOfWork; note the MediatR licensing decision.
- [ ] `docs/07-patterns-and-build-order.md`: replace CQRS-lite section; add repository/UoW pattern; record the dependency inversion.
- [ ] `docs/01-architecture.md`: update the read/write path split to reference MediatR pipeline + repositories.
- [ ] Add an ADR-style note (in `docs/`) capturing the MediatR-licensing choice and the inversion, so the decision is traceable.
- [ ] Reconcile the scattered root notes (`STEP_1_*.md`, `IMPLEMENTATION_NOTES.md`, `BACKEND_QUICK_START.md`) — at minimum point them at the new structure.

### Phase 6 — Verify
- [ ] `dotnet build` + `dotnet test` green (Phase 0 tests now exercise the MediatR path unchanged).
- [ ] Manual smoke via `NextAtlet.Api.http` against the 3 endpoints.
- [ ] Diff review: confirm no handler references `NextAtletDbContext`; confirm `Application` has no EF package/reference.

---

## Open decisions (resolve before/while coding)

| # | Question | Recommendation |
|---|---|---|
| Q-A | MediatR v13 Community vs v12 MIT vs free alt | Start v13 Community; revisit if revenue threshold / licensing policy demands. |
| Q-B | Queries: repository-returns-entity + map, vs query handler reads `DbContext` read-model directly (CQRS-classic) | For now route everything through repositories for consistency; allow read-only query handlers to use a dedicated read interface later if perf needs it. |
| Q-C | Keep slug/reserved/minor rules in handler vs push into Domain (factory methods / domain service) | Lean toward moving invariants into Domain over time; not required for this refactor. |
| Q-D | Add FluentValidation now or hand-write `ValidationBehavior` | Optional; can land in Phase 4 or be deferred. |
| Q-E | Generic `IRepository<T>` base vs only specific interfaces | Prefer specific interfaces (avoids leaky `IQueryable`); add a base only if duplication appears. |

---

## Risks & notes

- **Phase 1 is the riskiest** (reference inversion temporarily breaks the build). Keep it on its own branch; Phases 2–3 restore green quickly.
- `EnsureCreated()` in `Program.cs` and the existing JSONB value conversions are unaffected — repositories sit on top of the same `DbContext`/EF config.
- No DB schema change in this refactor. No migration needed.
- Net new packages: `MediatR` (Application), optionally `FluentValidation` (Application). No package removed.
- Keep PRs per-phase for reviewable diffs.
