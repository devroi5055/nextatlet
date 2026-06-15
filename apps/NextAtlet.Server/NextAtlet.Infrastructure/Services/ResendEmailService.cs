using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;

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

    public Task SendInviteAsync(string email, Guid invitationId, CancellationToken cancellationToken = default)
    {
        var acceptUrl = $"{_options.AppBaseUrl.TrimEnd('/')}/invitations/{invitationId}/accept";
        return SendAsync(
            email,
            subject: "You've been invited to NextAtlet",
            html: $"""
                <p>You've been invited to a profile on NextAtlet.</p>
                <p><a href="{acceptUrl}">Click here to accept the invitation</a>.</p>
                <p>If the link doesn't work, copy and paste this address into your browser:<br>{acceptUrl}</p>
                <p>This invitation expires in 7 days. If you weren't expecting it, you can ignore this email.</p>
                """,
            text: $"You've been invited to a profile on NextAtlet.\n\nAccept the invitation: {acceptUrl}\n\nThis invitation expires in 7 days. If you weren't expecting it, you can ignore this email.",
            context: $"invite (invitation {invitationId})",
            cancellationToken);
    }

    public Task SendConsentRequestAsync(string email, Guid athleteProfileId, CancellationToken cancellationToken = default)
    {
        var consentUrl = $"{_options.AppBaseUrl.TrimEnd('/')}/athletes/{athleteProfileId}/consent";
        return SendAsync(
            email,
            subject: "Approve your child's NextAtlet profile",
            html: $"""
                <p>A NextAtlet profile has been created for a child in your care. As their guardian, your
                approval is required before the profile can be made public.</p>
                <p><a href="{consentUrl}">Click here to review and approve</a>.</p>
                <p>If the link doesn't work, copy and paste this address into your browser:<br>{consentUrl}</p>
                <p>If you weren't expecting this, you can ignore this email — the profile stays private.</p>
                """,
            text: $"A NextAtlet profile has been created for a child in your care. Your approval is required before it can be made public.\n\nReview and approve: {consentUrl}\n\nIf you weren't expecting this, you can ignore this email.",
            context: $"consent (profile {athleteProfileId})",
            cancellationToken);
    }

    private async Task SendAsync(string email, string subject, string html, string text, string context, CancellationToken cancellationToken)
    {
        var request = new ResendSendRequest(From: _options.FromAddress, To: [email], Subject: subject, Html: html, Text: text);
        try
        {
            // Web defaults (camelCase) → from/to/subject/html/text, exactly what the Resend API expects.
            using var response = await _http.PostAsJsonAsync("emails", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Resend {Context} to {Email} failed: {Status} {Body}", context, email, (int)response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            // Best-effort: the originating row is committed and is the source of truth; never fail the request here.
            _logger.LogError(ex, "Resend {Context} to {Email} threw", context, email);
        }
    }

    /// <summary>Resend POST /emails payload. Serialized with web defaults → camelCase field names.</summary>
    private sealed record ResendSendRequest(
        string From,
        string[] To,
        string Subject,
        string Html,
        [property: JsonPropertyName("text")] string Text);
}
