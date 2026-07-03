# 03 · Accounts & Permissions

**Depends on:** `00-overview.md`, `02-data-model.md`. **Pairs with:** `04-tiers-and-features.md`.

Covers: the identity model, guardian/parental control, organization multi-user roles, the change-request approval workflow, and the published-data privacy boundary.

---

## 1. Identity model — one profile, multiple linked roles

A **Site** represents one athlete or one organization. A site has one or more **linked logins** (`SiteLogin` in `02`), each carrying a **role**. The role vocabulary splits by site type:

**Individual sites (`SiteType.Individual`):**

| Role (`SiteLogin.SiteRoleId`) | Who | Default capability |
|------|-----|--------------------|
| `owner` (`IndividualRole.Owner`) | the athlete | Owns the profile; capability scales with effective tier + perks. For adults, the final approver. |
| `guardian` (`IndividualRole.Guardian`) | parent / legal guardian | Linked to a **minor** profile; permissions configurable; for minors, the final approver. |

**Organization sites (`SiteType.Organization`):**

| Role (`SiteLogin.SiteRoleId`) | Who | Default capability |
|------|-----|--------------------|
| `club_admin` (`OrganizationRole.ClubAdmin`) | club administrator | Full club control: billing, users, settings, content |
| `club_editor` (`OrganizationRole.ClubEditor`) | club content editor | Edits club page and manages featured athletes; cannot touch billing or users |

Why this shape (vs "1 or 2 fixed identities"): it scales cleanly to a second guardian, a temporary delegate, or future roles, and it keeps **legal account-holder** logic in one place. A minor profile must always have at least one `guardian` login (see registration paths below).

### Two registration paths

"Who authenticates" and "who the profile is for" are not always the same person. Two commands, sharing a private profile-creation core, cover both:

| | **Self-registration** (`RegisterIndividualSiteSelfCommand`) | **Guardian-creates-child** (`RegisterIndividualSiteGuardianCommand`) |
|---|---|---|
| Caller | the athlete (adult, or older minor with their own login) | the parent/guardian |
| Caller's login becomes | `IndividualRole.Owner` | `IndividualRole.Guardian` |
| `owner` login | the caller | **none in v1** (no child login yet — deferred) |
| `guardian` login | consent requested via `ActionToken` **if** the caller is a minor | **is the caller** (active by construction) |
| Idempotency | one owned profile per caller | a guardian may register **multiple** children |

- **Self + minor (< self-consent age 16):** the command requires `GuardianEmail`; the profile is written and a `ConsentActionToken` is issued and emailed in the same transaction. Missing guardian email → reject, nothing written.
- **Guardian-creates-child:** v1 is for minors only — registering an adult is rejected (`guardian.cannot_register_adult`); an adult must self-register. A child's own `owner` login is added later (when old enough) via a future invite flow.

### Pending vs. active guardian, and the publish gate

A minor profile can be **created** the moment a guardian email is provided, but until a real, active guardian has accepted consent, the profile's `ConsentState` is `PendingGuardianConsent` and **cannot be published**. No minor is ever publicly visible without an accountable adult having acted. The guardian-creates-child path skips the consent gate (the caller is already an active guardian — consent is implied by creation).

**Accepting the invite (ActionToken flow).** The pending invite is held by an `ActionToken(Invitation)`, not a ghost user row — we never pre-create Users for invitations. When the invited guardian follows the emailed link (authenticating via Auth0 if needed), a single `POST /api/action-tokens/{tokenId}/accept` call validates the token and creates an Active `SiteLogin` for the authenticated user, in one step. There is no separate explicit claim step; authentication via the link IS the redemption.

### Consent flow

When a minor self-registers and provides a guardian email, a `ConsentActionToken` is issued and emailed. The guardian follows the link, authenticates through Auth0 (carrying `returnUrl` so login/register navigation preserves the destination), and POSTs to `/api/action-tokens/{id}/accept`. This records an immutable `GuardianConsent` row (WHO, HOW, WHAT, WHEN per GDPR Art. 8) and lifts the publish gate on the minor's profile.

