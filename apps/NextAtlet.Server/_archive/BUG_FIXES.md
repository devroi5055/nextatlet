# Step 1 Bug Fixes

## Critical Bugs Fixed

### 1. JsonElement vs Dictionary Deserialization (BLOCKING FOR STEP 2)
**Issue:** ASP.NET Core's `System.Text.Json` deserializes JSON objects to `JsonElement`, not `Dictionary<string, object>`. The validators tried to cast and access these directly, causing `InvalidCastException` at runtime when processing updated configs.

**Fix:** 
- Created `JsonElementConverter` utility class that recursively converts `JsonElement` to `Dictionary<string, object>`
- Updated `UpdateDraftConfigCommand` to normalize layout before validation using `JsonElementConverter.NormalizeLayout()`
- Updated section data normalization to use `JsonElementConverter.NormalizeSectionData()`
- Validators now receive clean `Dictionary` objects, not `JsonElement`
- Fallback logic in validators still works for safety

**Files Changed:**
- `NextAtlet.Infrastructure/Services/JsonElementConverter.cs` (new)
- `NextAtlet.Application/Features/Athletes/Commands/UpdateDraftConfigCommand.cs`

**Impact:** Fixes `InvalidCastException` that would occur on any draft config update request.

---

### 2. DateTime.UtcNow in Seed Data
**Issue:** Seed data used `DateTime.UtcNow` which evaluates at migration-generation time, causing new timestamps in migrations every time `dotnet ef migrations add` is run, polluting migration history.

**Fix:** Replaced with static deterministic datetime: `new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)`

**Files Changed:**
- `NextAtlet.Infrastructure/Data/NextAtletDbContext.cs` (Theme seed data)

**Impact:** Migrations are now clean and don't change when regenerated.

---

### 3. Redundant EF Core Update() Call
**Issue:** `UpdateDraftConfigCommand` called `_context.SiteConfigs.Update(siteConfig)` explicitly after mutating a tracked entity. EF Core change tracking detects these changes automatically when `SaveChangesAsync` is called.

**Fix:** Removed the redundant `Update()` call. EF Core handles it automatically.

**Files Changed:**
- `NextAtlet.Application/Features/Athletes/Commands/UpdateDraftConfigCommand.cs`

**Impact:** Code is clearer and less misleading to maintainers; no functional change.

---

## Important Issues Fixed

### 4. SanitizationService Mutates Input
**Issue:** `SanitizeLayout()` modified the input dictionary in place (`layout["sections"] = sanitizedSections`), an unexpected side-effect for callers.

**Fix:** Changed method to create and return a new dictionary instead of mutating the input.

**Files Changed:**
- `NextAtlet.Infrastructure/Services/SanitizationService.cs`

**Impact:** `SanitizeLayout()` is now a pure function with no side effects.

---

## Design Notes

### IsMinor Calculation (Not a Bug)
The calculation in `CreateAthleteCommand` is **correct**:
- Formula: `dateOfBirth.AddYears(18) > DateTime.UtcNow`
- Meaning: "Is birth date + 18 years in the future?" If yes, they're still a minor.
- This is logically equivalent to the property: `DateTime.UtcNow.AddYears(-18) < DateOfBirth`

Both are correct and calculate the same value. The comment in `CreateAthleteCommand` is now explicit about this.

---

## Testing the Fix

### Round-Trip Verification
The fixes enable the critical round-trip to work:
1. Write layout with hero + bio sections (anonymous objects)
2. Read from PostgreSQL jsonb (deserializes to JsonElement)
3. Normalize to Dictionary (JsonElementConverter)
4. Validate against schema (no InvalidCastException)
5. Sanitize (returns new dictionary)
6. Save back to DB

### Test Case
```
POST /api/athletes  — create athlete "maria-jensen" (dob 2007-06-15, minor)
GET /api/athletes/{id}/config/draft — read draft config
PUT /api/athletes/{id}/config/draft — update draft config with new hero/bio sections
  - Should succeed without InvalidCastException
  - Should pass validation
  - Should be sanitized
  - Should increment Version for optimistic concurrency
```

---

## Deferred (Not Fixed in This Pass)

- Authorization skeleton (acknowledged as Step 2+)
- Enum types for roles/states (design improvement, low priority)
- Reserved slugs to appsettings.json (design improvement, can move later)
- Guardian default permissions as constant (design improvement, low priority)
- ProfileLogin.Permissions type safety (design improvement, lower priority than the bugs)

---

## Build Status

✅ Builds successfully with no errors
✅ All critical bugs fixed
✅ Ready for Step 2

