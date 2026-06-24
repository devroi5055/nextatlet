using NextAtlet.Domain.Entities.Identity;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IActionTokenRepository
{
    void Add(ActionToken token);

    /// <summary>Lookup by the row Id used as the accept-URL token.</summary>
    Task<ActionToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>True if a pending Invite token already exists for this site + email + role (anti double-send).</summary>
    Task<bool> HasPendingInviteAsync(Guid siteId, string email, string roleId, CancellationToken cancellationToken = default);

    /// <summary>Count of pending Invite tokens addressed to this email — surfaces "accept" prompts in /me.</summary>
    Task<int> CountPendingInvitesByEmailAsync(string email, CancellationToken cancellationToken = default);
}
