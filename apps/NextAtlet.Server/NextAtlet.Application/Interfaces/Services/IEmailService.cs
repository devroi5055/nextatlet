namespace NextAtlet.Application.Abstractions.Services;

/// <summary>
/// Sends transactional email. Implemented in Infrastructure (a logging no-op for MVP; a real
/// provider later). Handlers depend on this abstraction, never on a concrete transport.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an invitation email. The recipient claims it at <c>/invitations/{invitationId}/accept</c>
    /// — the invitation id is the link's secure token.
    /// </summary>
    Task SendInviteAsync(string email, Guid invitationId, CancellationToken cancellationToken = default);
}
