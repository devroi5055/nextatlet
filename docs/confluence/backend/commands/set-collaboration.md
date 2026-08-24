# Set Collaboration

> **Source:** [`SetCollaborationCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Individuals/Control/SetCollaborationCommand.cs)
> **Endpoint:** `POST /api/IndividualSites/{id}/collaboration`
> **Controller:** [`IndividualSitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/IndividualSitesController.cs) · **Auth:** authenticated + must be the current controller

## What it does

Turns **shared editing** on or off for a profile. It flips the currently-controlling side's mode between its plain and `_shared` variant (e.g. `athlete_controlled` ⇄ `athlete_controlled_shared`), letting the other party edit the draft without transferring control.

## Request

```csharp
public record SetCollaborationCommand(
    Guid SiteId,               // route {id} — a SITE id
    string CallerAuthProviderId,
    bool SharedEditing) : IRequest<Result>;
```

Controller binds [`SetCollaborationRequest`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Contracts/Individuals/Request/SetCollaborationRequest.cs) (`SharedEditing`).

## Response

Non-generic `Result` → **204** on success.

## How it works

1. Load the `IndividualProfile` by site id; missing → `site.not_found`.
2. Load the caller; missing → 500.
3. Load the caller's active login on the site; if none or not the controller → `not_authorized`.
4. Flip only the controlling side's shared flag:
   - `athlete_controlled` + true → `athlete_controlled_shared`
   - `athlete_controlled_shared` + false → `athlete_controlled`
   - `guardian_controlled` + true → `guardian_controlled_shared`
   - `guardian_controlled_shared` + false → `guardian_controlled`
   - anything else → no change (still a 204)
5. `SaveChangesAsync`.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `site.not_found` | No individual profile for the site id | 400 |
| `not_authorized` | Caller isn't an active controller | 400 |

## Dependencies

`ISiteLoginRepository`, `IIndividualProfileRepository`, `IUserRepository`, `PermissionResolver`, `IUnitOfWork`. (`ISiteRepository` injected but unused.)

## Transaction behaviour

One `SaveChangesAsync`, run even when the switch was a no-op.

## Side effects

Mutates `IndividualProfile.ControlModeId` only.

## Gotchas

- The route `{id}` is a **site** id here, but the sibling [transfer-control](./transfer-control.md) route uses a **profile** id — an inconsistency in the same controller.
- A local variable named `site` actually holds an `IndividualProfile` — a readability trap.
- The `_shared` variants still "belong" to the controlling side, so only that side can toggle collaboration.

## Related

- [Transfer control](./transfer-control.md) · [Accounts & permissions](../../03-data-model-erd.md)
