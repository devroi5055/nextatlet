namespace NextAtlet.Application.Abstractions.Services;

/// <summary>
/// Sends transactional email. Implemented in Infrastructure (a logging no-op for MVP; a real
/// provider later). Handlers depend on this abstraction, never on a concrete transport. Every link
/// below resolves to the single shared accept endpoint <c>/action-tokens/{tokenId}/accept</c>; the
/// token id is the secure link key and its type selects the action taken on accept.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends an invitation email — the recipient joins a site by following the accept link.</summary>
    Task SendInviteAsync(string email, Guid tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a guardian-consent request (GDPR Art. 8). The guardian authenticates + confirms via the
    /// accept link, which records the consent and lifts the publish gate. Consenting does not join the site.
    /// </summary>
    Task SendConsentRequestAsync(string email, Guid tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an organization email-verification request to a registry-sourced official. Following the
    /// accept link marks the organization Verified.
    /// </summary>
    Task SendOrgVerificationAsync(string email, Guid tokenId, CancellationToken cancellationToken = default);
}
