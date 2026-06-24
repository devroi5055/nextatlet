using Microsoft.Extensions.Logging;
using NextAtlet.Application.Abstractions.Services;

namespace NextAtlet.Infrastructure.Services;

/// <summary>
/// MVP email transport: logs the accept link instead of sending real mail. Swap for a real provider
/// (SendGrid/SES/etc.) behind the same <see cref="IEmailService"/> contract — no handler changes.
/// Every flow links to the shared <c>/action-tokens/{tokenId}/accept</c> endpoint.
/// </summary>
public class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;

    public LoggingEmailService(ILogger<LoggingEmailService> logger) => _logger = logger;

    public Task SendInviteAsync(string email, Guid tokenId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Invitation email to {Email}: accept at /action-tokens/{TokenId}/accept", email, tokenId);
        return Task.CompletedTask;
    }

    public Task SendConsentRequestAsync(string email, Guid tokenId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Guardian-consent email to {Email}: confirm at /action-tokens/{TokenId}/accept", email, tokenId);
        return Task.CompletedTask;
    }

    public Task SendOrgVerificationAsync(string email, Guid tokenId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Org-verification email to {Email}: verify at /action-tokens/{TokenId}/accept", email, tokenId);
        return Task.CompletedTask;
    }
}
