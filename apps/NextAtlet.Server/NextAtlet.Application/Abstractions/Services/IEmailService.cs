namespace NextAtlet.Application.Abstractions.Services;

/// <summary>
/// Sends transactional email. Implemented in Infrastructure (a logging no-op for MVP; a real
/// provider later). Handlers depend on this abstraction, never on a concrete transport.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an invitation email. The recipient claims it at <c>/invitations/{invitationId}/accept</c>
    /// — the invitation id is the link's secure token. An invitation is about <i>joining</i> a profile.
    /// </summary>
    Task SendInviteAsync(string email, Guid invitationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a guardian-consent request (GDPR Art. 8). The guardian authenticates + confirms at
    /// <c>/athletes/{athleteProfileId}/consent</c>, which records the consent and lifts the publish gate.
    /// Distinct from an invitation — consenting does not join the profile.
    /// </summary>
    Task SendConsentRequestAsync(string email, Guid athleteProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a Email verification request to a verified member of a organization
    /// </summary>
    Task SendOrgVerificationAsync(string email, Guid siteId, CancellationToken cancellationToken = default);
}
