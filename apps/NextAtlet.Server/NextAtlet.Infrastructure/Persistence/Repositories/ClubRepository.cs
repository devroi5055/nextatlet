using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Interfaces.Repositories;
using NextAtlet.Domain.Entities.Verification;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Enumerations.Verification;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class ClubRepository : IClubRepository
{
    private readonly NextAtletDbContext _context;

    public ClubRepository(NextAtletDbContext context) => _context = context;

    public async Task<Club?> GetByIdAsync(string id, CancellationToken ct)
    {
        return await _context.Clubs
            .Include(c => c.Officials)
            .FirstOrDefaultAsync(c => c.Id.ToString() == id, ct);
    }
    public async Task<Club> UpsertAsync(ScrapedClub club, CancellationToken ct)
    {
        var existing = await _context.Clubs
            .Include(c => c.Officials)
            .FirstOrDefaultAsync( c => c.SourceKey == club.SourceKey && c.Source == club.Source, ct);


        if (existing is null)
        {
            var created = new Club
            {
                Source = club.Source,
                SourceKey = club.SourceKey,
                Name = club.Name,
                Address = club.Address,
                CountryId = Country.Denmark.Id,   // DJU is DK-only today; revisit when sources span countries
                SportIds = club.Sports.ToArray(),
                LastImportedUtc = DateTime.UtcNow,
            };
            _context.Clubs.Add(created);
            AddOfficials(created.Id, club.ScrapedOfficials);
            return created;
        }

        existing.Name = club.Name;
        existing.Address = club.Address;
        existing.LastImportedUtc = DateTime.UtcNow;
        existing.IsActive = true; // re-appearing in the feed reactivates it

        // Preserve admin-added sports: union the existing set with the freshly scraped one so a sport
        // an admin added manually (and the source doesn't report) survives the re-import.
        existing.SportIds = existing.SportIds.Union(club.Sports).ToList();

        // Officials come entirely from the source — replace them wholesale each import.
        _context.ClubOfficials.RemoveRange(existing.Officials);
        AddOfficials(existing.Id, club.ScrapedOfficials);
        return existing;
    }

    public async Task DeactivateMissingAsync(string source, IEnumerable<string> presentKeys, CancellationToken ct)
    {
        var present = presentKeys.ToList();
        var stale = await _context.Clubs
            .Where(c => c.Source == source && c.IsActive && !present.Contains(c.SourceKey))
            .ToListAsync(ct);

        foreach (var club in stale)
            club.IsActive = false;
    }

    private void AddOfficials(Guid clubId, IEnumerable<ScrapedClubOfficial>? officials)
    {
        foreach (var o in officials ?? [])
            _context.ClubOfficials.Add(new ClubOfficial
            {
                ClubId = clubId,
                Name = o.Name,
                Email = o.Email,
                Phone = o.Phone,
                RoleId = o.Role
            });
    }


}
