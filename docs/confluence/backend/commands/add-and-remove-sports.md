# Add / Remove Club Sports

> **Source:** [`AddSports.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ClubRegistry/AddSports.cs) · [`RemoveSports.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ClubRegistry/RemoveSports.cs)
> **Endpoints:** `PUT /api/clubs/add-sports` · `PUT /api/clubs/remove-sports`
> **Controller:** [`ClubsController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/ClubsController.cs) · **Auth:** `[AllowAnonymous]`

## What it does

Edits the list of sports attached to a registry `Club`. `AddSports` unions in new sport ids; `RemoveSports` removes them. Each returns the delta (what was actually added/removed).

## Request

```csharp
public record AddSportsCommand(Guid id, List<string> sportIds) : IRequest<Result<IEnumerable<string>>>;
public record RemoveSportsCommand(Guid id, List<string> sportIds) : IRequest<Result<IEnumerable<string>>>;
```

Binding is unusual: with `[ApiController]` inference, `id` binds from the **query string** and `sportIds` from the **body**.

## Response

`Result<IEnumerable<string>>` → **200** with the sports actually added (`AddSports`) or removed (`RemoveSports`).

## How it works

1. Load the club by id; missing → `club.not_found`.
2. `club.AddSports(...)` (`Except` then `Union`) or `club.RemoveSports(...)` (`Intersect` then `Except`).
3. `SaveChangesAsync`; return the delta.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `club.not_found` | Club id not found | 400 |

**No validation that the sport ids are real `Sport` enumeration values** — arbitrary strings are persisted.

## Dependencies

`IClubRepository`, `IUnitOfWork`.

## Transaction behaviour

One `SaveChangesAsync` after the mutation.

## Side effects

Mutates `Club.SportIds`.

## Gotchas

- **Unauthenticated mutation.** Both are `[AllowAnonymous]` — any anonymous caller can edit any club's sports.
- Arbitrary strings are accepted as sport ids (no enumeration validation).
- The camelCase record parameters (`id`, `sportIds`) and query+body binding are non-obvious.

## Related

- [Scrape clubs](./scrape-clubs.md) · [List club officials](./list-club-officials.md)
