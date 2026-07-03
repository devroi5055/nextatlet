# NextAtlet Backend — Step 1 Quick Start

**Status:** ✅ Build complete. Database and API ready for integration testing.

> **⚠️ Updated after the CQRS/MediatR refactor (2026-06-03).** The architecture now uses
> **MediatR** (`IRequest`/handlers via `ISender`) over a **repository + Unit of Work** layer,
> with the dependency direction inverted (`Infrastructure → Application`). Section payloads are
> **typed polymorphic DTOs** (the `type` discriminator lives inside `data`), not dictionaries.
> **For current request/response shapes use `NextAtlet.Api/NextAtlet.Api.http`** — some JSON
> examples below predate the typed-section + MediatR changes. See `docs/08` (ADR) and `CLAUDE.md`.

## Prerequisites

- .NET 10 SDK
- PostgreSQL 14+
- (Optional) Visual Studio Code, Rider, or Visual Studio

## Setup

### 1. PostgreSQL Database

```bash
# Create databases
createdb nextatlet          # production
createdb nextatlet_dev      # development

# Connection strings in appsettings.json already configured
# Default: Host=localhost;Port=5432;Database=nextatlet;Username=postgres;Password=postgres
```

### 2. Restore & Build

```bash
cd apps/NextAtlet.Server

# Restore NuGet packages
dotnet restore

# Build
dotnet build
```

### 3. Create & Apply Database Migration

```bash
# Generate migration from DbContext
dotnet ef migrations add InitialCreate \
  --project NextAtlet.Infrastructure \
  --startup-project NextAtlet.Api

# Apply to database
dotnet ef database update \
  --project NextAtlet.Infrastructure \
  --startup-project NextAtlet.Api
```

### 4. Run API

```bash
dotnet run --project NextAtlet.Api

# API listens on:
# https://localhost:5001
# http://localhost:5000
```

## API Testing

### Create an Athlete (Minor with Guardian)

```bash
curl -X POST http://localhost:5000/api/athletes \
  -H "Content-Type: application/json" \
  -d '{
    "email": "maria@example.dk",
    "displayName": "Maria Jensen",
    "slug": "maria-jensen",
    "dateOfBirth": "2007-06-15",
    "defaultLocale": "da",
    "guardianEmail": "aunt@example.dk"
  }'
```

Response:
```json
{
  "id": "uuid...",
  "slug": "maria-jensen",
  "displayName": "Maria Jensen",
  "dateOfBirth": "2007-06-15T00:00:00",
  "isMinor": true,
  "defaultLocale": "da"
}
```

### Get Draft Config

```bash
curl http://localhost:5000/api/athletes/{id}/config/draft
```

### Update Draft Config

```bash
curl -X PUT http://localhost:5000/api/athletes/{id}/config/draft \
  -H "Content-Type: application/json" \
  -d '{
    "layout": {
      "sections": [
        {
          "id": "hero-1",
          "type": "hero",
          "order": 0,
          "data": {
            "headline": { "da": "Velkommen", "en": "Welcome" },
            "subheading": { "da": "Min judoprofil", "en": "My judo profile" },
            "backgroundImageAssetId": null
          }
        },
        {
          "id": "bio-1",
          "type": "bio",
          "order": 1,
          "data": {
            "bio": { "da": "Jeg er...", "en": "I am..." },
            "highlightItems": [
              { "label": { "da": "Bælte", "en": "Belt" }, "value": "2. dan" }
            ]
          }
        }
      ]
    },
    "globalSettings": {
      "colors": { "primary": "#000000", "secondary": "#ffffff", "accent": "#ffd700" },
      "fonts": { "headingFont": "Inter", "bodyFont": "Inter" }
    },
    "expectedVersion": 1
  }'
```

## Architecture

### Layers

- **Domain** — Entities, enums, value objects (incl. typed `SectionData` hierarchy, `LocalizedText`)
- **Application** — MediatR `IRequest`/handlers (CreateAthlete, UpdateDraftConfig, GetDraftConfig), repository + `IUnitOfWork` **interfaces**, service interfaces, DTOs. No EF here.
- **Infrastructure** — `NextAtletDbContext`, repository + `EfUnitOfWork` implementations, section registry, sanitization service. References Application.
- **API** — Controllers (thin, inject `ISender`), `GlobalExceptionHandler`, DI wiring in `Program.cs`

