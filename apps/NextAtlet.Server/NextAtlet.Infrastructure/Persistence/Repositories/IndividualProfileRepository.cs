using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Interfaces.Repositories;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class IndividualProfileRepository : IIndividualProfileRepository
{
    private readonly NextAtletDbContext _context;

    public IndividualProfileRepository(NextAtletDbContext context) => _context = context;

    public async Task<IndividualProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.IndividualProfiles.FindAsync(id, cancellationToken);
    }

    public async Task<IndividualProfile?> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.IndividualProfiles.FirstOrDefaultAsync(
            p => p.SiteId == siteId,
            cancellationToken);
    }

    public void Add(IndividualProfile profile) => _context.IndividualProfiles.Add(profile);
}