Authentication-via-the-link IS the redemption — there is no separate "I agree" button after login; the single accept POST is the consent act.

The guardian must be authenticated for consent to be recorded (`authRequired = true` on `ConsentStrategy`). Email-click alone is NOT the authority — the authenticated guardian identity is what makes the consent GDPR-compliant evidence.

`returnUrl` must be validated to be a local path before use (open-redirect protection).

### Permission model — `ControlMode` + `PermissionResolver`

What a login may do is **computed**, not stored per-action and not expressed as Specifications. The domain `PermissionResolver.Resolve(SiteLogin, IndividualProfile)` returns a `SitePermissions` preset from the profile's **`ControlMode`** and the login's role:

```csharp
record SitePermissions(
  bool CanEditContent, bool CanPublish, bool CanApproveChanges,
  bool CanManageMedia, bool CanManageMemberships);
// presets: None · ReadOnly · EditOnly (shared) · FullControl
```

- `ControlMode` (`athlete_controlled` | `guardian_controlled` | `*_shared`) decides **who is the controller** → `FullControl`; the other party gets `ReadOnly`, or `EditOnly` if a `*_shared` (collaboration) mode is on.
- The controller toggles collaboration via the `collaboration` endpoint and hands over via `transfer-control` (athlete must be ≥13 to receive control).
- Optional per-login overrides can be stored in `SiteLogin.Permissions` (the `LoginPermissions` value object: `MinorCanEditDraft`, `MinorCanPublish`, `MinorCanApproveChanges`, `MinorCanManageMedia`, `MinorCanManageMemberships`) for families that want to widen a minor's autonomy.

Default for a minor (guardian-created → `guardian_controlled`): guardian holds full control incl. publish/approve; the young athlete (`owner` login) is read-only until control is shared or transferred. Families can loosen this as the athlete matures.

> The hard line is **18**. Under 18 → guardian-gated. 18+ → athlete self-approves. The build treats 18 as the boundary and does not implement regional variants now.

---

## 2. Organization multi-user roles

Clubs (and other orgs) have multiple staff logins via the shared `SiteLogin` table, with `OrganizationRole` values (`club_admin`, `club_editor`) in `SiteLogin.SiteRoleId`. Build **two roles now**; document the rest as reserved.

**Build now**

| Role | Can | Cannot |
|------|-----|--------|
| `club_admin` (`OrganizationRole.ClubAdmin`) | manage billing & subscription; invite/remove users; control permissions; approve sponsorship/athlete slots; full club settings; everything ClubEditor can | — |
| `club_editor` (`OrganizationRole.ClubEditor`) | edit club page content; manage featured athletes; submit athlete change requests | touch billing/subscription; manage users |

**Reserved for later (document, do not build)**

| Role | Intended scope |
|------|----------------|
| `ClubViewer` | read-only internal access |
| `Coach` | submit athlete updates / results |
| `Photographer` | upload media, no other control |

Organization-side authorization is **not yet built** (no org-page editing endpoints exist). When it lands it should reuse the same chokepoint idea as the individual side (a resolver returning a capability set), combined with natural "caller holds an active org login" checks — not scattered role `if`s. (The earlier docs called for a Specification pattern; the implemented individual side uses `PermissionResolver` instead — see §1.)

---

## 3. Change-request & approval workflow

> **Status: not built.** §3b (club-proposed changes) describes the intended design. The `ChangeRequest` entity is a scaffold with no create/approve/reject commands and a shape that diverges from the design (`02` §5b). §3a self-editing is also gated on the editor write path, which is currently disabled (`02` §3).

Two distinct workflows depending on who edits what.

### 3a. Editing one's own athlete profile

Determined by age + guardian config:

