# 04 · Tiers, Subscriptions & the Perk Layer

**Depends on:** `00-overview.md`, `02-data-model.md`, `03-accounts-and-permissions.md`.

> **Values below are a starting proposal, not final pricing.** Numbers (slot counts, session counts, prices) are marked as placeholders `[…]` where they need a real business decision. The *structure* — additive perks, never-replace — is the part to keep stable.

> **Implementation status:** none of this is built yet. There are `AthleteTier` (`free`/`plus`/`pro`) and `OrganizationTier` (`free`/`plus`/`pro`) **enumerations**, and denormalized `IndividualProfile.SelfTierId` / `OrganizationProfile.OrganizationTierId` fields — but no billing tables, no `Subscription`, no Stripe, no `PerkResolver` (stub only), and no tier-based gating. Tiers don't yet change anything at runtime. The perk layer, slots, and `EffectiveCapability` below describe the intended design.

---

## 1. The one rule that governs everything

**Club perks are additive and scoped. They never replace, downgrade, or substitute for an athlete's own tier.**

If they did, clubs would become a way to bypass paying individually — which kills athlete revenue, creates billing confusion, and breaks trust. So:

```
EffectiveCapability(feature) = max( SelfTier(feature), ActiveClubPerks(feature) )
```

- An athlete always has a `SelfTier` (Free or paid).
- An active Club membership *adds* capability on top, per feature.
- Leaving the club removes only the added layer; `SelfTier` is untouched.
- Photos captured of the athlete **stay with the athlete** (capture funded ≠ identity owned); only `IsClubBranding` assets are club-retained (`02` §3).

---

## 2. Athlete self-tiers (B2C)

Three tiers proposed. The auto-generation engine is identical across all; tier only gates **which fields/sections are editable** and **which themes are selectable** — enforced server-side.

| Capability | **Free** | **Plus** *(paid)* | **Pro** *(paid)* |
|------------|----------|-------------------|------------------|
| Public profile page | ✓ simple | ✓ | ✓ |
| Editable sections | core only (hero, bio, basic results) | + gallery, sponsors, extended results | + custom section ordering, video |
| Themes | 2–3 fixed | 6–8 | all + advanced color/layout control |
| Bilingual (da/en) | ✓ | ✓ | ✓ |
| Subdomain / custom URL | `nextatlet.dk/slug` | optional subdomain | custom subdomain |
| Mentoring content | — | guides & documents | guides + **1:1 mentoring** `[N sessions]` |
| Photography | — | discount on sessions `[%]` | included session `[count/yr]` |
| Analytics on own profile | basic views | + referrers, engagement | full |
| Price | **0** | `[kr/mo or kr/yr]` | `[kr/mo or kr/yr]` |

**Pricing model proposal:** subscriptions for the recurring capabilities (page, themes, analytics, mentoring access) **plus one-time payments** for discrete services (a photoshoot booking, a video edit). One-time and subscription coexist; a photoshoot is a purchasable add-on at any tier, just cheaper/included higher up.

> Photography is the strongest differentiator but the hardest to scale (one no-show damages trust). Keep it tightly quality-controlled; model it as a **bookable service** (one-time) that tiers discount or include, not as an always-on entitlement.

---

## 3. Club subscriptions (B2B)

Clubs register and pick a subscription that grants **athlete slots** + club-level tools. A club starts free.

| Capability | **Club Free** | **Club Plus** *(paid)* | **Club Pro** *(paid)* |
|------------|---------------|------------------------|------------------------|
| Club showcase page | ✓ | ✓ | ✓ |
| Athlete slots | `[10]` | `[40]` | `[100]` / custom |
| Multi-user staff | ClubAdmin + ClubEditor | same | same |
| Perks granted to slotted athletes | basic profile boost | enhanced customization + analytics | full + funded photoshoots `[count]` |
| Club analytics | basic | + roster-level | + recruitment dashboards |
| Event tracking | — | ✓ | ✓ |
| Recruitment tools | — | basic | advanced |
| Content-creation tools | — | basic | advanced |
| Price | **0** | `[kr/mo]` | `[kr/mo]` |

### Athlete slots — how they work

- A slot is the right to **affiliate one athlete** and grant them the club's perk layer.
- The athlete **must have their own profile first**; affiliation links it via a `Membership` with `OccupiesSlot = true` (`02` §5).
- While slotted + active, the athlete receives the club's perk layer **on top of** their own tier.
- Leave the club (or the membership goes inactive) → slot frees up, perk layer drops, athlete reverts to `SelfTier`. History is retained (`02` §5).
- This yields the intended behavior: an athlete can move clubs and pick up the new club's perks; a club's value grows with both its current roster and its alumni history.

---

## 4. Perk layer contents (proposed)

What an active club subscription can add on top of an athlete's `SelfTier`:

| Perk | Effect while active | On membership end |
|------|---------------------|-------------------|
| Profile customization boost | unlocks extra themes/sections beyond SelfTier | reverts to SelfTier scope |
| Funded photoshoot | a session the club pays for | **photos stay with athlete**; entitlement to *future* shoots ends |
| Enhanced profile analytics | richer stats than SelfTier | reverts to SelfTier analytics |
| Club/national badges | display affiliation/prestige badges | badge removed when affiliation ends |
| Recruitment visibility | surfaced in club/recruiter tools | removed |

Perks are **resolved at request time** by the (planned) `PerkResolver` (`02` §6, `07`). They are never written into `IndividualProfile.SelfTierId`.

---

## 5. Edge cases to keep consistent

- **Athlete pays Pro AND gets a club slot:** effective capability is the per-feature max of the two. The club never lowers Pro. If the athlete leaves, they keep Pro.
- **Athlete on Free + club slot:** enjoys club perks while affiliated; reverts to Free on exit. No surprise charges — they were never billed for the club layer.
- **Two clubs over time:** only one **active Club** (display primary) applies perks at a time (`02` §5). No stacking of two clubs' perks.
- **National Team membership:** prestige/badge only — **not** a perk source. Server-managed (`00`, `02`).
- **Club downgrades its subscription below its current roster size:** affiliations exceeding the new `AthleteSlotCount` must be handled explicitly (e.g. block downgrade until under limit, or mark overflow memberships inactive with notice). Decide before billing ships — flagged in `07`.
