using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class SiteConfigRepository : ISiteConfigRepository
{
    private readonly NextAtletDbContext _context;

    public SiteConfigRepository(NextAtletDbContext context) => _context = context;

    public Task<SiteConfig?> GetDraftByProfileIdAsync(Guid athleteProfileId, CancellationToken cancellationToken = default)
        => _context.SiteConfigs.FirstOrDefaultAsync(sc => sc.AthleteProfileId == athleteProfileId && sc.IsDraft, cancellationToken);

    public void Add(SiteConfig config) => _context.SiteConfigs.Add(config);
}
