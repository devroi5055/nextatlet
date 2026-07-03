using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Consent;
using NextAtlet.Infrastructure.Persistence;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class GuardianConsentRepository : IGuardianConsentRepository
{
    private readonly NextAtletDbContext _context;

    public GuardianConsentRepository(NextAtletDbContext context) => _context = context;

    public void Add(GuardianConsent consent) => _context.GuardianConsents.Add(consent);

    public Task<bool> ExistsForProfileAsync(Guid siteId, CancellationToken cancellationToken = default)
        => _context.GuardianConsents.AnyAsync(c => c.SiteId == siteId, cancellationToken);
}
