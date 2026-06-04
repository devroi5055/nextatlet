using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class AthleteProfileRepository : IAthleteProfileRepository
{
    private readonly NextAtletDbContext _context;

    public AthleteProfileRepository(NextAtletDbContext context) => _context = context;

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
        => _context.AthleteProfiles.AnyAsync(ap => ap.Slug == slug, cancellationToken);

    public async Task<AthleteProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AthleteProfiles.FindAsync([id], cancellationToken);

    public void Add(AthleteProfile profile) => _context.AthleteProfiles.Add(profile);
}
