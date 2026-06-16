using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class AthleteProfileRepository : IAthleteProfileRepository
{
    private readonly NextAtletDbContext _context;

    public AthleteProfileRepository(NextAtletDbContext context) => _context = context;

    public async Task<AthleteProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AthleteProfiles.FindAsync(id, cancellationToken);
    }

    public async Task<AthleteProfile?> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.AthleteProfiles.FirstOrDefaultAsync(
            p => p.SiteId == siteId,
            cancellationToken);
    }

    public void Add(AthleteProfile profile) => _context.AthleteProfiles.Add(profile);
}
