# List Club Officials

> **Source:** [`ListClubOfficials.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ClubRegistry/ListClubOfficials.cs)
> **Endpoint:** **none — there is no controller action for this handler**
> **Auth:** n/a

## What it does

Returns the officials (contact people) for a registry `Club`. The handler is fully implemented and tested, but **it is not wired to any HTTP route**, so it is currently unreachable.

## Request

```csharp
public record ListClubOfficialsCommand(Guid ClubId) : IRequest<Result<List<ClubOfficial>>>;
```

## Response

`Result<List<ClubOfficial>>` — **raw domain entities**.

## How it works

1. Load the club by id; missing → `club.not_found`.
2. Return `club.Officials.ToList()`.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `club.not_found` | Club id not found | 400 |

## Dependencies

`IClubRepository`.

## Transaction behaviour

None — read-only.

## Side effects

None.

## Gotchas

- **No HTTP route.** `ClubsController` only exposes `scrape`, `add-sports`, and `remove-sports`. This handler can't be reached over the API despite being tested.
- **Would leak raw entities.** It returns `ClubOfficial` domain entities directly (the only handler that does), so if it were wired up it would expose every official's `email` and `phone` plus audit timestamps. Add a DTO before exposing it.

## Related

- [Scrape clubs](./scrape-clubs.md) · [Add and remove sports](./add-and-remove-sports.md)
