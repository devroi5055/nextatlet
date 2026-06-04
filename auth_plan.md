# Backend Implementation Plan — Auth + Athlete Registration

**Goal:** wire Auth0 (OIDC) authentication in front of athlete profile registration, with the
guardian requirement enforced at profile-registration time (driven by DOB), not at auth time.

**Token transport — cookie-based.** The JWT is carried in an **httpOnly + Secure + SameSite cookie**,
not in a raw `Authorization` header from browser JS. The API still validates a standard JWT; it just
reads it out of the cookie. This protects the token from XSS theft — important for a platform handling
guardians' sessions over minors' data. Swagger is configured to use the same cookie so the API has a
single auth path.

**Principle:** authentication and domain registration are two separate gates.
- **Gate 1 — Auth0 (authentication):** credential creation, age-blind, guardian-free. Not built by us.
- **Gate 2 — Profile registration (domain):** display name + DOB + conditional guardian. Built by us,
  sits behind `[Authorize]`.

Touches: `01` (auth layer), `02` (`User`/`AuthIdentity`, `ProfileLogin`), `03` (guardian model),
`05` (signup gate), `07` (build order, pattern log).

---

## The flow this implements

```
New user
   │  clicks "Sign up" in frontend
   ▼
Auth0 hosted page  ── creates credential (email + password) ── issues JWT
   │  Next.js auth handler stores JWT in httpOnly + Secure + SameSite cookie
   ▼
Frontend calls GET /me  (cookie sent automatically)  ──► { registered: false }
   │
   ▼
Frontend shows registration form (DOB drives whether guardian field appears)
   │
   ▼
POST /athletes/register  (cookie carries JWT; API reads + validates it)
   │
   ▼
RegisterAthleteCommand:
   ├── GetOrCreate User from sub + email claims
   ├── Create AthleteProfile
   ├── Create ProfileLogin (AthleteOwner)
   └── if minor (computed from DOB):
         ├── require guardian email (else reject)
         ├── GetOrCreate pending guardian User (no sub yet)
         └── Create ProfileLogin (Guardian)
```

---

## Step 1 — JWT validation reading from a cookie

Wire Auth0 as the JWT authority, but pull the token from the cookie instead of (only) the
`Authorization` header. The validation itself is unchanged — same authority, audience, signature
checks — only the *source* of the token differs.

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"]; // https://nextatlet-dev.eu.auth0.com/
        options.Audience  = builder.Configuration["Auth:Audience"];  // https://api.nextatlet.dk

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                // Read the JWT out of the cookie. Fall back to the header if absent
                // (lets tooling / machine clients still send a Bearer header).
                if (ctx.Request.Cookies.TryGetValue("access_token", out var token))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// after building the app — order matters
app.UseAuthentication();
app.UseAuthorization();
```

```json
// appsettings.Development.json
{
  "Auth": {
    "Authority": "https://nextatlet-dev.eu.auth0.com/",
    "Audience": "https://api.nextatlet.dk"
  }
}
```

**Cookie attributes (set wherever the cookie is issued — see Step 1b):**
`HttpOnly = true`, `Secure = true`, `SameSite = Lax` (or `Strict` if frontend and API share a site).
`HttpOnly` blocks JS access (XSS protection); `Secure` forces HTTPS; `SameSite` is the CSRF guard.

**Checkpoint:** an `[Authorize]` endpoint returns 401 with no cookie, 200 once the `access_token`
cookie is present and valid.

---

## Step 1b — Where the cookie gets set, and CSRF

Two viable places to set the cookie; pick based on where the Auth0 redirect lands.

- **Next.js sets it (recommended):** the Auth0 redirect returns to the Next.js app; an auth library
  (Auth.js / `@auth0/nextjs-auth0`) exchanges the code for the JWT server-side and writes the
  httpOnly cookie. The .NET API only ever *reads* it. Cleanest separation.
- **.NET sets it:** if the redirect lands on the API, a `/auth/callback` endpoint exchanges the code
  and writes the cookie via `Response.Cookies.Append("access_token", jwt, cookieOptions)`.

**CSRF — the one cost of cookies.** Because the browser sends the cookie automatically, a malicious
site could trigger an authenticated request. Mitigations, layered:
- `SameSite=Lax/Strict` blocks the classic cross-site form-post vector (covers most cases).
- For state-changing endpoints, add anti-forgery: ASP.NET's antiforgery token, or require a custom
  header (e.g. `X-Requested-With`) that simple cross-site requests cannot set.
- Public read endpoints need no CSRF protection (no state change, no cookie required).

> Decide the cookie-issuer (Next.js vs .NET) before wiring the frontend — it changes which app owns
> the Auth0 client secret. Next.js-issued is the documented lean.

---

## Step 2 — Claims extraction helper

Keep claim-key strings in one place; avoid the `User` (ClaimsPrincipal) vs `User` (entity) confusion
at every call site.

```csharp
public static class ClaimsPrincipalExtensions
{
    public static string GetAuthProviderId(this ClaimsPrincipal p)
        => p.FindFirstValue("sub")
           ?? throw new DomainException(ErrorCodes.AuthSubMissing);

