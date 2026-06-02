# NextAtlet Backend — Step 1 Quick Start

**Status:** ✅ Build complete. Database and API ready for integration testing.

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

- **Domain** — Entities (User, AthleteProfile, ProfileLogin, Theme, SiteConfig, MediaAsset)
- **Infrastructure** — DbContext, section registry, sanitization service
- **Application** — Commands (CreateAthlete, UpdateDraftConfig), Queries (GetDraftConfig)
- **API** — Controllers (AthletesController), dependency injection, middleware

### Section Registry Pattern

Extensible validator pattern for layout sections:

```csharp
public interface ISectionValidator
{
    string SectionType { get; }
    ValidationResult Validate(Section section);
}

// In registry:
registry.Register(new HeroSectionValidator());
registry.Register(new BioSectionValidator());
// Step 4+: registry.Register(new ResultsSectionValidator());
```

### Supported Section Types (Step 1)

- **hero** — headline + subheading + optional background image
- **bio** — bio text + highlight items

## Key Features

✅ **External IdP Auth** — User model references external identity provider (not password hashing)
✅ **Guardian Model** — Minors require linked guardian with configurable permissions
✅ **Draft-Only Config** — Full draft/published state in schema; publish flow is Step 3
✅ **Optimistic Concurrency** — SiteConfig.Version prevents lost updates
✅ **XSS Sanitization** — All text fields sanitized on save
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

- Architecture: `docs/01-architecture.md`
- Data model: `docs/02-data-model.md`
- Accounts & permissions: `docs/03-accounts-and-permissions.md`
- Implementation notes: `IMPLEMENTATION_NOTES.md`
- Step 1 completion: `STEP_1_COMPLETE.md`

---

**Built with:** ASP.NET Core, EF Core 9, Npgsql, .NET 10
**Database:** PostgreSQL 14+
**Status:** ✅ Ready for integration
