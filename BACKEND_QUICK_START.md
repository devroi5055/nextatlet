# NextAtlet Backend — Quick Start

Get the .NET backend building, running, and seeded locally. For the full onboarding docs see [`docs/confluence/`](docs/confluence/README.md); for the frontend see [`apps/NextAtlet.Client`](apps/NextAtlet.Client).

## Prerequisites

| Tool | Version | For |
|------|---------|-----|
| .NET SDK | **10.0** | All projects target `net10.0` |
| Podman | 5+ (plus a compose provider — see below) | Runs the dev PostgreSQL container |
| Google Chrome | any | *Only* if you call `POST /api/clubs/scrape` (Playwright uses `Channel = "chrome"`) |
| Auth0 tenant | — | Anything requiring login (everything except `/api/clubs/*`) |

## 1. Database

The dev database runs in **Podman**, defined in [`compose.yaml`](compose.yaml) at the repo root (Postgres 16, bound to `127.0.0.1:32768` only). Every dev connection string — `appsettings*.json`, the `Program.cs` fallback, and the EF design-time factory — points at it:

```
Host=localhost;Port=32768;Database=nextatlet;Username=postgres;Password=postgres
```

```bash
podman machine init && podman machine start   # one-time, if you don't have a Podman machine yet
podman compose up -d --wait                   # start the DB (repo root) — waits until it's healthy
podman compose down                           # stop it (add -v to also delete the data volume)
```

`podman compose` delegates to a compose provider — install either `docker-compose` (`winget install Docker.DockerCompose`, Apache-2.0, no Docker Desktop needed) or `podman-compose` (`pip install podman-compose`).

## 2. Secrets (optional)

Two settings default to empty and aren't in any appsettings file:

```bash
cd apps/NextAtlet.Server/NextAtlet.Api
dotnet user-secrets set "Resend:InviteApiKey" "<resend-key>"   # empty ⇒ emails are only logged (fine for dev)
dotnet user-secrets set "CvrApi:AccessToken"  "<cvr-token>"    # CVR lookup is currently unused anyway
```

## 3. Build & run

```bash
# repo root
dotnet restore NextAtlet.slnx
dotnet build   NextAtlet.slnx

dotnet run --project apps/NextAtlet.Server/NextAtlet.Api            # http  → http://localhost:5278
dotnet run --project apps/NextAtlet.Server/NextAtlet.Api -lp https  # https → https://localhost:7162
```

Swagger UI (Development only): **http://localhost:5278/swagger**.

> ### ⚠️ Development startup DROPS the database on every run
> In Development, `Program.cs` calls `Database.EnsureDeleted()` → `Database.Migrate()` → seed on every startup. **Every `dotnet run` wipes and recreates the DB.** Don't keep anything precious in it, and don't bother running `dotnet ef database update` manually while in Development.

## 4. What gets seeded

[`DevelopmentDataSeeder`](apps/NextAtlet.Server/NextAtlet.Api/Seeding/DevelopmentDataSeeder.cs) (idempotent — skips if any Site exists):

- Adults: `ada-jensen`, `bjorn-madsen`, `david-sorensen`
- Minors: `clara-holm`, `emma-lund` (each with a live consent token); one guardian invite on the first minor
- 5 organizations (2 clubs, academy, training centre, national team), all `Pending`
- 1 registry `Club` + chairman `ClubOfficial` + one live org-verification token

Auth subjects: `seed|{slug}`; emails: `{slug}@seed.nextatlet.dk`.

## 5. Migrations

```bash
dotnet ef migrations add <Name> \
  --project apps/NextAtlet.Server/NextAtlet.Infrastructure \
  --startup-project apps/NextAtlet.Server/NextAtlet.Api

dotnet ef database update \
  --project apps/NextAtlet.Server/NextAtlet.Infrastructure \
  --startup-project apps/NextAtlet.Server/NextAtlet.Api
```

## 6. Tests

```bash
dotnet test NextAtlet.slnx
```

Only `tests/NextAtlet.Application.Tests` has real source (~113 xUnit facts). The Api/Domain/Infrastructure test projects are empty shells.

## Smoke test

1. `podman compose up -d --wait`, then run the API.
2. Open Swagger, authorize with an Auth0 token whose audience is `https://api.nextatlet.dk`.
3. `GET /api/Me` → `{ registered: false }` for a brand-new subject.
4. `POST /api/IndividualSites/self-register` with a slug + adult DOB → 200 `SiteResponse`.
5. `GET /api/Me` again → `{ registered: true, role: "owner", … }`.

## Known caveats

- CI (`.github/workflows/dotnet.yml`) installs .NET 8 against net10.0 projects and **cannot pass**.
- Several endpoints have known auth gaps — see [`docs/06-features-and-problems.md`](docs/06-features-and-problems.md).
- `infra/` is empty (no IaC); there is a Railway `Dockerfile` under `apps/NextAtlet.Server` (builds locally with `podman build apps/NextAtlet.Server`).
