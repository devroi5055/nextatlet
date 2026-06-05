using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Invitations.Commands;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// transfer-control: only the current controller may initiate; guardian→athlete is age-gated and needs
/// an athlete login; athlete→guardian needs a guardian login; the receiving side is reset to non-shared.
/// </summary>
public class TransferControlCommandTests
{
    private const string GuardianSub = TestApp.OwnerAuthProviderId;
    private const string GuardianEmail = TestApp.OwnerEmail;
    private const string AthleteSub = "athlete-sub";
    private const string AthleteEmail = "athlete@test.local";

    private static readonly DateTime YoungMinorDob = DateTime.UtcNow.AddYears(-14);
    private static readonly DateTime BelowMinDob = DateTime.UtcNow.AddYears(-10);
    private static readonly DateTime AdultDob = new(1995, 1, 1);

    /// <summary>Guardian-controlled child (guardian is the caller) WITH an athlete owner login attached.</summary>
    private static async Task<Guid> GuardianControlledWithOwnerLoginAsync(TestApp app, DateTime childDob)
    {
        var child = await app.Send(new GuardianRegisterAthleteCommand(
            GuardianSub, GuardianEmail, "Kid", "kid", childDob, Locale.Da.Id));

        await app.Send(new InviteToProfileCommand(
            child.Id, GuardianSub, GuardianEmail, AthleteEmail, ProfileRole.AthleteOwner.Id));
        var inviteId = (await app.QueryAsync(c => c.Invitations.SingleAsync(i => i.Email == AthleteEmail))).Id;
        await app.Send(new AcceptInvitationCommand(inviteId, AthleteSub, AthleteEmail));

        return child.Id;
    }

    private static Task<ControlMode> ControlModeAsync(TestApp app, Guid profileId)
        => app.QueryAsync(c => c.AthleteProfiles.Where(p => p.Id == profileId).Select(p => p.ControlMode).SingleAsync());

    [Fact]
    public async Task Guardian_can_transfer_control_to_a_13_plus_athlete()
    {
        using var app = new TestApp();
        var profileId = await GuardianControlledWithOwnerLoginAsync(app, YoungMinorDob);

        await app.Send(new TransferControlCommand(profileId, GuardianSub, "athlete"));

        Assert.Equal(ControlMode.AthleteControlled, await ControlModeAsync(app, profileId));
    }

    [Fact]
    public async Task Athlete_can_transfer_control_to_guardian()
    {
        using var app = new TestApp();
        var profileId = await GuardianControlledWithOwnerLoginAsync(app, YoungMinorDob);
        await app.Send(new TransferControlCommand(profileId, GuardianSub, "athlete")); // now AthleteControlled

        // the athlete (now controller) hands control to the guardian
        await app.Send(new TransferControlCommand(profileId, AthleteSub, "guardian"));

        Assert.Equal(ControlMode.GuardianControlled, await ControlModeAsync(app, profileId));
    }

    [Fact]
    public async Task Non_controller_cannot_transfer()
    {
        using var app = new TestApp();
        var profileId = await GuardianControlledWithOwnerLoginAsync(app, YoungMinorDob); // GuardianControlled

        // the athlete owner is ReadOnly here — they cannot grab control
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new TransferControlCommand(profileId, AthleteSub, "athlete")));
        Assert.Equal(ErrorCodes.NotAuthorized, ex.ErrorCode);
    }

    [Fact]
    public async Task Transfer_to_athlete_under_13_is_rejected()
    {
        using var app = new TestApp();
        var child = await app.Send(new GuardianRegisterAthleteCommand(
            GuardianSub, GuardianEmail, "Tiny", "tiny", BelowMinDob, Locale.Da.Id));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new TransferControlCommand(child.Id, GuardianSub, "athlete")));
        Assert.Equal(ErrorCodes.AthleteTooYoungForControl, ex.ErrorCode);
    }

    [Fact]
    public async Task Transfer_to_athlete_without_an_owner_login_is_rejected()
    {
        using var app = new TestApp();
        // guardian-register leaves no athlete owner login
        var child = await app.Send(new GuardianRegisterAthleteCommand(
            GuardianSub, GuardianEmail, "Kid", "kid", YoungMinorDob, Locale.Da.Id));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new TransferControlCommand(child.Id, GuardianSub, "athlete")));
        Assert.Equal(ErrorCodes.NoAthleteLoginExists, ex.ErrorCode);
    }

    [Fact]
    public async Task Transfer_to_guardian_without_a_guardian_login_is_rejected()
    {
        using var app = new TestApp();
        var profile = await app.Send(new SelfRegisterAthleteCommand(
            GuardianSub, GuardianEmail, "Anna", "anna", AdultDob, Locale.Da.Id)); // AthleteControlled, owner only

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new TransferControlCommand(profile.Id, GuardianSub, "guardian")));
        Assert.Equal(ErrorCodes.NoGuardianLoginExists, ex.ErrorCode);
    }

    [Fact]
    public async Task Invalid_transfer_target_is_rejected()
    {
        using var app = new TestApp();
        var profile = await app.Send(new SelfRegisterAthleteCommand(
            GuardianSub, GuardianEmail, "Anna", "anna", AdultDob, Locale.Da.Id));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new TransferControlCommand(profile.Id, GuardianSub, "sibling")));
        Assert.Equal(ErrorCodes.TransferTargetInvalid, ex.ErrorCode);
    }
}
