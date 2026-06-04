using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Account.Commands;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// The invited guardian claims their account and activates their pending guardian login(s).
/// </summary>
public class AcceptGuardianInviteCommandTests
{
    private const string GuardianEmail = "guardian@test.local";
    private const string GuardianSub = "guardian-sub";
    private static readonly DateTime MinorDob = DateTime.UtcNow.AddYears(-10);
    private static readonly DateTime AdultDob = new(1995, 1, 1);

    private static Task InviteGuardianViaMinorRegistrationAsync(TestApp app) =>
        app.Send(new RegisterOwnAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Kid", "kid", MinorDob, Locale.Da.Id,
            GuardianEmail: GuardianEmail));

    [Fact]
    public async Task Accepting_claims_the_user_and_activates_the_guardian_login()
    {
        using var app = new TestApp();
        await InviteGuardianViaMinorRegistrationAsync(app);

        var result = await app.Send(new AcceptGuardianInviteCommand(GuardianSub, GuardianEmail));

        Assert.Equal(1, result.Accepted);

        // the invited user is now claimed with the guardian's subject
        var guardian = await app.QueryAsync(c => c.Users.SingleAsync(u => u.Email == GuardianEmail));
        Assert.Equal(GuardianSub, guardian.AuthProviderId);
        Assert.True(guardian.IsClaimed);

        // their guardian login is now Active
        var login = await app.QueryAsync(c => c.ProfileLogins.SingleAsync(l =>
            l.UserId == guardian.Id && l.RoleId == ProfileRole.Guardian.Id));
        Assert.Equal(ProfileLoginStatus.Active, login.Status);
    }

    [Fact]
    public async Task Accepting_with_no_invite_is_rejected()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new AcceptGuardianInviteCommand("nobody-sub", "nobody@test.local")));
        Assert.Equal(ErrorCodes.GuardianInviteNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task Invited_guardian_can_self_register_without_email_collision()
    {
        using var app = new TestApp();
        await InviteGuardianViaMinorRegistrationAsync(app); // creates an unclaimed user for GuardianEmail

        // the same person (matching email, real sub) now self-registers their own profile
        var dto = await app.Send(new RegisterOwnAthleteCommand(
            GuardianSub, GuardianEmail, "Parent Athlete", "parent-athlete", AdultDob, Locale.Da.Id));

        Assert.Equal("parent-athlete", dto.Slug);

        // no duplicate user — the invited row was linked (claimed) to the subject
        var users = await app.QueryAsync(c => c.Users.CountAsync(u => u.Email == GuardianEmail));
        Assert.Equal(1, users);
        var guardian = await app.QueryAsync(c => c.Users.SingleAsync(u => u.Email == GuardianEmail));
        Assert.Equal(GuardianSub, guardian.AuthProviderId);
    }
}
