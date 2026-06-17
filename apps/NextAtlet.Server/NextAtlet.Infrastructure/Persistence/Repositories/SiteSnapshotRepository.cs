using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Interfaces.Repositories;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class SiteSnapshotRepository : ISiteSnapshotRepository
{
    private readonly NextAtletDbContext _context;

    public SiteSnapshotRepository(NextAtletDbContext context) => _context = context;

    public async Task<SiteSnapshot?> GetCurrentDraftBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        
        var draftId = await _context.Sites
            .Where(p => p.Id == siteId)
            .Select(p => p.CurrentDraftSnapshotId)
            .FirstOrDefaultAsync(cancellationToken);

        if (draftId == null) return null;
        return await _context.SiteSnapshots.FindAsync([draftId.Value], cancellationToken);
    }

    public void Add(SiteSnapshot snapshot) => _context.SiteSnapshots.Add(snapshot);
}
