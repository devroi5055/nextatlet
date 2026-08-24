# 02 · Data Model

> Source of truth: [`NextAtletDbContextModelSnapshot.cs`](../apps/NextAtlet.Server/NextAtlet.Infrastructure/Migrations/NextAtletDbContextModelSnapshot.cs). Visual ER diagram: [`ERD.mmd`](../ERD.mmd) and [`docs/confluence/03-data-model-erd.md`](confluence/03-data-model-erd.md).

**15 `DbSet`s → 14 tables** (`OrgVerification` is an owned type folded into `OrganizationProfiles`).

## Mental model

A **Site** is the publishable unit. It's specialised by exactly one of **IndividualProfile** or **OrganizationProfile** (linked via a `SiteId` column). Content is versioned as **SiteSnapshot** rows (jsonb layout + theme); the Site points at a current draft + published snapshot. Access is granted by **SiteLogin** (User × Site × role). Emailed flows funnel through one **ActionToken** table. **Club/ClubOfficial** is a separate scraped registry. **Membership / ChangeRequest / MediaAsset** are schema-only.

## Tables

| Table | PK | Key columns |
|-------|----|-----|
| `Users` | Id | `Email` (unique), `AuthProviderId` (unique, nullable — Auth0 `sub`) |
| `Sites` | Id | `Slug` (unique), `DisplayName`, `SiteTypeId`, `VisibilityStateId`, `VerificationStatusId`, `DefaultLocaleId`(varchar 2), `CurrentDraftSnapshotId`, `CurrentPublishedSnapshotId` |
| `IndividualProfiles` | Id | `SiteId` (no FK/index), `SportId`, `DateOfBirth` (date), `ControlModeId`, `ConsentStateId`, `SelfTierId` (nullable, never written) |
| `OrganizationProfiles` | Id | `SiteId` (no FK/index), `OrganizationTypeId`, `OrganizationTierId`, `AthleteSlotCount`, `IsServerManaged`, `VerificationStatusId`, owned `Verification_*` |
| `SiteSnapshots` | Id | `SiteId` (indexed, no FK), `ThemeId` (FK), `Layout` (jsonb), `GlobalSettings` (jsonb), `PublishedUtc` |
| `Themes` | Id | `Name`, `Manifest` (jsonb), `PreviewImageUrl`, `RetiredUtc` |
| `SiteLogins` | Id | `UserId` (FK), `SiteId` (FK), `SiteRoleId`, `StatusId`, `Permissions` (jsonb, always null) |
| `ActionTokens` | Id (= secret) | `TypeId`, `TargetSiteId` (FK), `ExpiresUtc`, `AcceptedUtc`, `Payload` (jsonb) |
| `GuardianConsents` | Id | `SiteId` (FK), `GuardianUserId` (FK, Restrict), `MethodId`, `TermsVersion` |
| `MediaAssets` | Id | `AthleteSiteId` (FK→Sites, nullable), `OrganizationId` (no FK), `TypeId`, `OriginId`, `IsClubBranding`, `StorageKey` |
| `Memberships` | Id | `IndividualProfileId` (FK), `OrganizationId` (FK), `RoleId`, `statusId`, `OccupiesSlot` |
| `ChangeRequests` | Id | `TargetProfileId` (FK), `ProposingOrganizationId` (FK), `ProposedByUserId` (FK, Restrict), `ThemeId` (FK), `ProposedLayout` (jsonb), `IsActive` (no StatusId!) |
| `Clubs` | Id | `Source`+`SourceKey` (unique), `CountryId`, `Name`, `Address`, `SportIds` (text[]), `IsActive` |
| `ClubOfficials` | Id | `ClubId` (FK), `Name`, `Email`, `Phone`, `RoleId` |

## Relationships (delete behaviour)

Cascade: Users→SiteLogins, Sites→SiteLogins, Sites→ActionTokens, Sites→GuardianConsents, Sites→MediaAssets, IndividualProfiles→Memberships, OrganizationProfiles→Memberships, IndividualProfiles→ChangeRequests, OrganizationProfiles→ChangeRequests, Clubs→ClubOfficials.

Restrict: Users→GuardianConsents, Users→ChangeRequests, Themes→SiteSnapshots, Themes→ChangeRequests, Sites→SiteSnapshots (both current-draft and current-published pointers).

## ⚠️ Unenforced links (bare uuid, no FK)

`IndividualProfiles.SiteId`, `OrganizationProfiles.SiteId` (both also **no index**), `SiteSnapshots.SiteId` (indexed, no FK), `MediaAssets.OrganizationId`, `OrganizationProfiles.Verification_VerifiedByUserId`, and **every enumeration `*Id` column**. There are **no CHECK constraints** anywhere; the `MediaAsset` owner-XOR rule is documented but not enforced.

## JSONB storage

`SiteLayout`, `ThemeManifest`, `ActionTokenPayload`, `GlobalSettings`, `LoginPermissions` are stored via [`JsonbValueConversion`](../apps/NextAtlet.Server/NextAtlet.Infrastructure/Persistence/JsonbValueConversion.cs) as `jsonb`, but EF sees them as opaque `string` (camelCase, `JsonSerializerDefaults.Web`). **You cannot LINQ into them** — code that needs to filter materialises rows and filters in memory. `OrgVerification` is instead an EF owned type flattened into `OrganizationProfiles` columns.

## Smart-enumeration pattern

Enumerations (`ControlModes`, `Sport`, `ConsentStates`, …) use a base class with a string `Id` + bilingual `LocalizedText`. Entities store the raw string `Id` in a `varchar`. **No lookup table, no FK/CHECK** — a bad value is only caught if `FromId()` is called (which throws). Note `Enumeration.Equals` compares only `Id` with no type check, so `AthleteTier.Free == OrganizationTier.Free` is `true`.

## Seed

One `HasData` seed: the **"Classic" Theme** (`11111111-1111-1111-1111-111111111111`). Registration throws "Classic theme not found" if it's missing. Runtime dev data is seeded separately by `DevelopmentDataSeeder`.

## Known data-model issues

- The central `Site↔profile` link is unenforced and unindexed (`GetBySiteIdAsync` is a sequential scan).
- `GuardianConsents.SiteId` is not unique — multiple consents per site are possible.
- `SiteSnapshot` is a "created-only" entity but has a mutable `PublishedUtc`, so it isn't truly immutable.
- `ChangeRequests` has no `StatusId` column despite a `ChangeRequestStatus` enumeration existing.
- The seeded theme's `cards.radius = "medium"` isn't a legal radius value in `Strings.StyleValues`.
