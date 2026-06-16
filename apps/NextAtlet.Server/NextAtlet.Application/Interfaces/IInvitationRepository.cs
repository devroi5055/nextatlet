using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IInvitationRepository
{
    void Add(Invitation invitation);

    /// <summary>Lookup by the row Id used as the accept-URL token.</summary>
    Task<Invitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>True if a Pending invitation already exists for this profile + email + role (anti double-send).</summary>
    Task<bool> HasPendingAsync(Guid siteId, string email, string roleId, CancellationToken cancellationToken = default);

    /// <summary>Count of Pending invitations addressed to this email — surfaces "accept" prompts in /me.</summary>
    Task<int> CountPendingByEmailAsync(string email, CancellationToken cancellationToken = default);
}
