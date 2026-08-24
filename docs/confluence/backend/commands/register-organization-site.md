# Register Organization Site

> **Source:** [`RegisterOrganizationSiteCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Organizations/Registration/RegisterOrganizationSiteCommand.cs)
> **Endpoint:** `POST /api/OrganizationSites/club-register`
> **Controller:** [`OrganizationSitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/OrganizationSitesController.cs) · **Auth:** authenticated (`[Authorize]`)

## What it does

Registers a club/organization site: creates a `Site` (type `organization`), an `OrganizationProfile`, and a default draft `SiteSnapshot`. The organization type is hardcoded to `club` by the controller.

## Request

```csharp
public record RegisterOrganizationSiteCommand(
    string AuthProviderId,
    string Email,
    string Slug,
    string DisplayName,
    string PlanTierId,
    string DefaultLocaleId,
    string OrganizationTypeId) : IRequest<Result<SiteResponse>>;
```

Controller binds [`ClubRegisterRequest`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Contracts/Organizations/Request/ClubRegisterRequest.cs) (`DisplayName`, `Slug`, `PlanTierId`, `DefaultLocaleId`) and hardcodes `OrganizationTypeId = OrganizationType.Club.Id`.

| Field | Type | Required | Meaning |
|-------|------|----------|---------|
| `DisplayName` | string | yes | Club display name |
| `Slug` | string | yes | Unique, non-reserved |
| `PlanTierId` | string | yes | Stored on `OrganizationProfile.OrganizationTierId` (not validated) |
| `DefaultLocaleId` | string | yes | `da` or `en` |

## Response

`Result<SiteResponse>` → **200** with the standard `SiteResponse`.

## How it works

1. Get-or-create the `User` (**then never used** — see gotchas).
2. If slug exists → `slug.already_taken`. (Slug is **not** lowercased here, unlike the individual flow.)
3. If slug is reserved (case-insensitive) → `slug.reserved`.
4. Create `Site` (type `organization`).
5. Create `OrganizationProfile` (`OrganizationTierId = PlanTierId`, `VerificationStatusId = pending`, `AthleteSlotCount = 10`, `IsServerManaged = false`).
6. Attach a default draft snapshot (its own private copy of the base method).
7. `SaveChangesAsync` once; return the mapped DTO.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `slug.already_taken` | Slug exists | 400 |
| `slug.reserved` | Slug is reserved | 400 |
| *(thrown)* `internal_error` | Missing Classic theme, or invalid `DefaultLocaleId` in the mapper *after* the site is committed | 500 |

## Dependencies

`ISiteRepository`, `IOrganizationProfileRepository`, `IThemeRepository`, `ISiteSnapshotRepository`, `UserProvisioner`, `IUnitOfWork`.

## Transaction behaviour

One `SaveChangesAsync` covering `User` + `Site` + `OrganizationProfile` + `SiteSnapshot`.

## Side effects

Creates `User` (if new), an organization `Site`, an `OrganizationProfile` (pending verification), and a default draft snapshot.

## Gotchas

- **No `SiteLogin` is created.** The registrant ends up with *no* login on the org site they just created — nobody can invite to it, and `/api/Me` won't see it. This looks unfinished.
- **Slug is not lowercased** (individual flow lowercases), so `MyClub` and `myclub` become distinct sites.
- `PlanTierId`, `OrganizationTypeId`, and `DefaultLocaleId` are **not validated** at write time; a bad `DefaultLocaleId` only blows up in the mapper *after* the row is committed (a 500 with an orphaned site).
- Registration is unlimited — one user can create arbitrarily many org sites.
- The `MapToDto` projection is a third duplicate of `SiteMapper.ToResponse`.
- The controller's XML doc is copy-pasted from self-register and describes the wrong flow.

## Related

- [Send official email verification](./send-official-email-verification.md) · [Registration base](./individual-site-registration-base.md)
