# Get Draft Site Snapshot

> **Source:** [`GetDraftSiteSnapshotQuery.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Sites/GetDraftSiteSnapshotQuery.cs) *(the record is named `GetDraftAthleteSiteSnapshotQuery`)*
> **Endpoint:** `GET /api/IndividualSites/{id}/config/draft`
> **Controller:** [`IndividualSitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/IndividualSitesController.cs) · **Auth:** authenticated (`[Authorize]`)

## What it does

Reads a site's current **draft** content — the unpublished layout (sections) plus global settings — so an editor could render it. This is the read half of what was going to be a draft edit/save cycle.

## Request

```csharp
public record GetDraftAthleteSiteSnapshotQuery(Guid SiteId) : IRequest<Result<SiteSnapshotResponse>>;
```

## Response

`Result<SiteSnapshotResponse>` → **200**:

```json
{ "id": "…", "siteId": "…",
  "layout": { "sections": [ { "id": "…", "order": 0, "data": { "type": "hero", … } } ] },
  "globalSettings": { "accentColor": "#ffd700", "fontFamily": "Inter" },
  "version": 0 }
```

## How it works

1. Read `Site.CurrentDraftSnapshotId`, then load that snapshot; missing → `config.draft.not_found`.
2. Map to `SiteSnapshotResponse`.

## Validation and error codes

| Error code | When | HTTP |
|------------|------|------|
| `config.draft.not_found` | Site has no draft snapshot | **400** (the controller advertises 404, but `ResultFilter` emits 400) |

## Dependencies

`ISiteSnapshotRepository` only.

## Transaction behaviour

None — read-only.

## Side effects

None.

## Gotchas

- **No authorization whatsoever.** Any authenticated user can read *any* site's unpublished draft — there's no login check, no `PermissionResolver` call, no visibility check. This is a real information-disclosure gap.
- **`version` is always `0`.** `SiteSnapshot` has no `Version` property anymore; the DTO field is a leftover of the removed optimistic-concurrency design and is never populated.
- The response omits `ThemeId` and `PublishedUtc`, so a client can't tell which theme renders the layout.
- **The write path is gone.** The matching `PUT {id}/config/draft` action is commented out; `EditDraftAthleteSiteSnapshotCommand` was deleted in the Site/SiteSnapshot refactor. That's why `UpdateSiteSnapshotRequest`, `ErrorCodes.DraftVersionConflict`, `ISanitizationService`, and `ISectionTypeRegistry` are all currently orphaned (registered but unused).

## Related

- [Get sites](./get-sites.md) · [Data model](../../03-data-model-erd.md)
