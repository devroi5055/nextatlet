using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="ActionToken"/> via the entity's <c>Issue</c> factory + lifecycle
/// methods. Replaces the old per-state Invitation builders: an action token now unifies the three
/// link-bearing flows (invite / consent / org-verification) and its state is just AcceptedUtc/ExpiresUtc.
/// </summary>
public static class ActionTokens
{
    private const string DefaultEmail = "invitee@test.local";

    public static ActionToken APendingInviteToken(
        string? roleId = null,
        string? email = null,
        Guid? targetSiteId = null,
        DateTime? nowUtc = null)
        => ActionToken.Issue(
            ActionTokenType.Invitation.Id,
            targetSiteId ?? Guid.NewGuid(),
            new InvitePayload { Email = email ?? DefaultEmail, RoleId = roleId ?? IndividualRole.Guardian.Id },
            expiresUtc: (nowUtc ?? DateTime.UtcNow).AddDays(7));

    public static ActionToken AnExpiredInviteToken(string? roleId = null, string? email = null)
        => ActionToken.Issue(
            ActionTokenType.Invitation.Id,
            Guid.NewGuid(),
            new InvitePayload { Email = email ?? DefaultEmail, RoleId = roleId ?? IndividualRole.Guardian.Id },
            expiresUtc: DateTime.UtcNow.AddDays(-1)); // past, still pending → IsExpired

    public static ActionToken AnAcceptedInviteToken(string? roleId = null, string? email = null)
    {
        var token = APendingInviteToken(roleId, email);
        token.Accept(DateTime.UtcNow);
        return token;
    }

    public static ActionToken AConsentToken(
        string? email = null,
        string termsVersion = "2026-01",
        Guid? targetSiteId = null)
        => ActionToken.Issue(
            ActionTokenType.Consent.Id,
            targetSiteId ?? Guid.NewGuid(),
            new ConsentPayload { Email = email ?? "guardian@test.local", TermsVersion = termsVersion },
            expiresUtc: DateTime.UtcNow.AddDays(7));

    public static ActionToken AnOrgEmailVerificationToken(
        Guid? clubOfficialId = null,
        Guid? userId = null,
        string? email = null,
        Guid? targetSiteId = null)
        => ActionToken.Issue(
            ActionTokenType.OrgEmailVerification.Id,
            targetSiteId ?? Guid.NewGuid(),
            new OrgEmailVerificationPayload
            {
                ClubOfficialId = clubOfficialId ?? Guid.NewGuid(),
                UserId = userId,
                Email = email ?? "official@club.dk"
            },
            expiresUtc: DateTime.UtcNow.AddDays(7));
}
