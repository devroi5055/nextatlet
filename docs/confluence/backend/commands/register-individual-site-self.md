# Register Individual Site (Self)

> **Source:** [`RegisterIndividualSiteSelfCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Individuals/Registration/RegisterIndividualSiteSelfCommand.cs)
> **Endpoint:** `POST /api/IndividualSites/self-register`
> **Controller:** [`IndividualSitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/IndividualSitesController.cs) · **Auth:** authenticated (`[Authorize]`)

## What it does

An athlete registers **their own** profile site. It creates the whole starter bundle in one transaction — a `Site`, an athlete `IndividualProfile`, a default draft `SiteSnapshot` (Classic theme, hero + bio sections), and an owner `SiteLogin` — and, if the athlete is below the self-consent age, issues a guardian-consent token and emails the guardian.

## Request

```csharp
public record RegisterIndividualSiteSelfCommand(
    string AuthProviderId,   // from token claim "sub"
    string Email,            // from token claim
    string DisplayName,
    string Slug,
    DateTime DateOfBirth,
    string DefaultLocaleId,
    string? GuardianEmail) : IRequest<Result<SiteResponse>>;
```

The controller binds the body DTO [`RegisterIndividualSiteSelfRequest`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Contracts/Individuals/Request/RegisterIndividualSiteSelfRequest.cs) and injects `AuthProviderId`/`Email` from the JWT.

| Field | Type | Required | Meaning |
|-------|------|----------|---------|
| `DisplayName` | string | yes | The athlete's display name |
| `Slug` | string | yes | URL slug; lowercased server-side; must be unique and not reserved |
| `DateOfBirth` | DateTime | yes | Drives age gating |
| `DefaultLocaleId` | string | yes | `da` or `en` |
| `GuardianEmail` | string? | conditionally | **Required if the athlete is under the self-consent age (16)**, and must not equal the athlete's own email |

## Response

`Result<SiteResponse>` → **200** with the site DTO:

```json
{
  "id": "guid",
  "slug": "ada-jensen",
  "displayName": "Ada Jensen",
  "defaultLocale": { "id": "en", "title": { "da": "Engelsk", "en": "English" }, "description": {…} },
  "visibilityState": { "id": "public", "title": {…}, "description": {…} }
}
```

## How it works

1. Reject if `age < AbsoluteMinimumAge` (13) → `registration.below_minimum_age`.
2. Compute `needsConsent = age < SelfConsentAge` (16).
3. If `needsConsent` and `GuardianEmail` is blank → `guardian.email_required`.
4. If `needsConsent` and `GuardianEmail == Email` → `guardian.email_required` (guardian can't be the athlete).
5. Get-or-create the `User` from the token (`UserProvisioner`).
6. If the user already owns a site → `site.already_exists` (one owned site per user).
7. Create the `Site` + `IndividualProfile` (control mode `athlete_controlled`) + default draft snapshot via the [shared base](./individual-site-registration-base.md). Consent state is `pending_guardian_consent` if `needsConsent`, else `not_required`.
8. Add an owner `SiteLogin` (status `active`).
9. If `needsConsent`, issue a **Consent** `ActionToken` (expires in `InvitationOptions.ExpiryDays`, default 7).
10. `SaveChangesAsync` once.
11. **After commit**, if a consent token was issued, send the guardian-consent email.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `registration.below_minimum_age` | Age < 13 | 400 |
| `guardian.email_required` | Under 16 and guardian email blank, **or** guardian email equals athlete email | 400 |
| `site.already_exists` | Caller already owns a site | 400 |
| `slug.already_taken` | Slug exists | 400 |
| `slug.reserved` | Slug is a reserved word | 400 |
| *(thrown)* `internal_error` | Classic theme not seeded → 500 | 500 |

## Dependencies

- `ISiteRepository`, `ISiteLoginRepository`, `IIndividualProfileRepository`, `IThemeRepository`, `ISiteSnapshotRepository` — persistence
- `UserProvisioner` — just-in-time user creation
- `IActionTokenRepository` — the consent token
- `IEmailService` — the guardian email
- `IClock`, `IUnitOfWork`, `IOptions<AgeThresholdOptions>`, `IOptions<TermsOptions>`, `IOptions<InvitationOptions>`

## Transaction behaviour

One `SaveChangesAsync` covering `User` + `Site` + `IndividualProfile` + `SiteSnapshot` + owner `SiteLogin` + optional consent `ActionToken`. Email is sent only after commit, so a rolled-back registration never mails.

## Side effects

Creates a `User` (if new), a public individual `Site`, an athlete-controlled `IndividualProfile`, a default draft snapshot (Classic theme, `#ffd700` accent / Inter font), an owner `SiteLogin`, optionally a consent token; sends one guardian-consent email.

## Gotchas

- **The new site is `public` immediately**, before any consent — `VisibilityStateId` is hardcoded to `"public"` in the base. A minor's site is publicly visible the moment it's created.
- The `13–17` comment on `GuardianEmail` is misleading — the code requires it below **16** (i.e. 13–15).
- `SportId` is hardcoded to `"judo"`.
- Missing Classic theme is a **500**, not a clean error code.

## Related

- [Registration base](./individual-site-registration-base.md) · [Guardian register](./register-individual-site-guardian.md) · [Accept action token](./accept-action-token.md) · [Action token strategies](./action-token-strategies.md)