| Profile is | Who can edit/propose | Who approves/publishes |
|------------|----------------------|------------------------|
| **Minor (<18)** | Owner (propose), Guardian (edit), Club (propose) | **Guardian approves everything** |
| **Adult (18+)** | Owner | **Athlete approves all changes**; club suggestions optional |

### 3b. Club-proposed changes to an affiliated athlete

A club (via `ClubAdmin`/`ClubEditor`) may **propose** edits to an affiliated athlete's profile. These are **requests**, never direct writes. Each proposal is a `ChangeRequest` row (defined in `02` §5b) that lives **outside both the athlete's draft and published config** until the profile side acts on it — a club can never reach either directly.

The proposal carries a **snapshot of the affected section(s)** in their proposed form (not a fragile field-level patch), so the approver sees exactly what they are accepting and the "before" is simply the current draft at review time.

```
Club (ClubEditor/Admin) proposes
        │
        ▼
ChangeRequest { TargetProfileId, ProposingOrganizationId, ProposedByUserId,
                ProposedSections (snapshot), Status = Pending }      ← separate table (02 §6)
        │
   reviewed by the profile side
        │
   ┌────┴───────────────┐
   ▼                     ▼
 target is MINOR       target is ADULT
 Guardian approves     Athlete approves
 /rejects/—            /rejects/—
 (Athlete may also     (Club suggestions
  propose; Guardian     optional; Athlete
  is sole approver)     is sole approver)
        │
   ┌────┴────────────┬──────────────┐
 Approve          Reject          Withdraw (by club, while Pending)
   │                │                │
   ▼                ▼                ▼
 GATE 1:          Status =        Status =
 ProposedSections Rejected        Withdrawn
 merged into the  (nothing        (nothing
 athlete's DRAFT  changes)        changes)
 Status=Approved
   │
   ▼
 GATE 2: normal draft → publish flow
 (athlete/guardian still publishes; approval does NOT publish)
```

**Two gates, explicitly.** Approval (gate 1) only moves the club's proposed sections *into the draft*. Going public still requires the athlete/guardian's separate **publish** (gate 2). So an approved club change sits in the draft alongside the athlete's own edits and is invisible until they choose to publish.

`ChangeRequest` fields are defined in `02` §5b; `ProposedByUserId` and `ResolvedByUserId` capture *which specific person* on each side acted (audit trail for a workflow touching minors' public presence). `Status` is `Pending` / `Approved` / `Rejected` / `Withdrawn`.

> Approval authority always resolves to the profile side, never the club. A club can request; it can never write to an athlete's profile directly. This protects B2C ownership.

---

## 4. The published public data contract (privacy boundary)

This boundary is load-bearing for the B2B hybrid. State it once, enforce everywhere.

- Organizations and the public consume **only** the athlete's **published** profile **public** fields.
- **Never** drafts. **Never** private fields. **Never** unpublished sections.
- If an athlete sets `VisibilityState = Private`, or unpublishes, any club showcase referencing them degrades to a **graceful placeholder** (e.g. "Profile not currently public") — it must not break the club page and must not leak the hidden data.
- Club `featuredAthletes` sections hold **references** (athlete ids), resolved at render against this contract — so an athlete editing/hiding their profile is reflected everywhere automatically (`01` §4, `02` §4).

A single server-side projection (e.g. `ToPublicContract(profile)`) should be the only path by which athlete data leaves the system to the public/club surface. Nothing else may serialize a profile outward.

---

## 5. Resource ownership rules (summary)

- An athlete login may only act on its own site (scoped by `SiteLogin` + `PermissionResolver`).
- A club login may only act on its own organization (scoped by its org `SiteLogin`) and may only **propose** to affiliated athletes.
- Server-managed entities (National Teams at launch) are writable only by **NextAtlet internal admins**.
- All of the above enforced via `PermissionResolver` + natural "caller holds an active login" checks in handlers, combined with the (planned) tier/perk capability checks from `04`.