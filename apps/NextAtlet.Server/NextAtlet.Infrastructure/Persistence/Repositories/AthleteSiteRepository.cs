using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.AthleteProfile;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class AthleteSiteRepository : IAthleteSiteRepository
{
    private readonly NextAtletDbContext _context;

    public AthleteSiteRepository(NextAtletDbContext context) => _context = context;

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
        => _context.AthleteSites.AnyAsync(ap => ap.Slug == slug, cancellationToken);

    public async Task<AthleteSite?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AthleteSites.FindAsync([id], cancellationToken);

    public Task<AthleteSite?> GetOwnedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ownerRoleId = ProfileRole.AthleteOwner.Id;
        return _context.AthleteSites.FirstOrDefaultAsync(
            p => p.ProfileLogins.Any(l => l.UserId == userId && l.RoleId == ownerRoleId),
            cancellationToken);
    }

    public void Add(AthleteSite site) => _context.AthleteSites.Add(site);
}
