# 06 · Features & Problems They Solve

**Audience:** developers — this is the *why* behind each feature, so implementation decisions stay anchored to the actual problem. Not marketing copy.

> This is a problem/rationale doc covering the full product vision; many features below (photography, mentoring, club showcases, perk layer, sponsor reach) are **designed but not yet built** — see the status note in `CLAUDE.md` and the per-doc status banners for what exists today.

---

## 1. Why NextAtlet exists at all

Young athletes are digitally underserved. They have results and potential but no credible, discoverable online presence, and no realistic path to sponsorship. Generic site builders (Wix, Instagram) don't solve this: they're undiscoverable, inconsistent, unprofessional for a sponsor's eyes, and isolated from the athlete's competitive context. NextAtlet bundles **presence + professional media + mentoring + affiliation**, hosted coherently under one searchable domain.

---

## 2. Feature → problem → why this design

### Auto-generated athlete websites
- **Problem:** athletes can't build a credible site; ad-hoc sites are unfindable and inconsistent.
- **Solution:** config-as-data + themes; one hosted, SEO-coherent domain.
- **Why this way:** storing configuration (not HTML) means one engine serves every tier, themes scale without migrations, and everything is searchable in one place. (`01`, `02`)

### Free athlete tier
- **Problem:** youth athletes (really their parents) have little money; a paywall at the door kills adoption.
- **Solution:** a genuinely useful free page.
- **Why:** adoption first; the free page is the funnel into paid tiers, club affiliation, and photography. (`04`)

### Professional photography as a bookable service
- **Problem:** amateur photos undermine credibility with sponsors; athletes can't self-produce pro media.
- **Solution:** internal photographers, studios/competitions; media addable to the profile anytime.
- **Why bookable (not always-on):** photography is the strongest differentiator but the hardest to scale; modeling it as a one-time/booked service keeps quality controlled and lets tiers/clubs discount or fund it. (`04`, `05`)

### Media never blocks signup
- **Problem:** the pro photos don't exist yet when a youth athlete joins.
- **Solution:** lightweight text-only signup; media is post-signup onboarding, addable by owner/guardian/admin.
- **Why:** any media gate would make youth signup literally impossible. (`05`)

### One profile + linked roles (guardian model)
- **Problem:** most athletes are minors; legal account-holder, consent, and parental control must be explicit.
- **Solution:** one profile, multiple linked logins with roles; guardian permissions configurable.
- **Why:** keeps legal/consent logic in one place and scales to second guardians or delegates without redesign. (`03`)

### Approval workflow (minor vs adult)
- **Problem:** who is allowed to change/publish a minor's public presence?
- **Solution:** minors → guardian approves everything; adults → self-approve; clubs may only *propose*.
- **Why:** child-safety and trust; clubs never write directly to an athlete's profile. (`03`)

### Organizations (clubs et al.) — the B2B hybrid
- **Problem:** athletes are hard to reach one by one; clubs already aggregate them; clubs want showcase value.
- **Solution:** clubs register, manage their own page, affiliate athletes via slots.
- **Why:** clubs are a distribution channel and add athlete value (funded shoots, exposure) without taking ownership. (`00`, `04`)

### Generic memberships (Club / NationalTeam / Academy / TrainingCenter / SchoolTeam)
- **Problem:** an athlete's affiliations are plural and change over time; club-only modeling gets messy fast.
- **Solution:** time-bounded many-to-many memberships with derived display/prestige/training primaries.
- **Why:** clean, realistic, and supports "leaves active roster but history retained" + moving between clubs. (`02`)

### Additive perk layer (never replaces tier)
- **Problem:** if a club slot replaced an athlete's paid tier, clubs become a way to dodge individual payment → athlete revenue collapses, billing/trust break.
- **Solution:** perks resolved at request time as a per-feature max on top of `SelfTier`.
- **Why:** protects B2C revenue, avoids billing confusion, makes "join/leave club" behavior predictable. (`04`)

### Live-reference club showcases (published contract only)
- **Problem:** duplicating athlete data onto club pages goes stale and risks leaking private/draft data.
- **Solution:** club pages reference athletes and resolve against the published public contract at render.
- **Why:** single source of truth + a hard privacy boundary; athletes editing their profile update everywhere. (`01`, `03`)

### Affiliation history retained
- **Problem:** clubs and sponsors gain value from knowing an athlete's lineage; deleting links destroys it.
- **Solution:** ending a membership marks it inactive but keeps the row.
- **Why:** transparency + club value (current roster *and* alumni), without trapping the athlete. (`02`)

### National Team as server-managed prestige
- **Problem:** prestige affiliations can't be self-claimed without losing credibility.
- **Solution:** NT entities created/assigned only by internal admins; surfaced as a badge.
- **Why:** keeps prestige trustworthy; clean upgrade path to federation self-service later. (`00`, `02`)

### Mentoring (guides + 1:1)
- **Problem:** young athletes lack guidance on presence, sponsorship, and career steps.
- **Solution:** tiered mentoring content and 1:1 sessions.
- **Why:** deepens the service moat beyond "a website" and justifies recurring subscription. (`04`)

---

## 3. Problems intentionally deferred

| Deferred | Why it's safe to wait |
|----------|------------------------|
| Sponsor marketplace | needs reach/results data and aggregation to be valuable; model leaves room (`01`) |
| Federation self-service | internal-admin NT entities cover MVP; clean upgrade path (`02`) |
| Free-form custom HTML sections | reopens XSS + quality problems the engine exists to avoid (`07`) |
| Multi-sport beyond judo | `Sport` field generalizes; focus wins the first niche (`00`) |
