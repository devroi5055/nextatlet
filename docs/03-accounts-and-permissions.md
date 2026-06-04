# 03 · Accounts & Permissions

**Depends on:** `00-overview.md`, `02-data-model.md`. **Pairs with:** `04-tiers-and-features.md`.

Covers: the identity model, guardian/parental control, organization multi-user roles, the change-request approval workflow, and the published-data privacy boundary.

---

## 1. Identity model — one profile, multiple linked roles

A **profile** represents one athlete. A profile has one or more **linked logins** (`ProfileLogin` in `02`), each carrying a **role**:

| Role | Who | Default capability |
|------|-----|--------------------|
| `AthleteOwner` | the athlete | Owns the profile; capability scales with effective tier + perks. For adults, the final approver. |
| `Guardian` | parent / legal guardian | Linked to a **minor** profile; permissions configurable; for minors, the final approver. |

Why this shape (vs "1 or 2 fixed identities"): it scales cleanly to a second guardian, a temporary delegate, or future roles, and it keeps **legal account-holder** logic in one place. A minor profile must always have at least one `Guardian` login (see registration paths below).

### Two registration paths

"Who authenticates" and "who the profile is for" are not always the same person. Two commands, sharing a private profile-creation core, cover both:

| | **Self-registration** (`RegisterOwnAthleteCommand`) | **Guardian-creates-child** (`RegisterChildAthleteCommand`) |
|---|---|---|
| Caller | the athlete (adult, or older minor with their own login) | the parent/guardian |
| Caller's login becomes | `AthleteOwner` | `Guardian` |
| `AthleteOwner` login | the caller | **none in v1** (no child login yet — deferred) |
| `Guardian` login | invited **if** the caller is a minor | **is the caller** (active by construction) |
| Idempotency | one owned profile per caller | a guardian may register **multiple** children |

- **Self + minor:** the command requires `GuardianEmail`; the profile and a **`Pending`** guardian login are written in one transaction. Missing guardian email → reject, nothing written.
- **Guardian-creates-child:** v1 is for minors only — registering an adult is rejected (`guardian.cannot_register_adult`); an adult must self-register. A child's own `AthleteOwner` login is added later (when old enough) via a future `InviteAthleteOwnerLoginCommand`, reusing the pending-invite/claim machinery.

### Pending vs. active guardian, and the publish gate

A minor profile can be **created** the moment a guardian is named, but a `Pending` (invited, not-yet-accepted) guardian **cannot publish**, and the minor `AthleteOwner` does not hold publish rights by default. So a minor profile stays a private draft until a **real, active** guardian (holding `canPublish`) has accepted and published it — no minor is ever publicly visible without an accountable adult having acted. The guardian-creates-child path skips the pending state (the caller is already an active guardian).

**Accepting the invite (implemented).** When the invited guardian first authenticates (a new IdP `sub`, same email), the system **links** that subject to the unclaimed user row by matching the verified email (identity reconciliation only — it grants nothing), and `GET /api/me` surfaces the **pending guardian invite** so the frontend can prompt them. The guardian then **explicitly accepts** via `POST /api/guardianships/accept` (`AcceptGuardianInviteCommand`): this claims the account and flips their `Pending` guardian login(s) → `Active`. Acceptance is deliberate and auditable — never a silent side-effect of logging in. (An email-delivered invite link is a later, additive enhancement; the invite today is the guardian email captured at registration.)

### Guardian permission configuration

`ProfileLogin.Permissions` (jsonb) configures what a guardian may do, so families can choose how much autonomy the young athlete has:

```jsonc
{
  "canEditContent": true,
  "canPublish": true,        // typically guardian-only for minors
  "canApproveChanges": true, // approval authority (see §3)
  "canManageMedia": true,
  "canManageMemberships": true
}
```

Recommended defaults for a minor: guardian holds `canPublish` and `canApproveChanges`; the young athlete (`AthleteOwner`) may edit/propose but not publish. Families can loosen this as the athlete matures.

> The hard line is **18**. Under 18 → guardian-gated. 18+ → athlete self-approves. The build treats 18 as the boundary and does not implement regional variants now.

---

## 2. Organization multi-user roles

Clubs (and other orgs) have multiple staff logins (`OrganizationLogin` in `02`). Build **two roles now**; document the rest as reserved.

**Build now**

| Role | Can | Cannot |
|------|-----|--------|
| `ClubAdmin` | manage billing & subscription; invite/remove users; control permissions; approve sponsorship/athlete slots; full club settings; everything ClubEditor can | — |
| `ClubEditor` | edit club page content; manage featured athletes; submit athlete change requests | touch billing/subscription; manage users |

**Reserved for later (document, do not build)**

| Role | Intended scope |
|------|----------------|
| `ClubViewer` | read-only internal access |
| `Coach` | submit athlete updates / results |
| `Photographer` | upload media, no other control |

Authorization should be expressed as composable **Specifications** (`07`) — e.g. `CanManageBilling`, `CanEditClubPage` — not scattered role checks.

---

## 3. Change-request & approval workflow

Two distinct workflows depending on who edits what.

### 3a. Editing one's own athlete profile

Determined by age + guardian config:

| Profile is | Who can edit/propose | Who approves/publishes |
|------------|----------------------|------------------------|
| **Minor (<18)** | AthleteOwner (propose), Guardian (edit), Club (propose) | **Guardian approves everything** |
| **Adult (18+)** | AthleteOwner | **Athlete approves all changes**; club suggestions optional |

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

- An athlete login may only act on its own profile (scoped by `ProfileLogin`).
- A club login may only act on its own organization (scoped by `OrganizationLogin`) and may only **propose** to affiliated athletes.
- Server-managed entities (National Teams at launch) are writable only by **NextAtlet internal admins**.
- All of the above expressed as Specifications, combined with tier/perk capability checks from `04`.