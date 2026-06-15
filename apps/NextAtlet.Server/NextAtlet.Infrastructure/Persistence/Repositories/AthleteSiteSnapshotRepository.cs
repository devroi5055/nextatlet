using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class AthleteSiteSnapshotRepository : IAthleteSiteSnapshotRepository
{
    private readonly NextAtletDbContext _context;

    public AthleteSiteSnapshotRepository(NextAtletDbContext context) => _context = context;

    public async Task<AthleteSiteSnapshot?> GetDraftByProfileIdAsync(Guid athleteProfileId, CancellationToken cancellationToken = default)
    {
        var draftId = await _context.AthleteSites
            .Where(p => p.Id == athleteProfileId)
            .Select(p => p.CurrentDraftSnapshotId)
            .FirstOrDefaultAsync(cancellationToken);

        if (draftId == null) return null;
        return await _context.AthleteSiteSnapshots.FindAsync([draftId.Value], cancellationToken);
    }

    public void Add(AthleteSiteSnapshot snapshot) => _context.AthleteSiteSnapshots.Add(snapshot);
}
