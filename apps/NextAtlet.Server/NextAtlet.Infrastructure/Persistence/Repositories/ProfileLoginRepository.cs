using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class ProfileLoginRepository : IProfileLoginRepository
{
    private readonly NextAtletDbContext _context;

    public ProfileLoginRepository(NextAtletDbContext context) => _context = context;

    public Task<bool> HasActiveLoginAsync(Guid userId, Guid profileId, CancellationToken cancellationToken = default)
    {
        const ProfileLoginStatus active = ProfileLoginStatus.Active;
        return _context.ProfileLogins.AnyAsync(
            l => l.UserId == userId && l.AthleteProfileId == profileId && l.Status == active,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetActiveGuardianProfileIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var guardianRoleId = ProfileRole.Guardian.Id;
        const ProfileLoginStatus active = ProfileLoginStatus.Active;

        return await _context.ProfileLogins
            .Where(l => l.UserId == userId && l.RoleId == guardianRoleId && l.Status == active)
            .Select(l => l.AthleteProfileId)
            .ToListAsync(cancellationToken);
    }

    public void Add(ProfileLogin login) => _context.ProfileLogins.Add(login);
}
