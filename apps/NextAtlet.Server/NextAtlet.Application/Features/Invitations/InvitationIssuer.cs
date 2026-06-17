using Microsoft.Extensions.Options;
using NextAtlet.Application.Common.Options;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Application.Interfaces.Services;
using NextAtlet.Application.Interfaces.Repositories;

namespace NextAtlet.Application.Features.Invitations;

/// <summary>
/// The single home for issuing invitations — shared by minor self-registration (guardian invite) and
/// the invite endpoint, so neither handler duplicates the create + expiry + notify mechanics. No User
/// row or ProfileLogin is created here; the credential is materialized only on acceptance, so a
/// revoked/expired invite never leaves a dangling login.
/// </summary>
public sealed class InvitationIssuer
{
    private readonly IInvitationRepository _invitations;
    private readonly IEmailService _email;
    private readonly InvitationOptions _options;

    public InvitationIssuer(
        IInvitationRepository invitations,
        IEmailService email,
        IOptions<InvitationOptions> options)
    {
        _invitations = invitations;
        _email = email;
        _options = options.Value;
    }

    /// <summary>Stages a Pending invitation (commit is the caller's via IUnitOfWork). Returns the tracked row.</summary>
    public Invitation Issue(Guid siteId, string email, string roleId, Guid invitedByUserId)
    {
        var invitation = Invitation.Issue(
            siteId, email, roleId, invitedByUserId,
            expiresUtc: DateTime.UtcNow.AddDays(_options.ExpiryDays));

        _invitations.Add(invitation);
        return invitation;
    }

    /// <summary>Sends the invite email. Call AFTER the invitation is durably committed.</summary>
    public Task NotifyAsync(Invitation invitation, CancellationToken cancellationToken = default)
        => _email.SendInviteAsync(invitation.Email, invitation.Id, cancellationToken);
}
