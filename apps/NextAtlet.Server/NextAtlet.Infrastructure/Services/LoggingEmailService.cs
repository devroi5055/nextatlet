using Microsoft.Extensions.Logging;
using NextAtlet.Application.Abstractions.Services;

namespace NextAtlet.Infrastructure.Services;

/// <summary>
/// MVP email transport: logs the invite link instead of sending real mail. Swap for a real provider
/// (SendGrid/SES/etc.) behind the same <see cref="IEmailService"/> contract — no handler changes.
/// </summary>
public class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;

    public LoggingEmailService(ILogger<LoggingEmailService> logger) => _logger = logger;

    public Task SendInviteAsync(string email, Guid invitationId, CancellationToken cancellationToken = default)
    {
        // The invitation id is the secure token in the accept URL.
        _logger.LogInformation(
            "Invitation email to {Email}: accept at /invitations/{InvitationId}/accept", email, invitationId);
        return Task.CompletedTask;
    }
}
