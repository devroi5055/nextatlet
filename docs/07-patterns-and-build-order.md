# 07 · Patterns, Build Order & Open Questions

**Depends on:** all preceding documents. This is the implementation guidance and the running decision log.

---

## 1. Design patterns

| Pattern | Where | Why |
|---------|-------|-----|
| **Strategy** | section validation & rendering contract | one strategy per section type; add a type → add a strategy, no `switch` sprawl |
| **Factory / Registry** | `SectionTypeRegistry` | maps `"hero"` → schema + validator + (frontend) component contract; same idea for club section types |
| **Specification** | tier/perk gating, ownership, role checks | composable, declarative rules (`CanEditField`, `CanUseTheme`, `CanManageBilling`, `CanProposeToAthlete`) instead of scattered `if`s |
| **CQRS-lite** | editor (write) vs public (read) paths | separate, sanitized, cacheable public read model from the editable model (`01` §3) |
| **Builder** | public render payload | compose config + theme manifest + resolved media into one self-contained response |
| **Decorator / Middleware** | caching + sanitization on public path | wrap public render without touching core logic |
| **DTO + mapping** | API boundaries | never expose EF entities; map to DTOs so internal schema can evolve |
| **Options pattern** | tier/perk definitions, theme config | `IOptions<…>` for testable, injectable config |
| **Dedicated service: `PerkResolver`** | effective-capability resolution | single place that computes `max(SelfTier, ActiveClubPerks)` per feature (`02` §6, `04`) — never persist conflated state |
| **Dedicated service: `PublicContractProjector`** | the only path athlete data leaves to public/club | `ToPublicContract(profile)` — enforces the privacy boundary in one place (`03` §4) |

> Resist over-engineering. No event sourcing, no microservice-per-concept, no full DDD aggregates at this stage. **Strategy (sections) + Specification (gating) + CQRS-lite (read/write split) + two focused services (PerkResolver, PublicContractProjector)** give ~90% of the flexibility at a fraction of the cost.

---

## 2. Things to keep stable (don't churn these)

These are load-bearing; changing them ripples everywhere:

1. **Config-as-data** (sections + theme manifest; backend emits no HTML). (`01`, `02`)
2. **Additive perk layer, never replace.** (`04`)
3. **Published public data contract as the only outbound athlete projection.** (`03`)
4. **One profile + linked roles; 18 as the guardian boundary.** (`03`)
5. **Generic time-bounded memberships with derived primaries; history retained.** (`02`)

Everything else (tier numbers, theme counts, exact section types, storage vendor) is negotiable.

---

## 3. Incremental build order

Each step is shippable and additive — later steps don't rewrite earlier ones.

1. **Profiles + SiteConfig + auth (multi-login).** One hardcoded theme, two section types (`hero`, `bio`). Draft only. Guardian linking for minors.
2. **Public render endpoint + minimal Next.js renderer.** Prove data→render end-to-end with the cousin's profile as the first real case. *(Critical milestone — everything after is addition.)*
3. **Publish flow** (draft → published) + ISR/CDN caching + invalidation on publish.
4. **Section registry + Strategy validators;** add `results`, `gallery`.
5. **Theme manifest system + theme picker;** add a 2nd and 3rd theme.
6. **Athlete self-tiers + Specifications;** wire tier → editable scope + theme access.
7. **Media pipeline** (upload → blob/CDN → thumbnails); `gallery`/`video` sections; admin-on-behalf uploads.
8. **Organizations + multi-user roles** (ClubAdmin/ClubEditor); club page engine; club signup.
9. **Memberships + derived primaries;** affiliate athletes into slots; history retention.
10. **PerkResolver + additive perk layer;** club subscriptions; effective-capability resolution.
11. **Club showcases** via published-contract references; placeholder on private/unpublished; dependency revalidation.
12. **Change-request / approval workflow** (club proposes → guardian/athlete approves).
13. **Mentoring content + 1:1 scheduling;** photoshoot booking.
14. **Versioning/history** for rollback (higher tiers).
15. **Bilingual polish; custom subdomains** (DNS/cert complexity — last).

---

## 4. Open questions / decisions to resolve before the relevant step

| # | Question | Blocks step | Leaning |
|---|----------|-------------|---------|
| 1 | One-row-per-state vs `SiteConfigHistory` table for rollback? | 3 / 14 | history table if rollback matters to higher tiers |
| 2 | Final tier prices & exact slot/session counts? | 6 / 10 | placeholders in `04` until business decides |
| 3 | Club downgrade below current roster — block, or mark overflow inactive? | 10 | block downgrade until under limit (simpler, fairer) |
| 4 | Self-serve vs admin-created for Academy / TrainingCenter / SchoolTeam? | 8 | admin-created first; revisit per type |
| 5 | Bilingual: per-field locale maps vs section variants? | 4 | per-field maps for short text (`02` §7) |
| 6 | How "custom" can Pro custom sections get before a dev is needed? | 5 / 6 | hard boundary; **no** free-form HTML (XSS/quality) |
| 7 | Second-guardian / delegate flows beyond the single guardian default? | 1 | model supports it; build when needed |
| 8 | Data ownership/retention if an athlete deletes their profile while club-affiliated? | 8 / 11 | athlete owns; club showcase degrades to placeholder; ToS question |
| 9 | Photoshoot scheduling/logistics system (photographer calendars, locations)? | 13 | out of MVP core; revisit |

---

## 5. Document maintenance

When a decision in §4 is made, update the relevant numbered document **and** strike the row here, noting the date/decision. Keep this file as the single index of "what's still undecided."