    public static string GetEmail(this ClaimsPrincipal p)
        => p.FindFirstValue(ClaimTypes.Email) ?? p.FindFirstValue("email")
           ?? throw new DomainException(ErrorCodes.AuthEmailMissing);
}
```

> Auth0 sometimes puts email under a namespaced claim depending on token config; fall back as shown,
> and verify what your tokens actually contain (decode one at jwt.io during Step 1).

---

## Step 3 — `GET /me` — the domain-gate check

Tells the frontend which side of Gate 2 the caller is on, so it can route to the registration form
or the dashboard.

```csharp
[Authorize]
[HttpGet("me")]
public async Task<IActionResult> Me(CancellationToken ct)
{
    var authProviderId = User.GetAuthProviderId();
    var user = await _users.GetByAuthProviderIdAsync(authProviderId, ct);

    if (user == null)
        return Ok(new MeDto(Registered: false, Role: null));

    // a user may be an athlete owner, a guardian, or both — report what applies
    var profile = await _profiles.GetOwnedByUserIdAsync(user.Id, ct);
    var isGuardian = await _logins.HasGuardianLoginAsync(user.Id, ct);

    return Ok(new MeDto(
        Registered: profile != null,
        Role: profile != null ? "AthleteOwner" : (isGuardian ? "Guardian" : null)));
}

public record MeDto(bool Registered, string? Role);
```

**Why this matters for the guardian case:** a guardian who authenticates (after accepting an invite)
has a `User` + a `Guardian` `ProfileLogin` but is NOT an `AthleteOwner`. `/me` reports
`Registered: false, Role: "Guardian"` so the frontend doesn't push them into athlete registration.

---

## Step 4 — `RegisterAthleteCommand` (keep it as one flow)

User creation stays *inside* the command via GetOrCreate — for a new user, registration is the first
authenticated action, so a separate sync step would be a no-op round trip. Claims come from the
controller, never the request body.

```csharp
public record RegisterAthleteCommand(
    string AuthProviderId,   // from JWT (controller)
    string Email,            // from JWT (controller)
    string DisplayName,      // from form
    string Slug,             // from form
    DateTime DateOfBirth,    // from form — drives minor gating
    string DefaultLocaleId,  // from form
    string? GuardianEmail = null) : IRequest<AthleteProfileDto>;
```

Handler responsibilities (largely your current logic, tightened):

```csharp
public async Task<AthleteProfileDto> Handle(RegisterAthleteCommand request, CancellationToken ct)
{
    var slug = request.Slug.ToLowerInvariant();

    if (await _profiles.SlugExistsAsync(slug, ct))
        throw new DomainException(ErrorCodes.SlugAlreadyTaken, slug);
    if (ReservedSlugs.Contains(slug))
        throw new DomainException(ErrorCodes.SlugReserved, slug);

    // minor status is COMPUTED, never stored
    var isMinor = request.DateOfBirth.AddYears(18) > DateTime.UtcNow;
    if (isMinor && string.IsNullOrWhiteSpace(request.GuardianEmail))
        throw new DomainException(ErrorCodes.GuardianEmailRequired);

    // the athlete's own auth identity (already authenticated)
    var user = await GetOrCreateUser(request.Email, request.AuthProviderId, ct);

    var profile = new AthleteProfile { /* slug, displayname, dob, locale, ... */ };
    _profiles.Add(profile);
    _logins.Add(ProfileLogin.CreateOwner(user.Id, profile.Id));

    if (isMinor)
    {
        var guardian = await GetOrCreatePendingUser(request.GuardianEmail!, ct);
        _logins.Add(ProfileLogin.CreateGuardian(guardian.Id, profile));
    }

    // ... default SiteConfig (draft) as today ...

    await _unitOfWork.SaveChangesAsync(ct);
    return Map(profile);
}
```

**Idempotency guard:** if this user already owns a profile, reject rather than create a second.
```csharp
if (await _profiles.GetOwnedByUserIdAsync(user.Id, ct) is not null)
    throw new DomainException(ErrorCodes.ProfileAlreadyExists);
```

---

## Step 5 — Two distinct user-creation paths

The athlete is authenticated (has `sub`); the guardian is invited (no `sub` yet). Model the
difference explicitly — `AuthProviderId` becomes nullable.

```csharp
// athlete — authenticated, real sub
private async Task<User> GetOrCreateUser(string email, string authProviderId, CancellationToken ct)
{
    var user = await _users.GetByAuthProviderIdAsync(authProviderId, ct);
    if (user == null)
    {
        user = new User { Email = email, AuthProviderId = authProviderId };
        _users.Add(user);
    }
    return user;
}

// guardian — invited, not yet authenticated; looked up by email since no sub exists
private async Task<User> GetOrCreatePendingUser(string email, CancellationToken ct)
{
    var user = await _users.GetByEmailAsync(email, ct);
    if (user == null)
    {
        user = new User { Email = email, AuthProviderId = null }; // pending
        _users.Add(user);
    }
    return user;
}
```

`GetByAuthProviderIdAsync` must never match a pending (null-sub) row — naturally safe since pending
rows have `null` there.

---

## Step 6 — Guardian claim flow (separate, later in build)

When the invited guardian first authenticates via Auth0, their pending `User` row gets its real
`sub` backfilled. This is a distinct command, triggered on the guardian's first authenticated call.

```csharp
// ClaimGuardianInviteCommand
var user = await _users.GetByEmailAsync(email, ct)
           ?? throw new DomainException(ErrorCodes.GuardianInviteNotFound);
