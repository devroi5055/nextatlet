# Accept Action Token

> **Source:** [`AcceptActionTokenCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ActionTokens/Commands/AcceptActionTokenCommand.cs)
> **Endpoint:** `POST /api/action-tokens/{id}/accept`
> **Controller:** [`ActionTokensController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/ActionTokensController.cs) · **Auth:** authenticated (via global fallback policy)

## What it does

Accepts an emailed action link. Every emailed flow — invitation, guardian consent, org email verification — ends here. This handler validates the token, dispatches to a **strategy** by token type, stamps the token as used, and commits everything in one transaction.

The token's `{id}` in the URL **is the secret** — the row's GUID is the link key; there is no separate token column.

## Request

```csharp
public record AcceptActionTokenCommand(Guid TokenId, string? AuthProviderId) : IRequest<Result>;
```

The controller passes the route `{id}` and `User.TryGetAuthProviderId()` (which returns `null` rather than throwing if the subject claim is absent).

## Response

Non-generic `Result` → **204 No Content** on success, **400** + `ApiError` on failure.

## How it works

1. Load the token by id; missing → `action_token.not_found`.
2. If expired → `action_token.expired`.
3. If already used (`AcceptedUtc` set) → `action_token.already_used`.
4. Resolve the strategy from the [strategy registry](./action-token-strategies.md) by token type.
5. **Auth validation:**
   - if the strategy's `authRequired == false` → actor is `null` (anonymous accept)
   - else if `AuthProviderId` is null → `not_authorized`
   - else look up the `User`; if there's no row → throw (500, "provisioning invariant violated")
6. Run `strategy.ExecuteAsync(token, actor)`. On failure the token is **not** stamped.
7. `token.Accept(now)` — sets `AcceptedUtc` (single use).
8. `SaveChangesAsync` once — covers the token stamp *and* everything the strategy staged.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `action_token.not_found` | No token with that id | 400 |
| `action_token.expired` | Past its expiry | 400 |
| `action_token.already_used` | Already accepted | 400 |
| `not_authorized` | Auth required but no subject | 400 |
| *(strategy)* `consent.not_needed`, `organization.profile_not_found` | See [strategies](./action-token-strategies.md) | 400 |

## Dependencies

`IActionTokenRepository`, `ActionTokenStrategyRegistry`, `UserProvisioner`, `IClock`, `IUnitOfWork`.

## Transaction behaviour

Exactly one `SaveChangesAsync` at the end. Strategies never commit — they only stage. A strategy failure short-circuits before any commit, so nothing is persisted.

## Side effects

Stamps `AcceptedUtc`, plus whatever the dispatched strategy does (creates a `SiteLogin`, or a `GuardianConsent`, or flips an org's verification status).

## Gotchas

- **The anonymous-accept path is effectively unreachable.** `OrgEmailVerificationStrategy` sets `authRequired == false`, but the controller has **no `[AllowAnonymous]`**, and the global fallback policy rejects anonymous callers with 401 before the handler runs. So the "anonymous org verification" branch is dead in production, and `OrgVerification.VerifiedByUserId` is always null.
- The token's `IsExpired` uses `DateTime.UtcNow` directly rather than the injected `IClock`, so expiry can't be tested deterministically.
- An unknown token type or an unregistered strategy throws (→ 500) rather than returning a clean error.

## Related

- [Action token strategies](./action-token-strategies.md) · [Invite to profile](./invite-to-profile.md) · [Backend: Authentication & Tokens](../authentication-and-tokens.md)
