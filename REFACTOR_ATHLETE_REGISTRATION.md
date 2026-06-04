# Refactor Plan — Split Athlete Registration (Self vs Guardian-Creates-Profile-for-Child)

**Why:** the current `RegisterAthleteCommand` bakes in the assumption *caller = AthleteOwner*. For
youth judo, the common case is a **parent creating an athlete profile for a young child**, where the
caller becomes the Guardian login and the child is the conceptual AthleteOwner — possibly with **no
login of their own**. "Who authenticates" and "who the profile is for" are no longer the same person.

> **Terminology:** the guardian flow registers an **`AthleteProfile`** (and attaches the caller as a
> Guardian `ProfileLogin`). It does **not** create a "child" user/login — in v1 no child login exists
> at all (§4). "Child" in the command/endpoint names signals *intent* (a guardian setting up their
> kid's profile); the thing actually created is always an `AthleteProfile` + a Guardian login.

**Decision (locked):** two separate commands/endpoints sharing a private profile-creation helper.

**Recommendation on child login (the open question):** **guardian-only first; defer the optional
child login.** Rationale below in §4.

Touches: `02` (`User`/`AuthIdentity`, `ProfileLogin`, the "minor needs ≥1 Guardian" rule), `03`
(guardian model, roles), `05` (signup gate — two entry flows), `07` (build order, naming convention).

---

## 1. The two registration paths

| | **Self-registration** | **Guardian-creates-profile-for-child** |
|---|---|---|
| Who calls | the athlete (adult, or older minor with own login) | the parent/guardian |
| What's created | an `AthleteProfile` for the caller | an `AthleteProfile` for the child |
| Caller's `sub` becomes | **AthleteOwner** login | **Guardian** login |
| AthleteOwner login | the caller | **none in v1** (no child login exists) — deferred |
| Guardian login | invited separately *if* minor | **is the caller** (exists by construction) |
| Guardian-required rule | enforced via invite for minors | satisfied automatically |

The key insight: your model already supports "profile with only a Guardian login" because
`ProfileLogin` is decoupled from `AthleteProfile` (`03` "one profile + linked roles"). A profile can
have {AthleteOwner+Guardian}, {AthleteOwner only}, or {Guardian only}. No special-casing needed in
the schema — only in which logins each command attaches.

---

## 1a. THE CORE RULE — a minor profile may exist, but only with a guardian attached

> **A minor (DOB < 18 today) CAN have an `AthleteProfile`. It can never exist without a Guardian
> `ProfileLogin` attached to it. The two are created together, atomically, in the same transaction —
> there is no moment where a minor profile exists with zero guardian logins.**

This is the single rule everything else in this document serves. Stated as a table:

| Athlete age (from `DateOfBirth`) | Profile allowed? | Guardian required? | When the guardian login is created |
|---|---|---|---|
| **Minor (< 18)** | ✅ yes | ✅ **mandatory** | in the same transaction as the profile — never after |
| **Adult (≥ 18)** | ✅ yes | ❌ no (athlete self-owns) | n/a |

How each registration path satisfies it:

- **Self-registration of a minor** (athlete has their own login): the command **requires**
  `GuardianEmail`. The handler creates the profile **and** a Guardian `ProfileLogin` in one
  transaction. If `GuardianEmail` is missing → reject with `GuardianEmailRequired`, no profile is
  created. The guardian is created as a **pending** user (invited); they claim the login later.
- **Guardian-creates-profile-for-child**: the caller **is** the guardian, so a Guardian
  `ProfileLogin` is attached by construction in the same transaction. The rule cannot be violated —
  there is no code path that creates the profile without it.

### Pending vs. active guardian — the one thing to decide explicitly

A subtlety the rest of the doc previously left fuzzy: in the **self-minor** path the guardian is
*invited* (a `pending` user who hasn't accepted yet). So at the instant of registration, a Guardian
`ProfileLogin` **row exists**, but the guardian hasn't confirmed. Two questions, answered:

1. **Does the minor profile exist immediately?** Yes. The profile + the (pending) Guardian login are
   written together. The "≥1 Guardian login" invariant is satisfied by the row existing.
2. **Can the minor profile go *public* before the guardian has accepted?** **No.** Publishing a minor
   profile requires an **active** (accepted) guardian who holds `canPublish`. A pending guardian
   cannot publish, and the minor (`AthleteOwner`) does not hold publish rights by default (`03`). So:

```
Minor profile lifecycle:
  register ──► profile EXISTS (draft only) + guardian login PENDING
                     │
            guardian accepts invite
                     │
                     ▼
  guardian login ACTIVE ──► guardian can now approve + publish ──► profile can go PUBLIC
```

> **Net effect of the rule:** a minor's profile can be *created* the moment a guardian is named, but
> it stays a private draft and **cannot become publicly visible until a real guardian has accepted
> and published it**. This is the child-safety guarantee — no minor is ever publicly visible without
> an accountable adult having acted.

The guardian-flow path skips the pending state entirely: the caller is already an authenticated,
active guardian, so the profile can move toward publish as soon as it's set up.

---

## 2. Target shape — two commands, one shared helper

```
RegisterOwnAthleteCommand            RegisterChildAthleteCommand
   caller → AthleteOwner                caller → Guardian
   if minor: invite guardian            child  → AthleteOwner (login deferred)
        │                                     │
        └──────────────┬──────────────────────┘
                       ▼
         CreateAthleteProfileCore(...)   ← private shared helper
           - slug validation (taken / reserved)
           - new AthleteProfile (the ATHLETE's details)
           - default draft SiteConfig + theme + GlobalSettings
           - returns the tracked profile (no logins attached yet)
```

The helper owns everything identical between the two flows. Each command owns only the
**login-attachment** logic and its own validation rules. This is the "name commands by intent"
principle from earlier: two intents, shared mechanics extracted.

---

## 3. Command definitions

```csharp
// Self — the athlete is the caller
public record RegisterOwnAthleteCommand(
    string AuthProviderId,     // caller sub (from the authenticated principal, via controller)
    string Email,              // caller email
    string DisplayName,
    string Slug,
    DateTime DateOfBirth,
    string DefaultLocaleId,
    string? GuardianEmail = null   // required IF caller is a minor
) : IRequest<AthleteProfileDto>;

// Guardian creates a profile for a child — the parent is the caller
public record RegisterChildAthleteCommand(
    string AuthProviderId,     // GUARDIAN's sub (the caller)
    string Email,              // GUARDIAN's email
    string ChildDisplayName,   // the ATHLETE (child)
    string Slug,
    DateTime ChildDateOfBirth, // the ATHLETE's DOB
    string DefaultLocaleId
    // no child login fields in v1 — see §4
) : IRequest<AthleteProfileDto>;
```

---

## 4. The deferred child-login decision (recommended: defer)

**Recommendation: ship guardian-only first; do NOT take an `AthleteEmail` / child-login in v1.**

Reasoning:
- **It's the honest common case.** A young judoka does not have or need their own login. A profile
  with only a Guardian login is complete and fully functional — the guardian edits, approves, and
  publishes (the minor defaults in `03` already give the guardian publish + approval).
- **Avoids a half-built invite flow.** Supporting a child login now means a second invite/claim path
  (like the guardian one) before it's needed. That's machinery on spec — exactly what `07` warns
  against.
- **Clean upgrade later.** When an older minor wants their own login, add a separate
  `InviteAthleteOwnerLoginCommand` that attaches an AthleteOwner `ProfileLogin` (pending → claimed)
  to an existing guardian-managed profile. This reuses the existing pending-user + backfill
  machinery from the guardian flow. Nothing about the v1 schema blocks it.

So v1: `RegisterChildAthleteCommand` attaches **only** a Guardian login (the caller). The
AthleteOwner login is simply absent until/unless invited later.

---

## 5. Handler logic

### Shared helper

```csharp
private async Task<AthleteProfile> CreateAthleteProfileCore(
    string slug, string displayName, DateTime dob, string localeId, CancellationToken ct)
{
    slug = slug.ToLowerInvariant();
    if (await _profiles.SlugExistsAsync(slug, ct))
        throw new DomainException(ErrorCodes.SlugAlreadyTaken, slug);
    if (ReservedSlugs.Contains(slug))
        throw new DomainException(ErrorCodes.SlugReserved, slug);

    var profile = new AthleteProfile
    {
        Slug = slug,
        DisplayName = displayName,
        SportId = "judo",
        DateOfBirth = DateOnly.FromDateTime(dob),
        DefaultLocaleId = localeId,
        VisibilityStateId = "public"
    };
    _profiles.Add(profile);

    // default draft SiteConfig (theme + layout + global settings) — unchanged from today
    await AttachDefaultDraftSiteConfig(profile, ct);
    return profile;
}
```

### Self-registration

```csharp
public async Task<AthleteProfileDto> Handle(RegisterOwnAthleteCommand r, CancellationToken ct)
{
    var caller = await GetOrCreateUser(r.Email, r.AuthProviderId, ct);

    // idempotency: a user can't self-register two owned profiles
    if (await _profiles.GetOwnedByUserIdAsync(caller.Id, ct) is not null)
        throw new DomainException(ErrorCodes.ProfileAlreadyExists);

    var isMinor = r.DateOfBirth.AddYears(18) > DateTime.UtcNow;
    if (isMinor && string.IsNullOrWhiteSpace(r.GuardianEmail))
        throw new DomainException(ErrorCodes.GuardianEmailRequired);

    var profile = await CreateAthleteProfileCore(r.Slug, r.DisplayName, r.DateOfBirth, r.DefaultLocaleId, ct);
    _logins.Add(ProfileLogin.CreateOwner(caller.Id, profile.Id));

    if (isMinor)
    {
        var guardian = await GetOrCreatePendingUser(r.GuardianEmail!, ct);
        _logins.Add(ProfileLogin.CreateGuardian(guardian.Id, profile));
    }

    await _unitOfWork.SaveChangesAsync(ct);
    return Map(profile);
}
```

### Guardian-creates-profile-for-child

```csharp
public async Task<AthleteProfileDto> Handle(RegisterChildAthleteCommand r, CancellationToken ct)
{
    var guardian = await GetOrCreateUser(r.Email, r.AuthProviderId, ct); // caller IS the guardian

    var profile = await CreateAthleteProfileCore(
        r.Slug, r.ChildDisplayName, r.ChildDateOfBirth, r.DefaultLocaleId, ct);

    // caller becomes the Guardian; child AthleteOwner login deferred (§4)
    _logins.Add(ProfileLogin.CreateGuardian(guardian.Id, profile));

    await _unitOfWork.SaveChangesAsync(ct);
    return Map(profile);
}
```

> Note the asymmetry: self-registration guards against a duplicate *owned* profile, but a guardian
> can legitimately create **multiple** profiles (one per child) — so no single-profile idempotency
> guard there. (A guardian with three judoka kids = three `AthleteProfile`s, three guardian logins.)
> See §10 open item on duplicate detection.

---

## 6. Validation rules to keep consistent

- **Minor needs ≥1 Guardian — see §1a (the core rule).** Enforced *atomically* in both paths: the
  profile and a Guardian `ProfileLogin` are created in the same transaction, so a minor profile with
  zero guardians can never exist. Self-minor → guardian invited (pending); child-flow → caller is the
  guardian (active by construction).
- **Minor profile cannot go public without an *active* guardian.** Creating the profile is allowed
  with a pending guardian; **publishing** requires an accepted guardian holding `canPublish` (`03`).
  The minor `AthleteOwner` does not publish by default. (Publish gate, not a registration gate — but
  stated here so it isn't lost.)
- **Guardian registering an adult-aged person:** odd but possible (DOB ≥ 18). Decide: reject
  (`ErrorCodes.GuardianCannotRegisterAdult`) or allow guardian-managed adult. Lean: **reject** in v1
  — an adult should self-register; revisit if a real case appears.
- **Slug uniqueness/reserved:** centralized in the shared helper — single source of truth.
- **Claims read from the authenticated principal only**, never the request body (carries over from
  the auth plan). The principal is populated by whichever scheme authenticated the request — cookie
  *or* bearer — see §6a; the handlers don't care which.

---

## 6a. Authentication wiring — cookie (production) + bearer (Swagger)

**Problem:** `Program.cs` today registers **JWT bearer only**. That's why Swagger works — it sends
`Authorization: Bearer …`. But production users come through the **Next.js frontend on a cookie
session**, which this refactor's endpoints assume (§6, §9.5 say "claims from cookie JWT"). Before
these endpoints ship, the API must accept **both** schemes so the *same* `[Authorize]` endpoints
serve real users (cookie) and manual testing (bearer) without duplication.

This is a prerequisite for the registration split, not an afterthought: both
`athletes/register` and `athletes/register-child` read the caller's `sub`/email from the principal,
and that principal must be populated correctly regardless of how the caller authenticated.

### Dual-scheme setup (`Program.cs`)

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme          = "smart";
    options.DefaultChallengeScheme = "smart";
})
.AddCookie("cookie", options =>
{
    options.Cookie.Name     = "nextatlet.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    // Cookie.SecurePolicy = Always in production
})
.AddJwtBearer("bearer", options =>
{
    options.Authority = builder.Configuration["Auth0:Authority"];   // https://YOUR_DOMAIN/
    options.Audience  = builder.Configuration["Auth0:Audience"];    // your API identifier
})
.AddPolicyScheme("smart", "smart", options =>
{
    // Route each request to the right handler:
    //   Authorization: Bearer …  → JWT (Swagger, service-to-service)
    //   otherwise                → cookie (Next.js frontend)
    options.ForwardDefaultSelector = ctx =>
    {
        var auth = ctx.Request.Headers["Authorization"].FirstOrDefault();
        return auth?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? "bearer"
            : "cookie";
    };
});
```

Endpoints keep a plain `[Authorize]` — the policy scheme picks the handler automatically, so the
controllers in §7 need no per-scheme attributes.

### Claim extraction must be scheme-agnostic

`User.GetAuthProviderId()` / `User.GetEmail()` (§7) must resolve from whichever scheme ran. The two
schemes can surface the subject under different claim types (e.g. bearer `sub` vs the cookie's
`ClaimTypes.NameIdentifier`), so the helpers should check both:

```csharp
public static string GetAuthProviderId(this ClaimsPrincipal user) =>
    user.FindFirst("sub")?.Value
    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
    ?? throw new DomainException(ErrorCodes.MissingSubjectClaim);

public static string GetEmail(this ClaimsPrincipal user) =>
    user.FindFirst("email")?.Value
    ?? user.FindFirst(ClaimTypes.Email)?.Value
    ?? throw new DomainException(ErrorCodes.MissingEmailClaim);
```

> Whatever issues the cookie (the Next.js `@auth0/nextjs-auth0` layer, or an API-issued cookie if you
> later go that route — see the auth discussion) must carry the same subject + email claims the
> handlers rely on. The handler/command code is unchanged by all of this; only the principal's
> *source* differs.

### Notes / decisions

- **CORS + cookies:** the cookie path needs `AllowCredentials()` and an explicit allowed origin (not
  `*`) for the Next.js domain. The bearer path doesn't, but the same CORS policy covers both.
- **CSRF:** cookie auth on state-changing endpoints (both register endpoints are `POST`) needs CSRF
  protection or `SameSite=Strict`/`Lax` + a custom-header check. Bearer is immune. Confirm the
  approach before production — flagged in §10.
- **No change to Swagger:** it keeps sending bearer and keeps working unchanged.

---

## 7. Controller surface (two endpoints, both `[Authorize]`)

```csharp
[Authorize] [HttpPost("athletes/register")]            // self
public async Task<IActionResult> RegisterOwn(RegisterOwnAthleteRequest body, ...) {
    var sub = User.GetAuthProviderId(); var email = User.GetEmail();
    return Ok(await _mediator.Send(new RegisterOwnAthleteCommand(sub, email, /* body... */)));
}

[Authorize] [HttpPost("athletes/register-child")]      // guardian creates a profile for a child
public async Task<IActionResult> RegisterChild(RegisterChildAthleteRequest body, ...) {
    var sub = User.GetAuthProviderId(); var email = User.GetEmail();
    return Ok(await _mediator.Send(new RegisterChildAthleteCommand(sub, email, /* body... */)));
}
```

`sub`/`email` come from the authenticated principal (§6a) — cookie or bearer, transparently.

`/me` is unaffected in shape but now legitimately reports a caller who is **Guardian with owned
children but no AthleteOwner profile of their own** — already covered by the Guardian branch.

---

## 8. Refactor steps (order matters — keep green between steps)

1. **Extract `CreateAthleteProfileCore` + `GetOrCreateUser` / `GetOrCreatePendingUser`** out of the
   current `RegisterAthleteCommandHandler` with **no behavior change**. Existing tests stay green.
2. **Rename** `RegisterAthleteCommand` → `RegisterOwnAthleteCommand` (+ handler, + endpoint
   `athletes/register`). Pure rename; behavior identical. Update references.
3. **Add dual-scheme auth (§6a):** add the cookie scheme + `smart` policy scheme alongside the
   existing bearer; make `GetAuthProviderId`/`GetEmail` scheme-agnostic. Verify Swagger (bearer)
   still authenticates **and** a cookie-bearing request authenticates. No endpoint changes.
4. **Add `RegisterChildAthleteCommand`** + handler (guardian-only logins) + endpoint
   `athletes/register-child`, reusing the extracted helper.
5. **Add validation** for the guardian-registers-adult case (§6) and any new `ErrorCodes`
   (incl. `MissingSubjectClaim` / `MissingEmailClaim` from §6a).
6. **Tests:** self-adult, self-minor (guardian invited), guardian-registers-child (minor),
   guardian-registers-adult (rejected), guardian-registers-two-children (both succeed); plus the
   core-rule tests (§1a): self-minor **without** `GuardianEmail` → rejected and **no profile row
   written** (atomicity); minor profile created → a Guardian `ProfileLogin` row exists in the same
   transaction; minor profile with a **pending** guardian → publish is **blocked**; after guardian
   accepts (active) → publish allowed; plus auth: bearer-authenticated request and cookie-authenticated
   request both resolve the same principal.
7. *(later, not this refactor)* `InviteAthleteOwnerLoginCommand` to give a guardian-managed child
   their own login when they're old enough (§4).

---

## 9. Definition of done

1. One private helper owns slug validation + profile + default SiteConfig; both commands call it.
2. `RegisterOwnAthleteCommand`: caller → AthleteOwner; minor → guardian invited; rejects duplicate
   owned profile.
3. `RegisterChildAthleteCommand`: caller → Guardian; child AthleteOwner login deferred; one guardian
   may register multiple children.
4. **Core rule (§1a) holds:** a minor profile is **never** created without a Guardian `ProfileLogin`
   in the same transaction (both paths); a minor profile with only a *pending* guardian **cannot be
   published** until the guardian is active; an adult needs no guardian. Guardian-registers-adult
   handled per §6.
5. **Both auth schemes wired (§6a):** cookie (production / Next.js) and bearer (Swagger) authenticate
   the same `[Authorize]` endpoints; claim extraction is scheme-agnostic; Swagger still works
   unchanged.
6. Two `[Authorize]` endpoints; claims read from the authenticated principal only, never the body.
7. Test matrix in §8.6 passes (including the cookie-vs-bearer principal test).

---

## 10. Open items

- **Guardian-registers-adult:** confirm reject vs allow (lean: reject v1).
- **Duplicate child detection:** a guardian could double-submit and create two profiles for the same
  child (no natural unique key — names repeat). Decide if a soft guard is needed (e.g. warn on
  same-guardian + same name + same DOB) or leave to UX. Low priority.
- **`User` entity rename** (`User` → `AppUser`/`AuthIdentity`): still open; this refactor touches the
  same handlers, so a good moment to do both together if you've decided.
- **Cookie origin (§6a):** confirm where the production cookie is issued — Next.js `@auth0/nextjs-auth0`
  (recommended; API stays bearer-validating and the cookie is the frontend's session) vs an
  API-issued cookie after JWT validation. Affects what claims the cookie must carry.
- **CSRF policy (§6a):** decide CSRF protection for the cookie path on `POST` endpoints before
  production (token vs `SameSite` + custom-header). Bearer path unaffected.
- **Primary launch path:** which flow gets the smoothest UX first — self vs child? (Founder's cousin
  is an internationally competing judoka, likely older → possibly self; but the parent-as-operator
  pattern argues child-flow matters early too.)
```