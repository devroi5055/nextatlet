using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class ActionTokenRepository : IActionTokenRepository
{
    private readonly NextAtletDbContext _context;

    public ActionTokenRepository(NextAtletDbContext context) => _context = context;

    public void Add(ActionToken token) => _context.ActionTokens.Add(token);

    public async Task<ActionToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.ActionTokens.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    // The Email/RoleId live inside the jsonb Payload (opaque to LINQ via the value converter). The
    // pending-Invite set is small, so pre-filter on the indexed Type + AcceptedUtc columns in SQL, then
    // match the payload in memory. (A jsonb expression index is the optimization if this ever grows hot.)
    public async Task<bool> HasPendingInviteAsync(Guid siteId, string email, string roleId, CancellationToken cancellationToken = default)
    {
        var pending = await _context.ActionTokens
            .Where(t => t.TypeId == ActionTokenType.Invitation.Id && t.TargetSiteId == siteId && t.AcceptedUtc == null)
            .ToListAsync(cancellationToken);

        return pending.Any(t => t.Payload is InvitePayload p
            && string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase)
            && p.RoleId == roleId);
    }

    public async Task<int> CountPendingInvitesByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var pending = await _context.ActionTokens
            .Where(t => t.TypeId == ActionTokenType.Invitation.Id && t.AcceptedUtc == null)
            .ToListAsync(cancellationToken);

        return pending.Count(t => t.Payload is InvitePayload p
            && string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase));
    }
}
