# 06 · Feature Status & Known Problems

An honest status board. This replaces the older aspirational "features & problems" narrative.

## Feature status

| Area | Status | Notes |
|------|--------|-------|
| Auth0 authentication (bearer) | ✅ Working | Cookie scheme is vestigial |
| Just-in-time user provisioning | ✅ Working | `UserProvisioner`, matched by `sub` |
| Self / guardian registration | ✅ Working | Age-gated; creates the full starter bundle |
| Permission model | ✅ Working | `PermissionResolver` × `ControlMode` |
| ActionToken flow (invite/consent/verify) | ⚠️ Partial | Works, but no accept-time email match; org-verify anonymous branch unreachable |
| Guardian consent audit | ✅ Working | `GuardianConsent` written on accept |
| Control transfer | ❌ Broken | Passes profile id where a site id is needed; also `GetByIdAsync` `FindAsync` bug |
| Collaboration (shared editing) | ✅ Working | Toggles `_shared` modes |
| `GET /api/Me` decision gate | ✅ Working | O(all pending invites) query; 500 on org-owner edge case |
| `GET /api/sites` listing | ⚠️ Partial | No visibility enforcement (`?visibility=private` leaks) |
| Draft read (`config/draft`) | ⚠️ Partial | No authorization; `version` always 0 |
| Draft **write** / editor | ❌ Not built | Command deleted in refactor; `ISanitizationService`/`ISectionTypeRegistry` orphaned |
| Public render endpoint + renderer | ❌ Not built | The next real milestone |
| Publish flow + ISR/CDN | ❌ Not built | |
| Theme system | ⚠️ Minimal | One seeded "Classic" theme; no picker |
| Club registry (scrape) | ⚠️ Partial | Works but `[AllowAnonymous]`; deactivation source-key mismatch |
| Org email verification | ❌ Insecure | No ownership check (see below) |
| Billing / tiers / perks | ❌ Not built | Enumerations only; `PerkResolver`/`ResolveCapabilities` commented out |
| Media pipeline | ❌ Not built | `MediaAsset` schema-only |
| Memberships | ❌ Not built | `Membership` schema-only |
| Change-request workflow | ❌ Not built | `ChangeRequest` schema-only, no `StatusId` |
| Frontend marketing page | ✅ Working | Section-registry driven |
| Frontend onboarding wizard | ✅ Working | Only fully-working authed flow |
| Frontend dashboard/editor | ❌ Stub | Placeholder cards |

## Known problems (prioritised)

### Security (fix before production)

1. **`SendOfficialEmailVerificationCommand`** — no ownership check and returns the token id in the body → any authenticated user can verify an arbitrary organization.
2. **`GetDraftAthleteSiteSnapshotQuery`** — no authorization → any authenticated user reads any site's unpublished draft.
3. **`ClubsController`** — `scrape` / `add-sports` / `remove-sports` are all `[AllowAnonymous]` (unauthenticated crawl + DB writes).
4. **Invitation/Consent strategies** — never compare the accepting user's email to the token payload; the link-holder gets the role. `invitation.email_mismatch` exists but is unused.
5. **`GetSitesQuery`** — `visibility` is a client filter, not a server constraint; private sites are enumerable.
6. **CORS** `SetIsOriginAllowed(_ => true) + AllowCredentials()` (Development only, but dangerous if promoted).
7. Missing claims produce **500**, not 401.

### Bugs

- **`TransferControlCommand`** always fails: passes `ProfileId` to login checks that expect a site id; tests mirror the bug so they pass.
- **`IndividualProfileRepository`/`OrganizationProfileRepository.GetByIdAsync`** misuse `FindAsync(id, ct)` → EF throws at runtime.
- **Guardian-registered under-16s** stuck in `pending_guardian_consent` with no token to clear it.
- **`SiteSnapshotResponse.Version`** always serializes as 0.
- **`CountPendingInvitesByEmailAsync`** loads every pending invite in the DB on every `/api/Me`.
- Scraper `Source` mismatch (`dju_portalen` vs `dju_portal`) breaks deactivation.
- `SanitizationService` HTML-decodes *after* stripping tags, so `&lt;script&gt;` survives.

### Error contract

- Every business failure is **HTTP 400** regardless of category (403/404/409/422 are comments only).
- `ApiError.Parameters` is always empty.
- The frontend error catalog (`da.json`/`en.json`) contains **no** error-code translations, contradicting `ErrorCodes.cs`'s own comment.

### Dead / unwired code

`PerkResolver`, `ResolveCapabilitiesCommand`, `RequestsAndResponses.cs`, `PlanCapabilities.cs`, three `strings/*.cs` files — all commented out. `ActionTokenActor`, `ActionTokenAcceptedResponse`, `IndividualProfileResponse`, `CvrLookupResult`, `UpdateSiteSnapshotRequest` — unreferenced. `ISanitizationService`, `ISectionTypeRegistry`, `ICvrLookupService` — registered but unused. 12 of 33 error codes unreferenced. `ListClubOfficialsCommand` tested but has no route.

### Infra / hygiene

- CI (`.github/workflows/dotnet.yml`) pins .NET 8 against net10.0 — cannot pass. `infra/` empty.
- Frontend: `pnpm lint` broken (Next 16), `pnpm gen:api` would break the client, `tailwind.config.cjs` inert, `text-primary-gold` generates no CSS, `/onboarding/complete` and several dashboard nav links are dead.
- Numerous file-name ≠ type-name mismatches (see [`07-patterns-and-build-order.md`](07-patterns-and-build-order.md)).
