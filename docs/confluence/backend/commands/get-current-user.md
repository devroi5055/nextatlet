# Get Current User (the decision gate)

> **Source:** [`GetCurrentUserQuery.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Identity/GetCurrentUserQuery.cs) · [`UserProvisioner.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Identity/UserProvisioner.cs)
> **Endpoint:** `GET /api/Me`
> **Controller:** [`MeController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/MeController.cs) · **Auth:** authenticated (`[Authorize]`)

## What it does

The "who am I and what should I see" query. The frontend calls it right after login to decide whether to send the user to onboarding or to the dashboard. It reports whether the caller is registered, their role, their profile id, control state, and any pending guardian invites — **without** creating a `User` row (so an invited person can see their invite before they've ever registered).

## Request

```csharp
public record GetCurrentUserQuery(string AuthProviderId, string Email) : IRequest<MeResponse>;
```

Both fields come from the JWT claims. Note this returns a **plain `MeResponse`**, not a `Result`, so it always yields 200.

## Response

```csharp
public record MeResponse
{
    public required bool Registered { get; init; }
    public string? Role { get; init; }                  // "owner" | "guardian" | null
    public Guid? ProfileId { get; init; }               // an IndividualProfile id
    public ControlModes? ControlMode { get; init; }     // whole enumeration object
    public bool IsInControl { get; init; }
    public bool CanEdit { get; init; }
    public required IReadOnlyList<Guid> GuardedProfileIds { get; init; }  // actually SITE ids
    public int PendingGuardianInvites { get; init; }
}
```

## How it works

1. Count pending invites for the caller's **email** (surfaced even before a `User` row exists).
2. Look up the `User` by subject.
3. **No user:** `Registered = false`; `Role = "guardian"` if there are pending invites, else null.
4. Otherwise gather the user's guarded site ids and any owned site.
5. **No owned site:** if there are guarded sites or pending invites → unregistered guardian response; else fully-empty response.
6. With an owned site: load its `IndividualProfile`, compute `IsInControl` / `CanEdit` via `PermissionResolver`, and return a registered owner response with `ProfileId`, `ControlMode`, guarded sites, and pending invites.

## `UserProvisioner`

`GetCurrentUserQuery` deliberately does **not** provision. The `UserProvisioner` service (used by the registration flows) get-or-creates a `User` keyed **by subject only** (never by email), and does **not** call `SaveChangesAsync` — the calling handler owns the commit. A pending invitee is represented by an `ActionToken`, not a placeholder `User` row.

## Validation and error codes

None. The only failure is a thrown invariant → 500 (see gotchas).

## Dependencies

`IUserRepository`, `IIndividualProfileRepository`, `ISiteRepository`, `ISiteLoginRepository`, `IActionTokenRepository`, `PermissionResolver`.

## Transaction behaviour

None — read-only.

## Side effects

None.

## Gotchas

- **`GuardedProfileIds` holds SITE ids**, while `ProfileId` holds a *profile* id — different identifier spaces under similar names.
- **An org-site owner hitting `/api/Me` gets a 500.** If `GetOwnedByUserIdAsync` ever matches an organization site, the code then demands an `IndividualProfile` for it and throws when there isn't one.
- `CountPendingInvitesByEmailAsync` loads **every pending invite in the entire database** into memory and filters client-side (the JSONB payload is opaque to SQL). This is a scalability landmine on every `/api/Me` call.
- `ControlMode` serializes as a whole enumeration object (`{ id, title, description }`), whereas `Role` is a bare id string — an inconsistency for the frontend.

## Related

- [Frontend: Onboarding Flow](../../frontend/onboarding-flow.md) · [Accounts & permissions](../../03-data-model-erd.md)
