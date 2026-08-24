# Get Sites (paged listing)

> **Source:** [`GetSitesQuery.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/Sites/GetSitesQuery.cs)
> **Endpoint:** `GET /api/sites`
> **Controller:** [`SitesController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/SitesController.cs) · **Auth:** authenticated (via global fallback)

## What it does

Returns a paged, filterable, sortable list of sites. Intended as a public directory listing.

## Request

Query string binds [`SiteListRequest`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Contracts/Sites/Request/SiteListRequest.cs) (extends `PagedQuery`):

| Param | Type | Meaning |
|-------|------|---------|
| `page` | int | 1-based; clamped to ≥ 1 |
| `pageSize` | int | clamped to 1–100 (default 20) |
| `sortBy` | string? | one of `slug`, `displayName`, `createdUtc`, `updatedUtc` (case-insensitive) |
| `sortDescending` | bool | flips sort direction |
| `search` | string? | `ILIKE %term%` on `Slug` or `DisplayName` |
| `siteType` | string? | `individual` / `organization`; null = any |
| `visibility` | string? | `public` / `private`; null = any |

## Response

`Result<PagedResult<SiteResponse>>` → **200**:

```json
{ "items": [ { "id": "…", "slug": "…", "displayName": "…", "defaultLocale": {…}, "visibilityState": {…} } ],
  "page": 1, "pageSize": 20, "totalCount": 42, "totalPages": 3, "hasPrevious": false, "hasNext": true }
```

## How it works

1. The repository runs the query via `SiteListQueryBuilder` over `Sites.AsNoTracking()` — one `COUNT` + one page query.
2. Map each `Site` to `SiteResponse`.
3. Return the paged result.

Default sort is `CreatedUtc` **ascending** (oldest first).

## Validation and error codes

No error codes. `SiteMapper` can throw (→ 500) if any row has a bad `DefaultLocaleId`/`VisibilityStateId`.

## Dependencies

`ISiteRepository` only.

## Transaction behaviour

None — read-only, `AsNoTracking`.

## Side effects

None.

## Gotchas

- **No visibility enforcement.** `visibility` is a client-chosen *filter*, never a server-enforced constraint — a caller can pass `?visibility=private` and enumerate every private site's slug and display name. There is no per-caller authorization on what sites are listed.
- A single bad enumeration id on any row **poisons the whole page** (the mapper throws → 500), because there's no DB-level validation of enumeration ids.
- The `search` uses a leading `%` wildcard, so it can't use an index.

## Related

- [Data model](../../03-data-model-erd.md) · [Get draft site snapshot](./get-draft-site-snapshot.md)
