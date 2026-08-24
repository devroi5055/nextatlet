# Transfer Control

> **Source:** [`TransferControlCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Individuals/Control/TransferControlCommand.cs)
> **Endpoint:** `POST /api/IndividualSites/{id}/transfer-control`
> **Controller:** [`IndividualSitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/IndividualSitesController.cs) · **Auth:** authenticated + must be the current controller

## What it does

Moves control of a profile between the athlete and the guardian — flipping `ControlModeId` to `athlete_controlled` or `guardian_controlled`. Only the party that currently controls the profile may initiate. Any active collaboration (shared mode) is cleared by resetting to the non-shared mode.

## Request

```csharp
public record TransferControlCommand(
    Guid ProfileId,            // NOTE: profile id (route {id})
    string CallerAuthProviderId,
    string TransferTo) : IRequest<Result>;   // "athlete" | "guardian"
```

Controller binds [`TransferControlRequest`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Contracts/Individuals/Request/TransforControlRequest.cs) (`To`).

## Response

Non-generic `Result` → **204 No Content** on success.

## How it works

1. If `TransferTo` is not `athlete`/`guardian` → `control.transfer_target_invalid`.
2. Load the profile; missing → `individual.profile_not_found`.
3. Load the caller; missing → 500.
4. Load the caller's active login on `profile.SiteId`; if none, or the caller is not the controller → `not_authorized`.
5. **To athlete:** reject if the athlete is under 13 (`control.athlete_too_young`) or if there's no active owner login (`control.no_athlete_login`); else set `athlete_controlled`.
6. **To guardian:** reject if there's no active guardian login (`control.no_guardian_login`); else set `guardian_controlled`.
7. `SaveChangesAsync`.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `control.transfer_target_invalid` | Target not athlete/guardian | 400 |
| `individual.profile_not_found` | Profile id not found | 400 |
| `not_authorized` | Not an active controller | 400 |
| `control.athlete_too_young` | Athlete under 13 | 400 |
| `control.no_athlete_login` | No active owner login | 400 |
| `control.no_guardian_login` | No active guardian login | 400 |

## Dependencies

`IIndividualProfileRepository`, `ISiteLoginRepository`, `IUserRepository`, `PermissionResolver`, `IUnitOfWork`, `IClock`. (`ISiteRepository` is injected but unused.)

## Transaction behaviour

One `SaveChangesAsync`.

## Side effects

Mutates `IndividualProfile.ControlModeId`.

## Gotchas — this handler is currently BROKEN

Two verified live bugs make this endpoint fail in production:

1. **Wrong id passed to the login checks.** `HasActiveOwnerLoginAsync` / `HasActiveGuardianLoginAsync` take a **site** id (they filter `l.SiteId == …`), but the handler passes `request.ProfileId`. So in production the checks always return `false` and **every transfer returns `control.no_athlete_login` / `control.no_guardian_login`**. (Step 4 correctly used `profile.SiteId` — the inconsistency is within the same handler.)
2. **`GetByIdAsync` throws.** `IndividualProfileRepository.GetByIdAsync` calls `FindAsync(id, cancellationToken)`, which mis-binds the cancellation token as a *second key value* → EF throws at runtime. So step 2 actually 500s before bug #1 is reached.

The unit tests stub the mocks with `profileId`, so they **mirror the first bug and pass** — the tests do not catch it.

Also note: the sibling [collaboration](./set-collaboration.md) endpoint takes a **site** id in its route, while this one takes a **profile** id — an inconsistency in the same controller.

## Related

- [Set collaboration](./set-collaboration.md) · [Data model](../../03-data-model-erd.md)
