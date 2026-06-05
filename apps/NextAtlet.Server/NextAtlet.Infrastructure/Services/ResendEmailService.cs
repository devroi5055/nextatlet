using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.Options;

namespace NextAtlet.Infrastructure.Services;

/// <summary>
/// Sends invitation emails via the Resend API (https://resend.com). Configured as a typed HttpClient
/// (base address + bearer auth) so it needs no third-party SDK. Delivery is best-effort: the invitation
/// row is already committed and is the source of truth, so a send failure is logged, not thrown — it
/// must never turn a successfully-created invitation into a 500.
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly EmailOptions _options;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(HttpClient http, IOptions<EmailOptions> options, ILogger<ResendEmailService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendInviteAsync(string email, Guid invitationId, CancellationToken cancellationToken = default)
    {
        var acceptUrl = $"{_options.AppBaseUrl.TrimEnd('/')}/invitations/{invitationId}/accept";

        var request = new ResendSendRequest(
            From: _options.FromAddress,
            To: [email],
            Subject: "You've been invited to NextAtlet",
            Html: BuildHtml(acceptUrl),
            Text: BuildText(acceptUrl));

        try
        {
            // Web defaults (camelCase) → from/to/subject/html/text, exactly what the Resend API expects.
            using var response = await _http.PostAsJsonAsync("emails", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Resend invite to {Email} (invitation {InvitationId}) failed: {Status} {Body}",
                    email, invitationId, (int)response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            // Best-effort: the invitation is persisted and can be re-sent; never fail the request here.
            _logger.LogError(ex, "Resend invite to {Email} (invitation {InvitationId}) threw", email, invitationId);
        }
    }

    private static string BuildHtml(string acceptUrl) =>
        $"""
        <p>You've been invited to a profile on NextAtlet.</p>
        <p><a href="{acceptUrl}">Click here to accept the invitation</a>.</p>
        <p>If the link doesn't work, copy and paste this address into your browser:<br>{acceptUrl}</p>
        <p>This invitation expires in 7 days. If you weren't expecting it, you can ignore this email.</p>
        """;

    private static string BuildText(string acceptUrl) =>
        $"You've been invited to a profile on NextAtlet.\n\nAccept the invitation: {acceptUrl}\n\nThis invitation expires in 7 days. If you weren't expecting it, you can ignore this email.";

    /// <summary>Resend POST /emails payload. Serialized with web defaults → camelCase field names.</summary>
    private sealed record ResendSendRequest(
        string From,
        string[] To,
        string Subject,
        string Html,
        [property: JsonPropertyName("text")] string Text);
}
