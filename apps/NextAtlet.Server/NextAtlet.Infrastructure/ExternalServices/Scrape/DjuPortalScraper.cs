using HtmlAgilityPack;
using Microsoft.Playwright;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Infrastructure.ExternalServices.Scrape;

public class DjuPortalScraper : IClubSourceStrategy
{
    public string Source => "dju_portalen";
    public IEnumerable<(string Sport, string Country)> Targets =>
    [
        (Sport.Judo.Id, "denmark"),
        (Sport.JiuJitsu.Id, "denmark"),   // matches Sport.JiuJitsu.Id
    ];

    public bool Supports(string sport, string country) => Targets.Contains((sport, country));


    private const string BaseUrl = "https://djuportalen.dk";
    private const string ClubsUrl = BaseUrl + "/DJU/Klubber";

    public async Task<ScrapedClub[]> FetchAsync(CancellationToken ct = default)
    {
        using var pw = await Playwright.CreateAsync();
        await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true, Channel = "chrome" });
        var page = await browser.NewPageAsync();

        var clubsRefs = await EnumerateAllClubsAsync(page, ct);

        var scrapedClubs = new List<ScrapedClub>();
        foreach (var club in clubsRefs)
        {
            await page.GotoAsync(BaseUrl + club.Url);
            var doc = ParseHtml(await page.ContentAsync());
            scrapedClubs.Add(ParseClubDetail(doc, club));
        }
        return scrapedClubs.ToArray();
    }
    private async Task<List<ClubRef>> EnumerateAllClubsAsync(IPage page, CancellationToken ct)
    {
        await page.GotoAsync(ClubsUrl);
        await page.Locator("[id$='_PanelSearch'] input[type='text']").First.FillAsync("%");

        const string clubRowSel =
            "[id$='_PanelSearchResult'] table.GridView tr td a[href$='.aspx']";

        // first search: click and just wait for result rows to render
        await page.ClickAsync("[id$='_ButtonSearch']");
        await page.WaitForSelectorAsync(clubRowSel);

        var all = new List<ClubRef>();
        var seen = new HashSet<string>();
        int nextPage = 2;

        while (true)
        {
            var doc = ParseHtml(await page.ContentAsync());
            foreach (var r in ParseClubRefs(doc))
                if (seen.Add(r.Id)) all.Add(r);

            var nextLink = page.Locator(
                "xpath=//*[contains(@id,'PanelSearchResult')]" +
                "//table[contains(@class,'GridView')]" +
                $"//a[normalize-space()='{nextPage}'][not(contains(@href,'.aspx'))]");

            if (await nextLink.CountAsync() == 0) break;   // no more pages

            // fingerprint the current first row, click, then wait until it changes
            var firstHref = await page.Locator(clubRowSel).First.GetAttributeAsync("href");

            await nextLink.First.ClickAsync();

            await Assertions.Expect(page.Locator(clubRowSel).First)
                .Not.ToHaveAttributeAsync("href", firstHref ?? "", new() { Timeout = 15000 });

            nextPage++;
        }

        return all;
    }

    private static List<ClubRef> ParseClubRefs(HtmlDocument doc)
    {
        var anchors = doc.DocumentNode.SelectNodes(
            "//*[contains(@id,'PanelSearchResult')]//table[contains(@class,'GridView')]" +
            "//tr/td/a[contains(@href,'.aspx')]");   // club detail links only
        if (anchors is null) return new();

        return anchors.Select(a =>
        {
            var href = a.GetAttributeValue("href", "");
            return new ClubRef(href, HtmlEntity.DeEntitize(a.InnerText).Trim(),
                               Path.GetFileNameWithoutExtension(href));
        }).ToList();
    }

    private static int? ParseCurrentPage(HtmlDocument doc)
    {
        var span = doc.DocumentNode.SelectSingleNode(
            "//tr[contains(@class,'GridPager')]//span");
        return int.TryParse(span?.InnerText?.Trim(), out var n) ? n : (int?)null;
    }

    public HtmlDocument ParseHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    private ScrapedClub ParseClubDetail(HtmlDocument doc, ClubRef club)
    {
        var street = FieldByLabel(doc, "Gade");
        var postal = FieldByLabel(doc, "Postnr");
        var city = FieldByLabel(doc, "By");

        var address = string.Join(", ", new[] { street, $"{postal} {city}".Trim() }
            .Where(s => !string.IsNullOrWhiteSpace(s)));   // "Skovvangskolen, Poppelvej 1A, 3450 Allerød"

        var email = doc.DocumentNode.SelectSingleNode("//a[contains(@id,'HyperLinkEmail')]")
            ?.GetAttributeValue("href", "").Replace("mailto:", "").Trim();
        var website = doc.DocumentNode.SelectSingleNode("//a[contains(@id,'HyperLinkURL')]")
            ?.GetAttributeValue("href", "").Trim();

        var officials = new List<ScrapedClubOfficial>();
        var rows = doc.DocumentNode.SelectNodes("//table[@id='GridViewContact']//tr[td]");
        foreach (var tr in rows ?? Enumerable.Empty<HtmlNode>())
        {
            var cells = tr.SelectNodes("./td");
            if (cells is null || cells.Count < 4) continue;
            var name = Clean(cells[1].InnerText);
            if (name.Length == 0) continue;
            officials.Add(new ScrapedClubOfficial
            {
                Role = Clean(cells[0].InnerText),
                Name = name,
                Email = cells[3].SelectSingleNode(".//a[starts-with(@href,'mailto:')]")
                              ?.GetAttributeValue("href", "").Replace("mailto:", "").Trim(),
                Phone = cells[3].SelectNodes("./text()")?.Select(t => Clean(t.InnerText))
                              .FirstOrDefault(t => t.Length > 0),
            });
        }

        return new ScrapedClub
        {
            SourceKey = club.Id,
            Source = "dju_portal",
            Name = club.Name,
            ScrapedOfficials = officials,
            Address = address,
            // DJU has no dedicated sport field — the sport lives in the club title (e.g. "… Judoklub").
            // The canonicalizer substring-matches, so handing it the title does the extract + map in one.
            Sports = [club.Name],
        };
    }

    static string? FieldByLabel(HtmlDocument doc, string label)
    {
        // the value is the first text node after the <div class="leftLabel"><span>Label</span></div>
        var node = doc.DocumentNode.SelectSingleNode(
            $"//div[@class='leftLabel'][span[normalize-space()='{label}']]/following-sibling::text()[1]");
        var v = node is null ? null : Clean(node.InnerText);
        return string.IsNullOrWhiteSpace(v) ? null : v;
    }

    static string Clean(string s) =>
    HtmlEntity.DeEntitize(s).Replace('\u00a0', ' ').Trim();   // decode &#248; etc.; &nbsp; → space


    private sealed record ClubRef(string Url, string Name, string Id);
}