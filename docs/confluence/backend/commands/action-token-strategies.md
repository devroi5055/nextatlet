# Action Token Strategies

> **Source folder:** [`Features/ActionTokens/Strategies/`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ActionTokens/Strategies)
> **Auth:** dispatched from [Accept Action Token](./accept-action-token.md)

## What it does

There are three kinds of emailed action link, and each has a **strategy** that performs the actual work when the token is accepted. [`AcceptActionTokenCommand`](./accept-action-token.md) picks the right strategy by token type and runs it inside its transaction.

```csharp
public interface IActionTokenStrategy
{
    ActionTokenType ActionTokenType { get; }
    bool authRequired { get; }
    Task<Result> ExecuteAsync(ActionToken token, User? actorUser, CancellationToken ct);
}
```

## The registry

[`ActionTokenStrategyRegistry`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ActionTokens/Strategies/ActionTokenStrategyRegistry.cs) builds a `Dictionary<ActionTokenType, IActionTokenStrategy>` from the three DI-registered strategies. `Get(type)` throws `KeyNotFoundException` on a miss (no safe fallback). The three strategies are registered `Scoped` in `Program.cs`.

## Strategy: Consent

> [`ConsentStrategy.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ActionTokens/Strategies/ConsentStrategy.cs) · type `consent` · `authRequired = true`

Records a guardian's consent for a minor's site.

1. If a `GuardianConsent` already exists for the site → `consent.not_needed`.
2. Create a `GuardianConsent` (WHO = actor, HOW = `email`, WHAT = terms version).
3. Set the profile's `ConsentStateId = consented` — this lifts the publish gate.

**Does not** create a guardian `SiteLogin` — consenting is not the same as joining.

## Strategy: Invitation

> [`InvitationStrategy.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ActionTokens/Strategies/InvitationStrategy.cs) · type `invitation` · `authRequired = true`

Grants the invited role by creating an active `SiteLogin`.

1. Load the target site.
2. Validate the role against the site type (throws on mismatch).
3. `SiteLogin.CreateActiveSiteLogin(actor.Id, site.Id, payload.RoleId)`.

## Strategy: Org Email Verification

> [`OrgEmailVerificationStrategy.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ActionTokens/Strategies/OrgEmailVerificationStrategy.cs) · type `org_email_verification` · `authRequired = false`

Marks an organization as verified.

1. Load the `OrganizationProfile` for the token's target site; missing → `organization.profile_not_found`.
2. Set `VerificationStatusId = verified` and stamp an `OrgVerification` owned value.

## Gotchas — important security gaps

- **No accept-time email match.** Neither `ConsentStrategy` nor `InvitationStrategy` compares the accepting user's email to the token payload's email. **Whoever is authenticated and holds the link** gets the role / is recorded as the consenting guardian. The error code `invitation.email_mismatch` exists for exactly this check and is **never used**.
- **No duplicate-login guard** in `InvitationStrategy` — accepting two invitations for the same person+site creates two `SiteLogin` rows.
- **`OrgEmailVerificationStrategy.authRequired = false` is unreachable** in production because the controller isn't `[AllowAnonymous]` (see [Accept Action Token](./accept-action-token.md#gotchas)). As a result `VerifiedByUserId` is always null, and the audit's `VerifiedByEmail` records the *requester's* email, not the official's (see [Send Official Email Verification](./send-official-email-verification.md#gotchas)).
- All three strategy classes are declared in the **global namespace** (no `namespace` statement).
- The JSON payload discriminators (`invite`/`consent`/`orgEmailVerification`) differ from the `ActionTokenType` ids (`invitation`/`consent`/`org_email_verification`) — two vocabularies for the same three concepts.

## Related

- [Accept action token](./accept-action-token.md) · [Invite to profile](./invite-to-profile.md) · [Send official email verification](./send-official-email-verification.md)
