# Backend: Configuration

Everything you can configure in the backend, where it's read, and which options class binds it. Source of truth: [`appsettings.json`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/appsettings.json), [`appsettings.Development.json`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/appsettings.Development.json), and [`Program.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Program.cs).

## appsettings keys

| Key | Purpose | Bound by |
|-----|---------|----------|
| `ConnectionStrings:DefaultConnection` | Npgsql connection. **Committed default uses port 32768** (a Docker-mapped port), not 5432. | `Program.cs` |
| `Authentication:Authority` | Auth0 tenant URL (trailing slash required) | read via indexer (no options class) |
| `Authentication:Audience` | JWT audience — must equal the frontend's `AUTH0_AUDIENCE` | indexer |
| `Authentication:EmailClaimType` | Namespaced email claim added by an Auth0 Action (`https://nextatlet.dk/email`) | `ClaimsPrincipalExtensions.ConfiguredEmailClaimType` |
| `Authentication:Swagger:ClientId` | Swagger UI PKCE client id (real SPA client in Development) | indexer |
| `Authentication:Swagger:Scopes` | Declared but **read only by dead code** — Program.cs hardcodes the scopes | — |
| `CvrApi:BaseUrl` | CVR company API base (`https://datacvrapi.dk/`) | `CvrApiOptions` |
| `CvrApi:TimeoutSeconds` | HTTP timeout for CVR calls | `CvrApiOptions` |
| `Resend:FromAddress` | Verified sender address | `EmailOptions` |
| `Resend:FromName` | Sender display name — **declared but never used** | `EmailOptions` |
| `Resend:AppBaseUrl` | Frontend base URL for building accept links | `EmailOptions` |
| `Logging:LogLevel:*`, `AllowedHosts` | Standard host config | host |

## Options classes bound in code

These are bound in `Program.cs` but **their config sections are NOT in any appsettings file**, so they always run on their hardcoded defaults:

| Section → Options class | Setting | Default | Meaning |
|-------------------------|---------|---------|---------|
| `Invitations` → [`InvitationOptions`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Common/Options/InvitationOptions.cs) | `ExpiryDays` | 7 | Token lifetime |
| | `RetentionDays` | 90 | **Never read** — no cleanup job exists |
| `AgeThresholds` → [`AgeThresholdOptions`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Common/Options/AgeThresholdOptions.cs) | `AbsoluteMinimumAge` | 13 | Can't self-register below this |
| | `SelfConsentAge` | 16 | Below this, a guardian must consent |
| | `GuardianBoundary` | 18 | Documented as the adult boundary but **never read** (18 is hardcoded in `AgePolicy`) |
| `Terms` → [`TermsOptions`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Common/Options/TermsOptions.cs) | `CurrentVersion` | `"2026-01"` | Terms version stamped into consent records |
| `Resend` → [`EmailOptions`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Common/Options/EmailOptions.cs) | `InviteApiKey` | `""` | Resend API key — **empty means email is only logged, not sent** |

## Secrets (user-secrets / environment)

| Setting | Where it should live | Effect if unset |
|---------|----------------------|-----------------|
| `Resend:InviteApiKey` | user-secrets (`UserSecretsId` in the csproj) or env `Resend__InviteApiKey` | Falls back to `LoggingEmailService` (logs the link, no real email) — fine for local dev |
| `CvrApi:AccessToken` | user-secrets or env `CvrApi__AccessToken` | CVR lookup sends an empty bearer (but the CVR service is unused anyway) |

> The appsettings comment says to set the key via `Email__ApiKey` — that's wrong on both counts. The correct env var is `Resend__InviteApiKey`.

Set secrets with:

```bash
cd apps/NextAtlet.Server/NextAtlet.Api
dotnet user-secrets set "Resend:InviteApiKey" "<key>"
```

## Email behaviour

`Program.cs` branches at startup: if `Resend:InviteApiKey` is non-empty it wires up [`ResendEmailService`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Infrastructure/Services/ResendEmailService.cs) (real emails via `api.resend.com`); otherwise [`LoggingEmailService`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Infrastructure/Services/LoggingEmailService.cs) (logs the action link). Both are **best-effort** — a send failure is logged and never fails the request. A missing `Resend` config section is a **hard startup failure** (`GetRequiredSection`).

## CORS

One policy, `"Development"`: `SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials()` — reflect-any-origin **with credentials**. It's only applied in Development, but it's one line away from being an account-takeover primitive if enabled in production.

## Middleware pipeline order

1. Exception handler (`GlobalExceptionHandler`)
2. (Development only) Swagger + Swagger UI + CORS `"Development"`
3. (non-Development) HTTPS redirection
4. Authentication → Authorization
5. Controllers
6. (Development only) **`Database.EnsureDeleted()` → `Database.Migrate()` → seed** — this drops the DB on every run
7. Run

## Ports

| Profile | URL |
|---------|-----|
| http | http://localhost:5278 |
| https | https://localhost:7162 |
| Swagger | http://localhost:5278/swagger (Development only) |

## Things to know

- No health checks, no background/hosted services, no rate limiting, no response caching/compression, no retry/resilience (no Polly, no Npgsql retry).
- `Microsoft.AspNetCore.OpenApi` is referenced only for a **dead** `OAuthSecuritySchemeTransformer` (never registered); Swagger is Swashbuckle.
- CI ([`.github/workflows/dotnet.yml`](https://github.com/devroi5055/nextatlet/blob/main/.github/workflows/dotnet.yml)) pins .NET 8 against net10.0 projects and cannot pass. `infra/` is empty.

## Related

- [Backend: Authentication & Tokens](./authentication-and-tokens.md) · [Running the Application](../04-running-the-application.md)
