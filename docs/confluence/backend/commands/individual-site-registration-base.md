# Individual Site Registration — Shared Base

> **Source:** [`IndividualSiteRegistrationHandlerBase.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Individuals/Registration/IndividualSiteRegistrationHandlerBase.cs)
> **Endpoint:** none — this is an abstract base, not a handler
> **Auth:** n/a

## What it does

Both individual-registration handlers ([self](./register-individual-site-self.md) and [guardian](./register-individual-site-guardian.md)) share this abstract base. It holds the common logic for creating the `Site` + `IndividualProfile` + default draft `SiteSnapshot`, plus the slug rules and the consent-state decision. It never commits — the concrete handler owns the single `SaveChangesAsync`.

## Key method: `CreateIndividualProfileCoreAsync`

Signature: `(slug, displayName, dateOfBirth, defaultLocaleId, controlMode, ct) → Result<SiteResponse>`.

1. Lowercase the slug.
2. If the slug already exists → `slug.already_taken`.
3. If the slug is in the reserved list → `slug.reserved`.
4. Create the `Site` with `VisibilityStateId = "public"` (hardcoded), `SiteTypeId = individual`.
5. Decide consent: `consentIsRequired = AgePolicy.RequiresGuardianConsent(dob, now, SelfConsentAge)`.
6. Create the `IndividualProfile` with `SportId = "judo"` (hardcoded), `ConsentStateId = pending_guardian_consent` if consent is required else `not_required`, and the passed control mode.
7. Attach a default draft snapshot (see below).
8. Return the mapped `SiteResponse`.

### `AttachDefaultDraftSnapshotAsync`

Looks up the **"Classic"** theme by name — if missing, throws `InvalidOperationException("Classic theme not found")` (→ 500). Then creates a `SiteSnapshot` with the default layout and `GlobalSettings { AccentColor = "#ffd700", FontFamily = "Inter" }`, and sets `Site.CurrentDraftSnapshotId`.

### `CreateDefaultLayout`

Two sections: order 0 a **Hero** section (empty localized headline + subheading), order 1 a **Bio** section (empty localized bio). Each gets a fresh GUID.

## Reserved slugs

```
admin, api, about, contact, terms, privacy, login, signup, settings, dashboard
```

## Gotchas

- **Slug reserved-word list is duplicated.** This base has its own `ReservedSlugs` array; the organization handler uses a *different* copy in `Domain.strings.Strings`, with different comparison semantics (this one lowercases + ordinal; the org one uses `OrdinalIgnoreCase`).
- The `Site → SiteResponse` mapping (`MapToDto`) is a byte-for-byte duplicate of `SiteMapper.ToResponse` — the projection exists in three places.
- The namespace on this file is `NextAtlet.Application.Features.Athletes.Commands`, which does not match its folder (`Individuals/Registration`).
- `SportId` and `VisibilityStateId` are hardcoded literals, not driven by config or request.

## Related

- [Self register](./register-individual-site-self.md) · [Guardian register](./register-individual-site-guardian.md) · [Data model](../../03-data-model-erd.md)
