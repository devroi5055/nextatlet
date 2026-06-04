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

    public Task<bool> HasGuardianLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var guardianRoleId = ProfileRole.Guardian.Id;
        return _context.ProfileLogins.AnyAsync(l => l.UserId == userId && l.RoleId == guardianRoleId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfileLogin>> GetPendingGuardianLoginsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var guardianRoleId = ProfileRole.Guardian.Id;
        const ProfileLoginStatus pending = ProfileLoginStatus.Pending;

        return await _context.ProfileLogins
            .Where(l => l.UserId == userId && l.RoleId == guardianRoleId && l.Status == pending)
            .ToListAsync(cancellationToken);
    }

    public void Add(ProfileLogin login) => _context.ProfileLogins.Add(login);
}
