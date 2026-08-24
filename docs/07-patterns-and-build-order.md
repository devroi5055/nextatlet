# 07 · Patterns, Conventions & How-To

Conventions a contributor must follow, and recipes for common changes.

## Design patterns in use

- **CQRS-lite via MediatR** — commands/queries as `IRequest<T>` records; handlers under `Features/**`; controllers only `_sender.Send`.
- **Repository + Unit of Work** — interfaces in Application, EF impls in Infrastructure; handlers commit once via `IUnitOfWork`.
- **Result<T> + global filter** — handlers return results; `ResultFilter` shapes HTTP; exceptions → `GlobalExceptionHandler`.
- **Strategy + Registry** — `IActionTokenStrategy` per token type; `SectionTypeRegistry`.
- **Smart-enumerations** — string-id value types with bilingual titles.
- **Static mappers** — hand-written DTO mapping; never expose EF entities.
- **Options pattern** — `AgeThresholdOptions`, `EmailOptions`, `InvitationOptions`, `TermsOptions`.
- **PermissionResolver + ControlMode presets** for authorization (not a Specification pattern).

There are **no MediatR pipeline behaviours** and **no explicit DB transactions** — keep a handler's writes within a single `SaveChangesAsync`.

## How to add a command or query

1. Add a record `record FooCommand(...) : IRequest<Result<TResponse>>` under the right `Features/**` folder, with its `FooCommandHandler` in the same file/folder.
2. Inject repository **interfaces** and services (never `DbContext`). Read/stage/mutate, then call `_unitOfWork.SaveChangesAsync(ct)` **once**.
3. Return `Error.FromCode(ErrorCodes.Xxx)` for failures, or the value for success.
4. Add the endpoint to a controller as `Ok(await _sender.Send(new FooCommand(User.GetAuthProviderId(), …)))`. Pull identity from claims, not the body.
5. Add a DTO under `Contracts/**` if the request/response shape is non-trivial.
6. Write a handler test in `tests/NextAtlet.Application.Tests` (xUnit + NSubstitute + AutoFixture).

## How to add an entity + migration

1. Add the entity to `NextAtlet.Domain/Entities/**` (derive from `AuditableEntity` or `CreatedOnlyEntity`).
2. Add an EF configuration under `Infrastructure/Persistence/Configurations/**` and a `DbSet` on `NextAtletDbContext`.
3. `dotnet ef migrations add <Name> --project …Infrastructure --startup-project …Api`.
4. Remember: Development startup **drops and recreates** the DB, so the migration runs automatically on next run.

## How to add a section type

Two section types exist today: `hero`, `bio` ([`ValueObjects/Sections/`](../apps/NextAtlet.Server/NextAtlet.Domain/ValueObjects/Sections)). To add one:

1. Subclass `SectionData` with a `TypeId` const.
2. Add a `[JsonDerivedType(typeof(YourSection), YourSection.TypeId)]` attribute on `SectionData`.
3. Add an `ISectionValidator` under `Infrastructure/Services/SectionRegistry/` and register it in `SectionTypeRegistry`.

> Note the validators/registry are currently **not wired to any live endpoint** (the draft-write path was removed), so this is prep work until the editor is rebuilt.

## How to add an enumeration value

Add a `public static readonly` field to the enumeration class and include it in `All`. Because enumeration ids are stored as raw strings with no DB constraint, also add any needed migration/seed handling — but there's no lookup table to update.

## Error-code conventions

- Add the constant to [`ErrorCodes.cs`](../apps/NextAtlet.Server/NextAtlet.Application/Common/Errors/ErrorCodes.cs) with a stable dotted string (e.g. `slug.already_taken`).
- Remember **every failure is 400 on the wire** regardless of the comment grouping.
- The frontend is supposed to translate these codes, but currently doesn't — if you rely on that, add the translations to `messages/*.json`.

## Testing conventions

- Tests live at repo root `tests/NextAtlet.Application.Tests` (the only project with source). xUnit + NSubstitute + AutoFixture; fixed clock `2025-06-15T12:00:00Z`.
- There are **no** API/integration tests (`WebApplicationFactory`) and no Domain/Infrastructure test source — those projects are empty shells.

## Naming traps (file name ≠ type name)

Be aware when navigating — several files are misnamed relative to their type:

| File | Declares |
|------|----------|
| `Common/IRetireable.cs` | `IRetirable` |
| `Authorization/ProfilePermissions.cs` | `SitePermissions` |
| `ValueObjects/GuardianPermissions.cs` | `LoginPermissions` |
| `Abstractions/Persistence/IProfileLoginRepository.cs` | `ISiteLoginRepository` |
| `Abstractions/Services/ICvrHttpService.cs` | `ICvrLookupService` |
| `Abstractions/Services/ISportCanonicalizer.cs` | `IClubCanonicalizer` |
| `Features/Sites/GetDraftSiteSnapshotQuery.cs` | `GetDraftAthleteSiteSnapshotQuery` |
| `Contracts/.../TransforControlRequest.cs` | `TransferControlRequest` |
| `ValueObjects/Theme/Typograhpy.cs` | `Typography` |

Also: `IndividualSiteRegistrationHandlerBase` sits in namespace `…Features.Athletes.Commands`; the three action-token strategy classes have no namespace at all.

## Build order (remaining, roughly)

1. **Public render endpoint + Next.js renderer** ← next milestone
2. Publish flow + ISR/CDN + cache invalidation
3. Rebuild the section registry + draft-edit write path
4. Theme manifest system + theme picker
5. Athlete self-tiers + billing (`Plan`/`PlanPrice`/`Subscription`)
6. Media pipeline
7. Organizations + multi-user roles + club page engine
8. Memberships + history retention
9. `PerkResolver` + additive perk layer
10. Club showcases via published contracts
11. Change-request / approval workflow
12. Mentoring, versioning/history, custom subdomains

## Open decisions

Tier prices + slot counts; club-downgrade behaviour; self-serve vs admin-created org types; feature-gating granularity (code vs data rows); MobilePay via Stripe; proration policy; data retention on profile deletion; photoshoot scheduling. None are resolved.
