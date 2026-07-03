using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Infrastructure.Persistence;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class OrganizationProfileRepository : IOrganizationProfileRepository
{
    private readonly NextAtletDbContext _context;

    public OrganizationProfileRepository(NextAtletDbContext context) => _context = context;

    public async Task<OrganizationProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OrganizationProfiles.FindAsync(id, cancellationToken);
    }

    public async Task<OrganizationProfile?> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.OrganizationProfiles.FirstOrDefaultAsync(
            p => p.SiteId == siteId,
            cancellationToken);
    }

    public void Add(OrganizationProfile profile) => _context.OrganizationProfiles.Add(profile);
}
