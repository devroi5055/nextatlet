using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class SiteLoginRepository : ISiteLoginRepository
{
    private static readonly ProfileLoginStatus Active = ProfileLoginStatus.Active;

    private readonly NextAtletDbContext _context;

    public SiteLoginRepository(NextAtletDbContext context) => _context = context;

    public Task<SiteLogin?> GetActiveLoginAsync(Guid userId, Guid SiteId, CancellationToken cancellationToken = default)
        => _context.SiteLogins.FirstOrDefaultAsync(
            l => l.UserId == userId && l.SiteId == SiteId && l.StatusId == Active.Id,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetActiveGuardianSiteIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var guardianRoleId = ProfileRoles.Guardian.Id;
        return await _context.SiteLogins
            .Where(l => l.UserId == userId && l.SiteRoleId == guardianRoleId && l.StatusId == Active.Id)
            .Select(l => l.SiteId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasActiveOwnerLoginAsync(Guid SiteId, CancellationToken cancellationToken = default)
    {
        var ownerRoleId = ProfileRoles.AthleteOwner.Id;
        return _context.SiteLogins.AnyAsync(
            l => l.SiteId == SiteId && l.SiteRoleId == ownerRoleId && l.StatusId == Active.Id,
            cancellationToken);
    }

    public Task<bool> HasActiveGuardianLoginAsync(Guid SiteId, CancellationToken cancellationToken = default)
    {
        var guardianRoleId = ProfileRoles.Guardian.Id;
        return _context.SiteLogins.AnyAsync(
            l => l.SiteId == SiteId && l.SiteRoleId == guardianRoleId && l.StatusId == Active.Id,
            cancellationToken);
    }

    public void Add(SiteLogin login) => _context.SiteLogins.Add(login);
}