> Dependency direction: `Api → Application ← Infrastructure`, both → `Domain`. Handlers never touch `DbContext`. See `docs/08`.

### Section Registry Pattern

Extensible validator pattern for layout sections:

```csharp
// ISectionValidator lives in Infrastructure; ValidationResult is an Application abstraction.
// Validators receive the already-typed, polymorphically-deserialized payload.
public interface ISectionValidator
{
    string SectionType { get; }
    ValidationResult Validate(SectionData data);
}

// In registry:
registry.Register(new HeroSectionValidator());
registry.Register(new BioSectionValidator());
// Step 4+: registry.Register(new ResultsSectionValidator());

// Application talks to ISectionTypeRegistry (IsSupported + Validate), not to individual validators.
```

### Supported Section Types (Step 1)

- **hero** — headline + subheading + optional background image
- **bio** — bio text + highlight items

## Key Features

✅ **External IdP Auth** — Auth0 (OIDC), dual-scheme (JWT bearer + cookie); just-in-time `UserProvisioner`
✅ **Guardian Model** — Minors get a linked guardian (consent via `ActionToken`); `ControlMode` + `PermissionResolver`
✅ **Draft/Published schema** — `Site` points at draft + published `SiteSnapshot`; publish flow is still Step 3
✅ **XSS Sanitization** — `SanitizationService` available for text/layout (wired in once the editor write path returns)
✅ **Bilingual Ready** — Locale maps (`{ "da": "...", "en": "..." }`) persisted; rendering deferred
✅ **Validation Seam** — Section registry scales to new types without schema changes

## Project Structure

```
apps/NextAtlet.Server/
├── NextAtlet.Domain/                — Entities & interfaces
├── NextAtlet.Infrastructure/        — DbContext, services, validators
├── NextAtlet.Application/           — Commands, queries, DTOs
├── NextAtlet.Api/                   — Controllers, Program.cs
└── NextAtlet.Server.slnx            — Solution file
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=nextatlet;..."
  },
  "Logging": { ... }
}
```

Override in `appsettings.Development.json` or environment variables:

```bash
export ConnectionStrings__DefaultConnection="Host=prod-db;..."
```

## Development

### Add a New Section Type (Example: Results)

1. Create `ResultsSectionValidator : ISectionValidator` in Infrastructure/Services/SectionRegistry
2. Register in `SectionTypeRegistry` constructor
3. Add to Theme.Manifest supportedSections
4. API immediately validates the new type

No migration or controller changes needed!

### Auth Integration (Step 2+)

Currently uses placeholder `authProviderId`. To integrate real Entra/Auth0:

1. Add JWT validation middleware in Program.cs
2. Extract claims from token → lookup/create User
3. Scoped authorization checks in controllers
4. Full implementation in Step 2

## Troubleshooting

### Build Fails: NuGet Package Not Found

```bash
dotnet nuget locals all --clear
dotnet restore
```

### Migration Failed

```bash
# Rollback last migration
dotnet ef migrations remove --project NextAtlet.Infrastructure

# Re-apply
dotnet ef database update
```

### API Won't Start: Database Connection Error

- Check PostgreSQL is running
- Verify connection string in appsettings.json
- Ensure database exists: `createdb nextatlet_dev`

## Next Steps

- **Step 2:** Public render endpoint (GET /api/athletes/{slug}/public)
- **Step 3:** Publish flow (draft → published)
- **Step 4:** Add `results` + `gallery` section types
- **Step 5:** Theme picker + multiple themes
- **Step 6:** Tiers + subscription gating

## References

- Global context + implemented-vs-planned status: `CLAUDE.md`
- Architecture: `docs/01-architecture.md`
- Data model: `docs/02-data-model.md`
- Accounts & permissions: `docs/03-accounts-and-permissions.md`
- ADR (CQRS/MediatR, layering): `docs/08-adr-cqrs-mediatr-and-layering.md`
- Live request/response shapes: `NextAtlet.Api/NextAtlet.Api.http`

> The build has progressed well past "Step 1": auth, the two-gate registration (self/guardian/org), action-token flows, consent, the control model, the club registry, and the `GET /api/Me` decision gate all exist. See the status note in `CLAUDE.md` for what's built vs planned. (The numbered "Step N" docs in `apps/NextAtlet.Server/` are point-in-time session notes from the initial build, not current status.)

---

**Built with:** ASP.NET Core, EF Core, Npgsql, .NET 10
**Database:** PostgreSQL 14+
