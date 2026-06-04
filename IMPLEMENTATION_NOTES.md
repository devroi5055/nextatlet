# NextAtlet Implementation Notes — Step 1

> **Historical note.** The backend has since moved to **CQRS via MediatR + repository/Unit-of-Work** with an inverted dependency direction, and typed polymorphic section payloads. See `docs/08` (ADR), `docs/07`, and `REFACTOR_PLAN.md` for the current design.

## Auth Decision

**Chosen: External Managed IdP (Entra ID / Auth0)**

**Rationale:**
- NextAtlet serves **minors** (athletes under 18). Owning password hashes adds significant security/privacy liability.
- External IdP delegates credential management to a provider with purpose-built security (MFA, breach response, etc.).
- Simpler compliance story: the IdP manages password policies and reset flows, not us.
- `User.AuthProviderId` references the external subject; we do **not** store password hashes.

**Implementation:**
- `User` table has `AuthProviderId (varchar)` — points at the IdP's subject claim (e.g. `oid` for Entra, `sub` for Auth0).
- Token validation happens in middleware (verify JWT signature against IdP's public key).
- User lookup by `AuthProviderId` on each request.
- If first login: auto-create `User` row from token claims.

**Alternative (not chosen):** ASP.NET Core Identity would require us to custody passwords for minors — rejected on security/liability grounds.

---

## Step 1 Scope

### What is built:
- `User`, `AthleteProfile`, `ProfileLogin`, `SiteConfig`, `Theme`, `MediaAsset` tables
- EF Core code-first migrations
- SectionTypeRegistry pattern (hero + bio validators only)
- Validation + sanitization seam
- Three API endpoints: POST athletes, GET draft config, PUT draft config
- One hardcoded theme (seeded)
- Guardian linking for minors (Pending/Active status)

### What is NOT built (explicitly deferred):
- `Organization`, `OrganizationLogin`, `Membership` — not even stubs
- Publishing / Published state
- Tiers, plans, subscriptions
- Media upload pipeline
- Change requests / approval workflow
- Billing / Stripe
- Locale rendering (shape only, no fallback/switching)

---

## Key Implementation Decisions

| Decision | Why |
|----------|-----|
| `IsMinor` computed at request time from `DateOfBirth` | Avoids stale boolean; rules depend on current age |
| `SelfTier` not used yet | Tiers are Step 6; column exists for future but no logic yet |
| `ProfileLogin.Status = Pending` for invited guardians | Minor can be created with pending guardian; guardian joins later |
| Layout stored as jsonb with Strategy registry | Scales to new section types without schema changes |
| Section validators in code, not DB | Easier to test, evolve, and reason about |
| One hardcoded theme (seeded) | Prove the engine before theme picker (Step 5) |
| No permission nuance in Step 1 | Athlete owns profile; guardian can edit (if linked). Full permission matrix is Step 3+. |

---

## File Structure

```
NextAtlet.Domain/
  Entities/
    User.cs
    AthleteProfile.cs
    ProfileLogin.cs
    SiteConfig.cs
    Theme.cs
    MediaAsset.cs
  Enums/
    ProfileLoginRole.cs
    ProfileLoginStatus.cs
    SiteConfigState.cs
    ... (others)

NextAtlet.Infrastructure/
  Data/
    NextAtletDbContext.cs
    Migrations/
      [timestamp]_InitialCreate.cs
  Services/
    SectionRegistry/
      ISectionValidator.cs
      SectionTypeRegistry.cs
      HeroSectionValidator.cs
      BioSectionValidator.cs
    SanitizationService.cs

NextAtlet.Application/
  Features/
    Athletes/
      CreateAthleteCommand.cs
      GetDraftConfigQuery.cs
      UpdateDraftConfigCommand.cs
    DTOs/
      CreateAthleteRequest.cs
      SiteConfigDto.cs
      ... (others)

NextAtlet.Api/
  Controllers/
    AthletesController.cs
  Program.cs
  appsettings.json
```

---

## Database Schema (Essentials)

```sql
-- Enums
CREATE TYPE profile_login_role AS ENUM ('AthleteOwner', 'Guardian');
CREATE TYPE profile_login_status AS ENUM ('Pending', 'Active', 'Revoked');
CREATE TYPE site_config_state AS ENUM ('Draft', 'Published');
CREATE TYPE visibility_state AS ENUM ('Public', 'Private');
-- ... (others)

-- Tables
CREATE TABLE "user" (
  id uuid PRIMARY KEY,
  email varchar NOT NULL UNIQUE,
  auth_provider_id varchar NOT NULL UNIQUE,
  created_utc timestamp NOT NULL,
  updated_utc timestamp NOT NULL
);

CREATE TABLE athlete_profile (
  id uuid PRIMARY KEY,
  slug varchar NOT NULL UNIQUE,
  display_name varchar NOT NULL,
  sport varchar NOT NULL DEFAULT 'judo',
  date_of_birth date NOT NULL,
  default_locale varchar NOT NULL DEFAULT 'da',
  visibility_state visibility_state NOT NULL DEFAULT 'Public',
  self_tier varchar, -- not used yet; null is fine
  created_utc timestamp NOT NULL,
  updated_utc timestamp NOT NULL
);

CREATE TABLE profile_login (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL REFERENCES "user"(id),
  athlete_profile_id uuid NOT NULL REFERENCES athlete_profile(id),
  role profile_login_role NOT NULL,
  permissions jsonb DEFAULT '{}',
  status profile_login_status NOT NULL DEFAULT 'Active',
  created_utc timestamp NOT NULL,
  updated_utc timestamp NOT NULL,
  UNIQUE(user_id, athlete_profile_id)
);

CREATE TABLE theme (
  id uuid PRIMARY KEY,
  name varchar NOT NULL,
  version int NOT NULL,
  minimum_capability jsonb DEFAULT '{}',
  manifest jsonb NOT NULL,
  preview_image_url varchar,
  is_active boolean NOT NULL,
  created_utc timestamp NOT NULL,
  updated_utc timestamp NOT NULL
);

CREATE TABLE site_config (
  id uuid PRIMARY KEY,
  athlete_profile_id uuid NOT NULL REFERENCES athlete_profile(id),
  state site_config_state NOT NULL DEFAULT 'Draft',
  theme_id uuid NOT NULL REFERENCES theme(id),
  theme_version int NOT NULL,
  layout jsonb NOT NULL DEFAULT '{"sections":[]}',
  global_settings jsonb DEFAULT '{"colors":{},"fonts":{}}',
  version int NOT NULL DEFAULT 1,
  published_utc timestamp,
  created_utc timestamp NOT NULL,
  updated_utc timestamp NOT NULL,
  UNIQUE(athlete_profile_id, state)
);

CREATE TABLE media_asset (
  id uuid PRIMARY KEY,
  athlete_profile_id uuid NOT NULL REFERENCES athlete_profile(id),
  type varchar NOT NULL,
  origin varchar NOT NULL DEFAULT 'SelfUpload',
  is_club_branding boolean NOT NULL DEFAULT false,
  storage_key varchar NOT NULL,
  width int,
  height int,
  alt_text varchar,
  created_utc timestamp NOT NULL,
  updated_utc timestamp NOT NULL
);

-- Indexes
CREATE INDEX idx_profile_login_user_id ON profile_login(user_id);
CREATE INDEX idx_profile_login_athlete_profile_id ON profile_login(athlete_profile_id);
CREATE UNIQUE INDEX idx_athlete_profile_slug ON athlete_profile(slug);
CREATE INDEX idx_site_config_athlete_profile_id_state ON site_config(athlete_profile_id, state);
CREATE INDEX idx_media_asset_athlete_profile_id ON media_asset(athlete_profile_id);
```

---

## Test Case: The Cousin

Create a test athlete:
- Email: `maria@example.dk`
- Name: `Maria Jensen`
- DOB: `2007-06-15` (minor at current date)
- Locale: `da`

Create a guardian:
- Email: `aunt@example.dk`
- Name: `Aunt Karoline`
- Link to Maria with `Status = Active` and default permissions

Then:
1. Load Maria's draft config (should have hero + bio stubs)
2. Edit sections with bilingual text
3. Save and reload — verify round-trip

---

## Next Steps After Step 1

- **Step 2:** Public render endpoint (read draft config → return render payload with theme manifest).
- **Step 3:** Publish flow (draft → published, invalidation on publish).
- **Step 4:** Section registry + `results`, `gallery` sections.
- **Step 5:** Theme manifest + multiple themes.
- **Step 6:** Tiers + Specifications gating.

