# 08 · ADR — CQRS via MediatR, Repository/Unit-of-Work, and Dependency Inversion

**Status:** Accepted · **Date:** 2026-06-03 · **Scope:** `apps/NextAtlet.Server`
**Supersedes:** the hand-rolled `*Command`/`*Query` classes and the `Application → Infrastructure` reference described in earlier Step-1 notes.

---

## Context

Step 1 shipped three features (`CreateAthlete`, `GetDraftConfig`, `UpdateDraftConfig`) as plain classes with an `ExecuteAsync` method that injected `NextAtletDbContext` directly. DB access and orchestration were fused, the `Application` project referenced `Infrastructure` (dependency arrow pointing the wrong way for testability/layering), and error handling was per-action `try/catch` in the controller.

## Decisions

1. **CQRS via MediatR.** Features are `IRequest` + `IRequestHandler`, dispatched from controllers via `ISender`. Controllers are thin (`return Ok(await _sender.Send(request))`).
2. **Repository + Unit of Work.** Handlers are orchestrators: they read/write via per-aggregate repository interfaces (`IAthleteProfileRepository`, `IUserRepository`, `IProfileLoginRepository`, `IThemeRepository`, `ISiteConfigRepository`) and commit once via `IUnitOfWork.SaveChangesAsync()`. No handler references `DbContext`. Interfaces are intention-revealing, **not** a generic `IRepository<T>` (avoids leaking `IQueryable`).
3. **Dependency inversion (Clean Architecture).** Abstractions live in `Application`; EF Core implementations live in `Infrastructure`, which now references `Application`. The previous `Application → Infrastructure` reference is removed.

```
Api ──► Application ◄── Infrastructure
          │                  │
          └──────► Domain ◄──┘
```

4. **Global exception handling (Model A — error codes).** Per-action `try/catch` is replaced by an `IExceptionHandler` (`GlobalExceptionHandler`). User-facing failures throw `DomainException(code, params)` → **400** + an `ApiError { errorCode, parameters }`; system failures are logged and returned as a generic **500** `internal_error` (no detail leaked). The backend never emits localized strings — the frontend resolves codes via its `da`/`en` catalog, guarded by a build-time code↔catalog test. (Supersedes the earlier ProblemDetails/`NotFoundException`→404 sketch.) Full plan: `error-handling-implementation-plan.md`; contract documented in `docs/01`.
5. **Test safety net.** An xUnit project (`NextAtlet.Application.Tests`) dispatches through a real MediatR + repository pipeline backed by EF Core InMemory, pinning behaviour across the refactor.

## MediatR licensing

MediatR went commercial on **2 July 2025** (v13+, dual RPL-1.5 / commercial). It is **free under the Community edition** for organizations under $5M gross annual revenue, non-profits, education, and non-production use; above that threshold it requires a paid license key.

**Decision:** start with **MediatR v13 Community**. The choice is reversible and does not lock us in — every feature is just a class implementing `IRequestHandler<TReq,TRes>`. If licensing policy changes, the portable fallbacks are:

- Pin **MediatR 12.x** (last MIT release) — identical API, frozen.
- **`martinothamar/Mediator`** (MIT, source-generated) — near-identical API, minor namespace changes.
- A ~40-line hand-rolled `ISender` over DI.

## Consequences

- **Positive:** testable handlers without a DB; query logic isolated in repositories; API contract decoupled from persistence; consistent error responses; correct layering.
- **Cost:** a small amount of plumbing (repository interfaces + impls, UoW) and one third-party dependency (MediatR) with the licensing caveat above.
- **Unchanged:** `EnsureCreated()` startup, the jsonb value conversions, and the DB schema — repositories sit on top of the same `DbContext`/EF config. No migration was required by this refactor.

## Not done (deliberately)

- `MediaAsset` repository — deferred until a feature needs it.
- FluentValidation / a MediatR `ValidationBehavior` — deferred (Q-D); add when validation grows beyond the in-handler checks.
- Moving slug/reserved/minor invariants into the domain (Q-C) — left in the handler for now.
