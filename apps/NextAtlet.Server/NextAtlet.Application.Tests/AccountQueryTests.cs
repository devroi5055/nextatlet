using NextAtlet.Application.Features.Account.Queries;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Domain.Enumerations;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Tests for the /me domain-gate query (GetCurrentUserQuery) via the MediatR pipeline.
/// AuthProviderId + Email are passed in (as the controller would from the validated token).
/// </summary>
public class AccountQueryTests
{
    private const string GuardianEmail = "guardian@test.local";
    private const string GuardianSub = "guardian-sub";

    [Fact]
    public async Task Me_reports_unregistered_when_caller_has_no_user()
    {
        using var app = new TestApp();

        var me = await app.Send(new GetCurrentUserQuery(TestApp.OwnerAuthProviderId, TestApp.OwnerEmail));

        Assert.False(me.Registered);
        Assert.Null(me.Role);
        Assert.Equal(0, me.PendingGuardianInvites);
    }

    [Fact]
    public async Task Me_reports_owner_after_self_registration()
    {
        using var app = new TestApp();
        await app.Send(new SelfRegisterAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Anna", "anna", new DateTime(1995, 1, 1), Locale.Da.Id));

        var me = await app.Send(new GetCurrentUserQuery(TestApp.OwnerAuthProviderId, TestApp.OwnerEmail));

        Assert.True(me.Registered);
        Assert.Equal(ProfileRole.AthleteOwner.Id, me.Role);
        Assert.Equal(0, me.PendingGuardianInvites);
        // The self-registered adult controls their own AthleteControlled profile.
        Assert.Equal(NextAtlet.Domain.Enumerations.Enums.AthleteProfile.ControlMode.AthleteControlled, me.ControlMode);
        Assert.True(me.IsInControl);
        Assert.True(me.CanEdit);
    }

    [Fact]
    public async Task Me_reports_guardian_after_registering_a_child()
    {
        using var app = new TestApp();
        await app.Send(new GuardianRegisterAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Kid", "kid", DateTime.UtcNow.AddYears(-9), Locale.Da.Id));

        var me = await app.Send(new GetCurrentUserQuery(TestApp.OwnerAuthProviderId, TestApp.OwnerEmail));

        // owns no profile of their own, but is an (active) guardian — nothing pending to accept
        Assert.False(me.Registered);
        Assert.Equal(ProfileRole.Guardian.Id, me.Role);
        Assert.Equal(0, me.PendingGuardianInvites);
    }

    [Fact]
    public async Task Me_surfaces_pending_invite_for_an_invited_guardian()
    {
        using var app = new TestApp();
        // a 14-year-old self-registers and names a guardian by email
        await app.Send(new SelfRegisterAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Kid", "kid", DateTime.UtcNow.AddYears(-14), Locale.Da.Id,
            GuardianEmail: GuardianEmail, ParentalConsentConfirmed: true));

        // the guardian logs in (new sub, matching email) and checks /me before accepting
        var me = await app.Send(new GetCurrentUserQuery(GuardianSub, GuardianEmail));

        Assert.False(me.Registered);
        Assert.Equal(ProfileRole.Guardian.Id, me.Role);
        Assert.Equal(1, me.PendingGuardianInvites);
    }
}
