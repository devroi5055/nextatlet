using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Entities.ClubRegistry;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Enumerations.Verification;

namespace NextAtlet.Infrastructure.Services;

/// <summary>
/// Maps free-text sport labels from a scraped source onto the canonical <see cref="Sport"/> enumeration.
/// Matching is by substring on a normalized (letters/digits, lower-cased) form, so combined words like
/// "Judoklub" still resolve, and a single label may name more than one sport ("Judo og Jiu-Jitsu Klub").
/// Unknown labels are dropped (a source listing a sport we don't model yet just isn't imported).
/// </summary>
public class ClubCanonicalizer : IClubCanonicalizer
{
    // Normalized alias (letters/digits only, lower-case) → canonical sport.
    private static readonly Dictionary<string, string> SportAliases = new()
    {
        ["judo"] = Sport.Judo.Id,
        ["jiujitsu"] = Sport.JiuJitsu.Id,
        ["juijitsu"] = Sport.JiuJitsu.Id,
        ["jujitsu"] = Sport.JiuJitsu.Id,
        ["jujutsu"] = Sport.JiuJitsu.Id,
    };
    private static readonly Dictionary<string, string> RoleAliases = new()
    {
        ["formand"] = ClubOfficialRole.Chairman.Id,
        ["kasserer"] = ClubOfficialRole.Cashier.Id,
        ["postadresse"] = ClubOfficialRole.PostalAddress.Id,
        ["instruktør"] = ClubOfficialRole.Instructor.Id,
        ["andenklubkontakt"] = ClubOfficialRole.Other.Id,
    };

    public ScrapedClub Canonicalize(ScrapedClub scrapedClub)
    {
        var sports = scrapedClub.Sports.ToList();
        var clubOfficials = scrapedClub.ScrapedOfficials.ToList();

        //canonicalize sports
        scrapedClub.Sports = sports
            .Select(NormalizeSport)
            .SelectMany(MatchSports)   // a single label can mention several sports
            .Distinct()
            .ToList();

        //canonicalize officials
        scrapedClub.ScrapedOfficials = clubOfficials
            .Select(NormalizeClubOfficial)
            .Select(MatchRole)
            .Distinct()
            .ToList();

        return scrapedClub;
    }

    // A label matches a sport when its normalized text contains a known alias — so "judoklub",
    // "københavnsjudoklub", etc. all resolve to Judo.
    private static IEnumerable<string> MatchSports(string normalized) =>
        normalized.Length == 0
            ? []
            : SportAliases.Where(a => normalized.Contains(a.Key)).Select(a => a.Value);

    private static ScrapedClubOfficial MatchRole(ScrapedClubOfficial normalized)
    {
        // Unknown role labels fall back to Other rather than throwing.
        normalized.Role = RoleAliases.GetValueOrDefault(normalized.Role, ClubOfficialRole.Other.Id);
        return normalized;
    }

    private static string NormalizeSport(string raw)
    {
        return new string((raw ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
    private static ScrapedClubOfficial NormalizeClubOfficial(ScrapedClubOfficial clubOfficial)
    {
        clubOfficial.Role = new string((clubOfficial.Role ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        return clubOfficial;
    }
}
