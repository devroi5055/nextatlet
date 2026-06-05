using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Invitations.Commands;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// The invited person claims their login via /invitations/{id}/accept. Role-agnostic: the invitation
/// Id carries who, which profile, and what role. Covers the §8.9 accept matrix.
/// </summary>
public class AcceptInvitationCommandTests
{
    private const string GuardianEmail = "guardian@test.local";
    private const string GuardianSub = "guardian-sub";
    private static readonly DateTime YoungMinorDob = DateTime.UtcNow.AddYears(-14);

    /// <summary>Minor self-registration issues a guardian Invitation; returns its Id (the accept token).</summary>
    private static async Task<Guid> IssueGuardianInviteAsync(TestApp app)
    {
        await app.Send(new SelfRegisterAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Kid", "kid", YoungMinorDob, Locale.Da.Id,
            GuardianEmail: GuardianEmail, ParentalConsentConfirmed: true));

        var invite = await app.QueryAsync(c => c.Invitations.SingleAsync(i => i.Email == GuardianEmail));
        return invite.Id;
    }

    [Fact]
    public async Task Accept_with_valid_id_materializes_an_active_guardian_login()
    {
        using var app = new TestApp();
        var invitationId = await IssueGuardianInviteAsync(app);

        var result = await app.Send(new AcceptInvitationCommand(invitationId, GuardianSub, GuardianEmail));

        Assert.Equal(ProfileRole.Guardian.Id, result.Role);

        // The user is created/claimed with the guardian's subject.
        var guardian = await app.QueryAsync(c => c.Users.SingleAsync(u => u.Email == GuardianEmail));
        Assert.Equal(GuardianSub, guardian.AuthProviderId);
        Assert.True(guardian.IsClaimed);

        // The guardian ProfileLogin is materialized AT ACCEPT TIME and is Active.
        var login = await app.QueryAsync(c => c.ProfileLogins.SingleAsync(l =>
            l.UserId == guardian.Id && l.RoleId == ProfileRole.Guardian.Id));
        Assert.Equal(ProfileLoginStatus.Active, login.Status);

        // The invitation is now Accepted with an audit timestamp.
        var invite = await app.QueryAsync(c => c.Invitations.SingleAsync(i => i.Id == invitationId));
        Assert.Equal(InvitationStatus.Accepted, invite.Status);
        Assert.NotNull(invite.AcceptedUtc);
    }

    [Fact]
    public async Task Accept_with_unknown_id_is_rejected()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new AcceptInvitationCommand(Guid.NewGuid(), GuardianSub, GuardianEmail)));
        Assert.Equal(ErrorCodes.InvitationNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task Accept_with_wrong_email_is_rejected()
    {
        using var app = new TestApp();
        var invitationId = await IssueGuardianInviteAsync(app);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new AcceptInvitationCommand(invitationId, "intruder-sub", "intruder@test.local")));
        Assert.Equal(ErrorCodes.InvitationEmailMismatch, ex.ErrorCode);
    }

    [Fact]
    public async Task Accept_is_case_insensitive_on_email()
    {
        using var app = new TestApp();
        var invitationId = await IssueGuardianInviteAsync(app);

        var result = await app.Send(new AcceptInvitationCommand(invitationId, GuardianSub, GuardianEmail.ToUpperInvariant()));

        Assert.Equal(ProfileRole.Guardian.Id, result.Role);
    }

    [Fact]
    public async Task Accept_already_used_invitation_is_rejected()
    {
        using var app = new TestApp();
        var invitationId = await IssueGuardianInviteAsync(app);
        await app.Send(new AcceptInvitationCommand(invitationId, GuardianSub, GuardianEmail));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new AcceptInvitationCommand(invitationId, GuardianSub, GuardianEmail)));
        Assert.Equal(ErrorCodes.InvitationAlreadyUsed, ex.ErrorCode);
    }

    [Fact]
    public async Task Accept_expired_invitation_is_rejected()
    {
        using var app = new TestApp();
        var invitationId = await IssueGuardianInviteAsync(app);

        // Force the invitation past its expiry window (check-on-use; no background sweeper for MVP).
        await app.QueryAsync(async c =>
        {
            var invite = await c.Invitations.SingleAsync(i => i.Id == invitationId);
            invite.ExpiresUtc = DateTime.UtcNow.AddDays(-1);
            await c.SaveChangesAsync();
            return invite.Id;
        });

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new AcceptInvitationCommand(invitationId, GuardianSub, GuardianEmail)));
        Assert.Equal(ErrorCodes.InvitationExpired, ex.ErrorCode);
    }
}
