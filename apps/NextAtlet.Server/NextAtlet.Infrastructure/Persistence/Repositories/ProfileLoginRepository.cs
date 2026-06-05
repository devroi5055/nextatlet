using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class ProfileLoginRepository : IProfileLoginRepository
{
    private const ProfileLoginStatus Active = ProfileLoginStatus.Active;

    private readonly NextAtletDbContext _context;

    public ProfileLoginRepository(NextAtletDbContext context) => _context = context;

    public Task<ProfileLogin?> GetActiveLoginAsync(Guid userId, Guid profileId, CancellationToken cancellationToken = default)
        => _context.ProfileLogins.FirstOrDefaultAsync(
            l => l.UserId == userId && l.AthleteProfileId == profileId && l.Status == Active,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetActiveGuardianProfileIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var guardianRoleId = ProfileRole.Guardian.Id;
        return await _context.ProfileLogins
            .Where(l => l.UserId == userId && l.RoleId == guardianRoleId && l.Status == Active)
            .Select(l => l.AthleteProfileId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasActiveOwnerLoginAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var ownerRoleId = ProfileRole.AthleteOwner.Id;
        return _context.ProfileLogins.AnyAsync(
            l => l.AthleteProfileId == profileId && l.RoleId == ownerRoleId && l.Status == Active,
            cancellationToken);
    }

    public Task<bool> HasActiveGuardianLoginAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var guardianRoleId = ProfileRole.Guardian.Id;
        return _context.ProfileLogins.AnyAsync(
            l => l.AthleteProfileId == profileId && l.RoleId == guardianRoleId && l.Status == Active,
            cancellationToken);
    }

    public void Add(ProfileLogin login) => _context.ProfileLogins.Add(login);
}
