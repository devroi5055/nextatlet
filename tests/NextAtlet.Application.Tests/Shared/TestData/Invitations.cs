using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="Invitation"/> via the entity's <c>Issue</c> factory + lifecycle
/// methods. Covers the four states the accept flow must distinguish.
/// </summary>
public static class Invitations
{
    private const string DefaultEmail = "invitee@test.local";

    public static Invitation APendingInvitation(
        string? roleId = null,
        string? email = null,
        Guid? targetProfileId = null,
        Guid? invitedByUserId = null,
        DateTime? nowUtc = null)
        => Invitation.Issue(
            targetProfileId ?? Guid.NewGuid(),
            email ?? DefaultEmail,
            roleId ?? ProfileRole.Guardian.Id,
            invitedByUserId ?? Guid.NewGuid(),
            expiresUtc: (nowUtc ?? DateTime.UtcNow).AddDays(7));

    public static Invitation AnExpiredInvitation(string? roleId = null, string? email = null)
        => Invitation.Issue(
            Guid.NewGuid(),
            email ?? DefaultEmail,
            roleId ?? ProfileRole.Guardian.Id,
            Guid.NewGuid(),
            expiresUtc: DateTime.UtcNow.AddDays(-1)); // past, still Pending → IsExpired

    public static Invitation AnAcceptedInvitation(string? roleId = null, string? email = null)
    {
        var invitation = APendingInvitation(roleId, email);
        invitation.Accept();
        return invitation;
    }

    public static Invitation ARevokedInvitation(string? roleId = null, string? email = null)
    {
        var invitation = APendingInvitation(roleId, email);
        invitation.Status = InvitationStatus.Revoked;
        return invitation;
    }
}
