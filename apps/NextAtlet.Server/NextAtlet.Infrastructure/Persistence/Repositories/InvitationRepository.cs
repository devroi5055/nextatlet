using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly NextAtletDbContext _context;

    public InvitationRepository(NextAtletDbContext context) => _context = context;

    public void Add(Invitation invitation) => _context.Invitations.Add(invitation);

    public async Task<Invitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Invitations.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<bool> HasPendingAsync(Guid profileId, string email, string roleId, CancellationToken cancellationToken = default)
    {
        var pending = InvitationStatus.Pending.Id;
        return _context.Invitations.AnyAsync(
            i => i.TargetProfileId == profileId && i.Email == email && i.RoleId == roleId && i.StatusId == pending,
            cancellationToken);
    }

    public Task<int> CountPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var pending = InvitationStatus.Pending.Id;
        return _context.Invitations.CountAsync(i => i.Email == email && i.StatusId == pending, cancellationToken);
    }
}
