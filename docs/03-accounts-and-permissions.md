# 03 · Accounts & Permissions

## Users and logins

- **`User`** — an Auth0 identity keyed by `sub` (`AuthProviderId`). No password. Provisioned **just-in-time** by `UserProvisioner` on first registration (matched by `sub` only, never email). `GET /api/Me` deliberately does *not* provision, so an invited person sees their pending invite before any `User` row exists.
- **`SiteLogin`** — grants a `User` access to a `Site` in a role. One row per (user, site), enforced by a unique `(UserId, SiteId)` index. Roles: `owner`, `guardian`. Status: `pending` / `active` / `revoked` — but the factory methods only ever create `active` logins; no `pending`-creating path exists.

## The permission model

Authorization is computed at request time by [`PermissionResolver`](../apps/NextAtlet.Server/NextAtlet.Domain/Authorization/PermissionResolver.cs) (Domain, singleton). It takes a `SiteLogin.SiteRoleId` and the profile's `ControlModeId` and returns a `SitePermissions` preset. **Nothing is stored per login.** Age is *not* an input — it only influences the model at registration time.

### `Resolve(login, profile)` matrix

| ControlMode | `owner` (athlete) | `guardian` | other |
|-------------|-------------------|-----------|-------|
| `athlete_controlled` | FullControl | ReadOnly | None |
| `guardian_controlled` | ReadOnly | FullControl | None |
| `athlete_controlled_shared` | FullControl | EditOnly | None |
| `guardian_controlled_shared` | EditOnly | FullControl | None |

### `SitePermissions` presets

| Preset | Edit | Publish | Approve | Media | Memberships |
|--------|------|---------|---------|-------|-------------|
| None | ✗ | ✗ | ✗ | ✗ | ✗ |
| ReadOnly | ✗ | ✗ | ✗ | ✗ | ✗ |
| EditOnly | ✓ | ✗ | ✗ | ✓ | ✗ |
| FullControl | ✓ | ✓ | ✓ | ✓ | ✓ |

> `None` and `ReadOnly` are flag-identical (kept distinct "so it can diverge later"), so `==` can't tell them apart.
>
> **Caveat:** `Resolve` ignores `login.StatusId` — safety depends on callers going through `ISiteLoginRepository.GetActiveLoginAsync` (which filters `StatusId == active`).

### Dead: `LoginPermissions`

The jsonb `SiteLogins.Permissions` column and its `LoginPermissions` value object (five `MinorCan*` booleans) are the **old, superseded** stored-permission model. It's always null and never read — the live model is `PermissionResolver`.

## Control modes and consent (two orthogonal axes)

- **ControlMode** (stored, explicit, never age-derived): `athlete_controlled` | `guardian_controlled` | `*_shared`. Flipped by `transfer-control` (between sides) and `collaboration` (toggles the `_shared` variant).
- **ConsentState** (GDPR publish gate): `not_required` | `pending_guardian_consent` | `consented`. Set at registration and lifted by accepting a consent `ActionToken`.

## Age policy

[`AgePolicy`](../apps/NextAtlet.Server/NextAtlet.Domain/Policies/AgePolicy.cs) computes age bands. Thresholds come from `AgeThresholdOptions`: `AbsoluteMinimumAge = 13`, `SelfConsentAge = 16`, `GuardianBoundary = 18`.

- Self-register rejected below 13.
- Self-register below 16 requires a guardian email and issues a consent token.
- Guardian-register rejected for adults (18+); allowed for any minor.

> `AgePolicy.BandToday` hardcodes 13/16/18, while `RequiresGuardianConsent` takes the configured value — the two can disagree if the option is changed. `IndividualProfile.IsMinor()` duplicates this with a hardcoded 18 and is used by nobody.

## The invite / consent / verification flows

All three emailed flows are `ActionToken`s dispatched by a strategy on accept. **Known gap:** neither the invitation nor consent strategy checks that the accepting user's email matches the token payload — the link-holder gets the role. See [`confluence/backend/authentication-and-tokens.md`](confluence/backend/authentication-and-tokens.md) and [`confluence/backend/commands/action-token-strategies.md`](confluence/backend/commands/action-token-strategies.md).
