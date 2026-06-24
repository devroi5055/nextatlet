# 05 · Signup & Onboarding

**Depends on:** `03-accounts-and-permissions.md`, `04-tiers-and-features.md`.

Core principle: **signup is lightweight; media is never a blocker.** A young athlete's professional photos come from NextAtlet's own photographers and may not exist yet — so requiring media at signup is impossible by design. Media is an **onboarding/post-signup step**, addable later by the profile owner, a guardian, or a NextAtlet admin.

---

## 1. Athlete signup — minimal gate

Signup is **two gates** (`03`): **Gate 1 — authentication** (Auth0 hosted login creates the credential; age-blind, guardian-free, not built by us), then **Gate 2 — profile registration** (the domain step below, behind `[Authorize]`). After login the frontend calls **`GET /api/me`**, which reports `{ Registered, Role }` so it can route a new user to the registration form and a returning one to their dashboard. Identity (email + IdP subject) always comes from the authenticated token, never the form body.

There are **two registration entry flows** (`03` §1) — the athlete sets up their own profile, or a parent sets one up for their child:

### Self-registration (`POST /api/individual-sites/self-register`, command: `RegisterIndividualSiteSelfCommand`)

A profile can be created and published (within tier) with **text only**. Required:

| Field | Required | Why |
|-------|----------|-----|
| Display name | yes | profile + slug seed |
| Date of birth | yes | determines `IsMinor` → guardian gating (`03`) |
| Sport | yes (defaults `judo`) | profile context |
| Preferred locale (da/en) | yes | bilingual default |
| **If minor (< 16):** guardian email | yes | a `ConsentActionToken` is issued and emailed; required before publish (`03`) |

(Email/login is not a form field — it's the authenticated caller.) That is the whole gate. Everything else — bio, results, photos, themes — is onboarding.

- **Minor (below self-consent age 16):** guardian email is required; the profile and a `ConsentActionToken` are created in the same transaction. The guardian authenticates and accepts at `POST /api/action-tokens/{tokenId}/accept`, which records consent and lifts the publish gate. The young athlete may build/propose but not publish until consent is recorded.
- **Adult (≥18):** athlete is sole owner/approver; no guardian step.

### Guardian-creates-profile-for-child (`POST /api/individual-sites/guardian-register`, command: `RegisterIndividualSiteGuardianCommand`)

The common youth-judo case: a parent sets up their child's profile. The **caller becomes the `guardian`** (active by construction); the child has **no login in v1**. Required: child display name, child DOB, locale (slug seeded from the name). Registering an adult this way is rejected — an adult must self-register. One guardian may register multiple children.

---

## 2. Onboarding (post-signup, non-blocking)

Presented as a checklist the athlete works through at their own pace; none of it blocks having a (sparse) live page.

1. **Bio & basics** — text.
2. **Results** — competition history (structured entries).
3. **Theme selection** — from the set the effective tier/perks allow.
4. **Media** — *optional, anytime.* Upload self-photos, or book a NextAtlet shoot, or wait for a club-funded shoot. Admin can also upload on the athlete's behalf.
5. **Sections** — gallery/sponsors/video as tier allows.
6. **Publish** — (guardian-gated for minors).

> Media uploaded later flows into `MediaAsset` with the right `Origin` (`02` §3) and can be referenced by any section. No re-onboarding needed.

---

## 3. Tier-specific signup differences

The **gate is the same** across athlete tiers — what differs is what onboarding unlocks and whether payment is collected.

| Tier | At signup | Onboarding unlocks |
|------|-----------|--------------------|
| **Free** | minimal gate only; no payment | core sections, 2–3 themes |
| **Plus** | minimal gate + payment setup | + gallery/sponsors, more themes, mentoring guides, photoshoot discount |
| **Pro** | minimal gate + payment setup | + video, all themes, 1:1 mentoring scheduling, included photoshoot booking |

Upgrade is always possible later without re-signup — a tier change just re-resolves effective capability (`04`).

---

## 4. Club signup (B2B)

| Field | Required | Why |
|-------|----------|-----|
| Org email / first ClubAdmin login | yes | account + first admin |
| Organization type | yes | `Club` at signup; other types may be admin-assigned |
| Club name | yes | page + slug |
| Locale | yes | bilingual default |
| Subscription choice | yes (Free default) | sets athlete slots + perk layer (`04`) |

Route: `POST /api/organization-sites/club-register`

Club onboarding:
1. Create club page (draft) using the club section types.
2. Invite staff (`ClubAdmin` invites `ClubEditor`s).
3. **Optional: verify the club** — `POST /api/organization-sites/send-official-email-verification` triggers the email-to-official flow: issues an `ActionToken(OrgEmailVerification)` and emails the accept link to the registry-sourced official address. Completion via `POST /api/action-tokens/{id}/accept` marks the club Verified. Verification gates **powers** (affiliating athletes) not existence (registering/building/publishing a club page is possible without verification — serves CVR-less clubs).
4. Affiliate athletes into slots — each athlete must already have a profile; affiliation creates a `Membership` (`02` §5) and grants the perk layer.
5. Publish club page (showcases affiliated athletes via published-contract references).

---

## 5. National Team & other org types

- **National Team** entities are **not** self-serve at launch — created and assigned by **NextAtlet internal admins** (`00`, `02`). No public signup path.
- `Academy` / `TrainingCenter` / `SchoolTeam` — model now; decide self-serve vs admin-created per type as a later business call (flagged in `07`).

---

## 6. What's deliberately not required

- No media at signup (ever a blocker) — the defining onboarding principle.
- No sponsor info at signup (sponsor features are post-MVP).
- No full bio/results to go live — a sparse page is valid and editable forever.
