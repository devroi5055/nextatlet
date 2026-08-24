# Send Official Email Verification

> **Source:** [`SendOfficialEmailVerificationCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Organizations/Verification/SendOfficialEmailVerificationCommand.cs)
> **Endpoint:** `POST /api/OrganizationSites/send-offical-email-verification` *(note the "offical" typo in the route)*
> **Controller:** [`OrganizationSitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/OrganizationSitesController.cs) · **Auth:** authenticated

## What it does

Starts organization verification by emailing a **club official** (from the scraped [club registry](./scrape-clubs.md)) a verification link. Accepting that link marks the organization as verified. The email goes to the official's registry address, not to anything the caller supplies.

## Request

```csharp
public record SendOfficialEmailVerificationCommand(
    string AuthProviderId,
    string Email,          // caller's claim email
    Guid OrgSiteId,
    Guid ClubOfficialId) : IRequest<Result<Guid>>;
```

Controller binds [`SendOfficialEmailVerificationRequest`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Contracts/Organizations/Request/SendOfficialEmailVerificationRequest.cs) (`OrgSiteId`, `ClubOfficialId`).

## Response

`Result<Guid>` → **200** with the raw action-token GUID as the body.

## How it works

1. Load the `ClubOfficial` by id; missing → `verification.official_not_found`.
2. If the official has no email → `verification.official_email_missing`.
3. Look up the caller `User` (may be null).
4. Build an `OrgEmailVerificationPayload` and issue an `OrgEmailVerification` `ActionToken`.
5. `SaveChangesAsync`; **after commit**, email the **official's registry address** the accept link.
6. Return the token id.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `verification.official_not_found` | Club official id not found | 400 |
| `verification.official_email_missing` | Official has no/blank email | 400 |

## Dependencies

`IClubRepository`, `IActionTokenRepository`, `IEmailService`, `IUnitOfWork`, `IClock`, `IOptions<InvitationOptions>`, `UserProvisioner`.

## Transaction behaviour

One `SaveChangesAsync` (the token); email after commit.

## Side effects

Creates an `OrgEmailVerification` `ActionToken`; sends one verification email to the registry address.

## Gotchas — the most serious authorization gap in the backend

- **No ownership check, and the token id is returned in the body.** There is no check that the caller owns or controls `OrgSiteId`, nor that `OrgSiteId` even exists or is an organization site. Combined with the token id being returned in the 200 response **and** `OrgEmailVerificationStrategy.authRequired == false`, any authenticated user can: (1) pick an arbitrary `OrgSiteId`, (2) pick any club official id, (3) receive the token id, and (4) immediately `POST /api/action-tokens/{id}/accept` to mark that organization **verified** — without ever seeing the official's mailbox. **Close this before production.**
- The audit's `VerifiedByEmail` is set to the **caller's** email (`request.Email`), not the official's address — the recorded address is wrong.
- `GetOfficialByIdAsync` ignores the cancellation token it's handed.

## Related

- [Register organization site](./register-organization-site.md) · [Action token strategies](./action-token-strategies.md) · [Scrape clubs](./scrape-clubs.md)
