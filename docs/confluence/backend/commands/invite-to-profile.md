# Invite to Profile

> **Source:** [`InviteToProfileCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Invitations/Commands/InviteToProfileCommand.cs)
> **Endpoint:** `POST /api/IndividualSites/{id}/invite`
> **Controller:** [`IndividualSitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/IndividualSitesController.cs) · **Auth:** authenticated + active login on the site

## What it does

Invites someone (by email) to become a co-**owner** or **guardian** of an individual site. It issues an **Invitation** `ActionToken` and emails the invitee a link. No `SiteLogin` is created yet — that happens when the invitee accepts the token.

## Request

```csharp
public record InviteToProfileCommand(
    Guid SiteId,              // from route {id}
    string CallerAuthProviderId,
    string CallerEmail,
    string Email,             // invitee
    string RoleId) : IRequest<Result<InvitationResponse>>;
```

Controller binds [`InviteToSiteRequest`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Contracts/Invitations/Request/InviteToSiteRequest.cs) (`Email`, `Role`).

| Field | Type | Meaning |
|-------|------|---------|
| `Email` | string | Invitee's email (stored in the token payload) |
| `RoleId` | string | Must be `owner` or `guardian` |

## Response

`Result<InvitationResponse>` → **200**:

```json
{ "id": "token-guid", "targetProfileId": "site-guid", "email": "x@y.z", "role": "guardian", "expiresUtc": "…" }
```

`id` is the action-token GUID used in the accept URL (returned to the inviter by design, since the inviter is authorized).

## How it works

1. If `RoleId` is not `owner`/`guardian` → `invitation.role_invalid`.
2. Load the caller `User`; missing → 500.
3. Load the `IndividualProfile` for the site; missing → `site.not_found`.
4. If the caller has no active login on the site → `not_authorized`.
5. If inviting a `guardian` but the profile is not a minor → `guardian.cannot_register_adult`.
6. If an identical pending invite exists (same site + email + role) → `invitation.already_pending`.
7. Issue an Invitation `ActionToken` (expires in `InvitationOptions.ExpiryDays`).
8. `SaveChangesAsync`; **after commit**, send the invite email.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `invitation.role_invalid` | Role not owner/guardian | 400 |
| `site.not_found` | No individual profile for the site id | 400 |
| `not_authorized` | Caller has no active login on the site | 400 |
| `guardian.cannot_register_adult` | Inviting a guardian to a non-minor | 400 |
| `invitation.already_pending` | Duplicate pending invite | 400 |

## Dependencies

`IUserRepository`, `ISiteLoginRepository`, `IIndividualProfileRepository`, `IActionTokenRepository`, `IEmailService`, `IUnitOfWork`, `IClock`, `IOptions<InvitationOptions>`.

## Transaction behaviour

One `SaveChangesAsync` (the token only); email after commit, best-effort.

## Side effects

Creates an Invitation `ActionToken`; sends one invite email. No `SiteLogin` here (created at accept time).

## Gotchas

- **Organization sites cannot be invited to at all** — only `owner`/`guardian` roles are accepted, even though the accept-side `InvitationStrategy` can handle org roles.
- **Any** active login (owner *or* guardian, even a read-only party) may invite.
- `CallerEmail` is carried on the command but never used.
- The `InvitationResponse.TargetProfileId` field actually holds the **site** id, not a profile id.
- The `InviteToSiteRequest` code comment says roles are `athlete_owner`/`guardian` — wrong; the accepted ids are `owner`/`guardian`.

## Related

- [Accept action token](./accept-action-token.md) · [Action token strategies](./action-token-strategies.md)
