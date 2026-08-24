# Scrape Clubs

> **Source:** [`ScrapeClubsCommand.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Application/Features/ClubRegistry/ScrapeClubsCommand.cs) · [`DjuPortalScraper.cs`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Infrastructure/ExternalServices/Scrape/DjuPortalScraper.cs)
> **Endpoint:** `POST /api/clubs/scrape`
> **Controller:** [`ClubsController`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Server/NextAtlet.Api/Controllers/ClubsController.cs) · **Auth:** `[AllowAnonymous]`

## What it does

Populates the **club registry** — the read-only list of real Danish clubs and their officials used to verify organizations. It scrapes the DJU portal (`djuportalen.dk`), canonicalizes the results, and upserts `Club` + `ClubOfficial` rows. This registry is *not* the same as `OrganizationProfile`.

## Request

```csharp
public record ScrapeClubsCommand(string Sport, string Country) : IRequest<string>;
```

Query params default to `sport = "judo"`, `country = "denmark"`. Returns a plain `string` (not a `Result`), so the response is a 200 with a summary string body.

## Response

**200** with a body like `"Imported 42 clubs from 1 source(s)."`

## How it works

1. Select strategies that support the requested `(sport, country)` — only `DjuPortalScraper` is registered.
2. For each: `FetchAsync` (Playwright + HtmlAgilityPack scrape), then upsert each canonicalized club, then deactivate clubs no longer in the feed.
3. One `SaveChangesAsync` covering the whole run.

The scraper uses **Playwright with `Channel = "chrome"`**, so it needs a real Google Chrome install (not the bundled Chromium).

## Validation and error codes

None. Zero matching strategies is a silent success. Any scraper/HTTP exception → 500.

## Dependencies

`IEnumerable<IClubSourceStrategy>`, `IClubCanonicalizer`, `IClubRepository`, `IUnitOfWork`.

## Transaction behaviour

A single commit covering every upsert + deactivation across all strategies — one scrape run is one atomic unit.

## Side effects

Creates/updates `Club` rows; **replaces all `ClubOfficial` rows** for each club (delete-and-reinsert, because officials have no stable key); sets `IsActive = false` on clubs missing from the feed; stamps `LastImportedUtc`.

## Gotchas

- **Unauthenticated + heavy.** `[AllowAnonymous]` on an endpoint that launches a headless browser and does a full external crawl + mass DB writes — a denial-of-service and data-integrity risk. The XML comment calls it "dev-only" but there is no environment guard.
- **Deactivation is broken by a source-key mismatch.** The scraper's `Source` property returns `"dju_portalen"`, but it stamps rows with `Source = "dju_portal"`; deactivation is called with the former, so it never matches the rows it wrote.
- Requires Google Chrome installed; fails otherwise.
- No tests cover this handler (the only ClubRegistry handler without coverage).

## Related

- [Add and remove sports](./add-and-remove-sports.md) · [List club officials](./list-club-officials.md) · [Send official email verification](./send-official-email-verification.md)
