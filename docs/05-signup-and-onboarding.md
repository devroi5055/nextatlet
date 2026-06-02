# 05 · Signup & Onboarding

**Depends on:** `03-accounts-and-permissions.md`, `04-tiers-and-features.md`.

Core principle: **signup is lightweight; media is never a blocker.** A young athlete's professional photos come from NextAtlet's own photographers and may not exist yet — so requiring media at signup is impossible by design. Media is an **onboarding/post-signup step**, addable later by the profile owner, a guardian, or a NextAtlet admin.

---

## 1. Athlete signup — minimal gate

A profile can be created and published (within tier) with **text only**. Required at signup:

| Field | Required | Why |
|-------|----------|-----|
| Email / login | yes | account |
| Display name | yes | profile + slug seed |
| Date of birth | yes | determines `IsMinor` → guardian gating (`03`) |
| Sport | yes (defaults `judo`) | profile context |
| Preferred locale (da/en) | yes | bilingual default |
| **If minor:** guardian email | yes | a `Guardian` login must exist before publish (`03`) |

That is the whole gate. Everything else — bio, results, photos, themes — is onboarding.

### Minor branch

If DOB < 18:
1. Collect guardian email.
2. Guardian receives an invite, creates/links a `Guardian` login (`03`).
3. Guardian permission defaults applied (guardian holds publish + approval).
4. Athlete may build/propose; guardian approves and publishes.

### Adult branch

If DOB ≥ 18: athlete is sole owner/approver; no guardian step.

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

Club onboarding:
1. Create club page (draft) using the club section types.
2. Invite staff (`ClubAdmin` invites `ClubEditor`s).
3. Affiliate athletes into slots — each athlete must already have a profile; affiliation creates a `Membership` (`02` §5) and grants the perk layer.
4. Publish club page (showcases affiliated athletes via published-contract references).

---

## 5. National Team & other org types

- **National Team** entities are **not** self-serve at launch — created and assigned by **NextAtlet internal admins** (`00`, `02`). No public signup path.
- `Academy` / `TrainingCenter` / `SchoolTeam` — model now; decide self-serve vs admin-created per type as a later business call (flagged in `07`).

---

## 6. What's deliberately not required

- No media at signup (ever a blocker) — the defining onboarding principle.
- No sponsor info at signup (sponsor features are post-MVP).
- No full bio/results to go live — a sparse page is valid and editable forever.
