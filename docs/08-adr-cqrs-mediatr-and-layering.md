# 08 · ADR — CQRS via MediatR, Repository/Unit-of-Work, and Layering

**Status:** Accepted and implemented (with the caveats noted).

## Context

The backend needs a clear write/read organization, testable business logic, and a clean separation between HTTP, orchestration, domain rules, and persistence.

## Decision

1. **CQRS-lite via MediatR 13.** Writes are commands, reads are queries — records implementing `IRequest<T>`, each with a co-located handler under `Features/**`. Controllers dispatch via `ISender.Send` and contain no logic.
2. **Repository + Unit of Work.** Persistence is abstracted behind intention-revealing interfaces in `NextAtlet.Application` (`ISiteRepository`, `IUnitOfWork`, …), implemented over EF Core in `NextAtlet.Infrastructure` (`EfUnitOfWork`, `NextAtletDbContext`). Handlers never touch `DbContext`.
3. **Dependency inversion / Clean layering.** `Api → Application ← Infrastructure`, both `→ Domain`. Domain depends on nothing (a lone EF package reference on the Domain csproj is vestigial and unused).
4. **Result<T> for expected failures**, exceptions for system faults. A global `ResultFilter` and `GlobalExceptionHandler` map both to the `ApiError` shape.

## Consequences and current reality

- **MediatR is pinned to v13** (Community). Handler/`IRequest` code is portable to MediatR 12 (MIT) or a hand-rolled `ISender` if licensing policy changes.
- **There are NO MediatR pipeline behaviours.** No cross-cutting validation, logging, or transaction behaviour was added. Validation is hand-rolled inside each handler; mapping uses static mappers. If you want validation/logging behaviours, they are not there yet.
- **There is NO explicit transaction API.** `EfUnitOfWork.SaveChangesAsync` just calls `DbContext.SaveChangesAsync`. Atomicity relies on EF's implicit single-`SaveChanges` transaction over the shared scoped `DbContext`. **Anything spanning two `SaveChangesAsync` calls is not atomic** — keep a handler's writes in one save.
- Each handler **owns its own commit** — it calls `SaveChangesAsync` once at the end. The base registration handler stages but never commits; the concrete handler commits.
- The **editor read/write split** (public cached path vs authenticated editor path) is an orthogonal design goal that is only partly realised — the public render path and the draft-write path are not built.

## Alternatives considered

- **Full CQRS with separate read/write models** — rejected as overkill for the MVP.
- **Specification pattern for authorization** — not used; authorization is `PermissionResolver` + `ControlMode` presets in the Domain.
- **FluentValidation / AutoMapper** — not referenced; validation and mapping are hand-written.

## Related

- [`01-architecture.md`](01-architecture.md) · [`07-patterns-and-build-order.md`](07-patterns-and-build-order.md) · [`confluence/02-architecture.md`](confluence/02-architecture.md)
