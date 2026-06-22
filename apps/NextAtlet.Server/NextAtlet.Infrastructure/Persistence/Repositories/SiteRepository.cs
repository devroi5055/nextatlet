using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Infrastructure.Persistence;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class SiteRepository : ISiteRepository
{
    private readonly NextAtletDbContext _context;

    public SiteRepository(NextAtletDbContext context) => _context = context;

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
        => _context.Sites.AnyAsync(ap => ap.Slug == slug, cancellationToken);

    public async Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Sites.FindAsync([id], cancellationToken);

    public Task<Site?> GetOwnedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ownerRoleId = IndividualRole.Owner.Id;
        return _context.Sites.FirstOrDefaultAsync(
            p => p.SiteLogins.Any(l => l.UserId == userId && l.SiteRoleId == ownerRoleId),
            cancellationToken);
    }

    public void Add(Site site) => _context.Sites.Add(site);
}