if (user.AuthProviderId != null) return; // already claimed
user.AuthProviderId = authProviderId;     // backfill — EF already tracks it
```

> Build this when the guardian-invite email/acceptance flow lands (build-order step 12 / approval
> workflow territory). For the first milestone, creating the pending guardian row in Step 4 is enough.

---

## Step 7 — Apply `[Authorize]` correctly

- `POST /athletes/register` → `[Authorize]` (need claims to know who's registering).
- `GET /me` → `[Authorize]`.
- Public athlete site read endpoints → **anonymous** (they serve the published public contract;
  nothing here is gated by login). Keep these explicitly `[AllowAnonymous]` so the global authorize
  default doesn't accidentally lock them.

Recommended: set a global "authenticated by default" policy and opt public endpoints out, so a new
endpoint is locked unless deliberately opened.

```csharp
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build());
// public read controllers/actions get [AllowAnonymous]
```

---

## Step 8 — Swagger with the same cookie

Since the API reads the JWT from the `access_token` cookie (Step 1), Swagger must result in that
cookie being set rather than sending a Bearer header. Because `OnMessageReceived` already falls back
to the header, you have two options:

- **Cookie path (matches production exactly):** drive the Auth0 Authorization-Code + PKCE flow from
  Swagger, then have the callback set the `access_token` cookie — so Swagger calls behave identically
  to the browser app. This is what you did in your previous project.
- **Header fallback (quicker for pure API testing):** keep the Bearer definition in Swagger; the
  `OnMessageReceived` header fallback accepts it. Useful for isolated endpoint testing, but it does
  not exercise the cookie path.

Swagger OAuth definition (Authorization Code + PKCE against Auth0):

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{authority}authorize?audience={audience}"),
                TokenUrl         = new Uri($"{authority}oauth/token"),
                Scopes = new Dictionary<string, string>
                {
                    ["openid"] = "OpenID", ["profile"] = "Profile", ["email"] = "Email"
                }
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme, Id = "oauth2" } },
            new[] { "openid", "profile", "email" }
        }
    });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NextAtlet API v1");
    c.OAuthClientId(swaggerClientId);   // the SPA app's client id
    c.OAuthUsePkce();                   // required — no secret in the browser
});
```

> Whichever Swagger path you choose, the **API config does not change** — it always reads the cookie
> first, header second. Swagger is just a client. The `audience` query param on the authorize URL is
> the Auth0 gotcha: omit it and Auth0 returns an opaque token, not a JWT, and validation fails.

---

## Build order (each shippable)

1. **Step 1 / 1b / 2** — cookie-reading JWT validation + cookie-issuer decision + claims helper.
   Verify 401 without cookie, 200 with a valid `access_token` cookie.
2. **Step 4–5** — `RegisterAthleteCommand` reading claims; nullable `AuthProviderId`; pending
   guardian creation; idempotency guard.
3. **Step 3** — `GET /me` so the frontend can route new vs returning users.
4. **Step 7** — lock down authorization defaults; mark public reads anonymous.
5. **Step 8** — Swagger wired to the cookie path (or header fallback for quick testing).
6. *(later, with invite flow)* **Step 6** — `ClaimGuardianInviteCommand`.

---

## Definition of done

1. `[Authorize]` endpoints reject requests with no valid `access_token` cookie (401) and accept a
   valid one (200).
2. The JWT is read from the httpOnly cookie; claims (`sub`, `email`) are read only from the validated
   token, never the request body.
3. Cookie is set `HttpOnly + Secure + SameSite`; state-changing endpoints have CSRF mitigation.
4. `RegisterAthleteCommand` creates user (GetOrCreate) + profile + owner login in one flow; rejects
   a second profile for the same user.
5. Minor status is computed from DOB; guardian email required for minors; pending guardian row
   created with `AuthProviderId = null`.
6. `GET /me` correctly distinguishes: unregistered, AthleteOwner, Guardian.
7. Public read endpoints remain anonymous; everything else authenticated by default.

---

## Open items to confirm

- **Cookie issuer:** Next.js-issued (lean) vs .NET `/auth/callback`-issued. Decide before frontend
  wiring — determines which app holds the Auth0 client secret.
- **Email claim shape:** decode a real Auth0 token and confirm where `email` lands (top-level vs
  namespaced). Adjust the helper accordingly.
- **`User` entity rename:** still open from earlier (`User` vs `AppUser`/`AuthIdentity`) — decide
  before this code spreads, since it touches every repository here.
- **Guardian-as-first-login edge:** if a guardian could ever authenticate before the athlete's
  profile exists, the `/me` Guardian branch covers reporting, but confirm the frontend route for
  "guardian with nothing to approve yet."