using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
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

    public void Add(ProfileLogin login) => _context.ProfileLogins.Add(login);
}
