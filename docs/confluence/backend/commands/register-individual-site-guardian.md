# Register Individual Site (Guardian)

> **Source:** [`RegisterIndividualSiteGuardianCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Individuals/Registration/RegisterIndividualSiteGuardianCommand.cs)
> **Endpoint:** `POST /api/IndividualSites/guardian-register`
> **Controller:** [`IndividualSitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/IndividualSitesController.cs) · **Auth:** authenticated (`[Authorize]`)

## What it does

A **guardian** registers a site on behalf of a child. Same starter bundle as self-register, but the profile is created `guardian_controlled`, the caller is attached as a **guardian** `SiteLogin`, and there is **no child login** in v1. The "minor always has a guardian" invariant holds by construction.

## Request

```csharp
public record RegisterIndividualSiteGuardianCommand(
    string AuthProviderId,   // from token
    string Email,            // from token
    string ChildDisplayName,
    string Slug,
    DateTime ChildDateOfBirth,
    string DefaultLocaleId) : IRequest<Result<SiteResponse>>;
```

Controller binds [`RegisterIndividualSiteGuardianRequest`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Contracts/Individuals/Request/RegisterIndividualSiteGuardianRequest.cs).

| Field | Type | Required | Meaning |
|-------|------|----------|---------|
| `ChildDisplayName` | string | yes | The child's display name |
| `Slug` | string | yes | Unique, non-reserved slug |
| `ChildDateOfBirth` | DateTime | yes | Must be a minor (under 18) |
| `DefaultLocaleId` | string | yes | `da` or `en` |

## Response

`Result<SiteResponse>` → **200** with the same `SiteResponse` shape as [self-register](./register-individual-site-self.md#response).

## How it works

1. Reject if the child is an adult (`AgeBand.Adult`, i.e. 18+) → `guardian.cannot_register_adult`. (Under-13 **is** allowed here — the absolute floor only applies to self-register.)
2. Get-or-create the guardian `User`.
3. Create `Site` + `IndividualProfile` (control mode `guardian_controlled`) + default draft snapshot via the [shared base](./individual-site-registration-base.md).
4. Add a **guardian** `SiteLogin` (status `active`).
5. `SaveChangesAsync` once.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `guardian.cannot_register_adult` | Child is 18+ | 400 |
| `slug.already_taken` | Slug exists | 400 |
| `slug.reserved` | Slug is reserved | 400 |
| *(thrown)* `internal_error` | Classic theme not seeded | 500 |

## Dependencies

The shared registration base set only: `ISiteRepository`, `ISiteLoginRepository`, `IIndividualProfileRepository`, `IThemeRepository`, `ISiteSnapshotRepository`, `UserProvisioner`, `IClock`, `AgeThresholdOptions` (injected as the bare value type), `IUnitOfWork`.

## Transaction behaviour

One `SaveChangesAsync` covering `User` + `Site` + `IndividualProfile` + `SiteSnapshot` + guardian `SiteLogin`. **No consent token, no email.**

## Side effects

Creates `User` (if new), a public individual `Site`, a `guardian_controlled` `IndividualProfile`, a default draft snapshot, and a guardian `SiteLogin`.

## Gotchas

- **Known bug — stranded consent state.** The shared base sets `ConsentStateId = pending_guardian_consent` whenever the child is under 16 — *including on this flow*. But this flow issues **no consent token**, so a guardian-registered 10-year-old is permanently stuck in `pending_guardian_consent` with no way to clear it. This contradicts the intent that guardian-registered minors need no separate consent. Fix: set `not_required` on this path (or issue a self-consent token).
- A guardian may register **multiple** children — there is no idempotency guard.

## Related

- [Registration base](./individual-site-registration-base.md) · [Self register](./register-individual-site-self.md) · [Transfer control](./transfer-control.md)
