using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Interfaces.Repositories;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class GuardianConsentRepository : IGuardianConsentRepository
{
    private readonly NextAtletDbContext _context;

    public GuardianConsentRepository(NextAtletDbContext context) => _context = context;

    public void Add(GuardianConsent consent) => _context.GuardianConsents.Add(consent);

    public Task<bool> ExistsForProfileAsync(Guid athleteProfileId, CancellationToken cancellationToken = default)
        => _context.GuardianConsents.AnyAsync(c => c.IndividualProfileId == athleteProfileId, cancellationToken);
}
