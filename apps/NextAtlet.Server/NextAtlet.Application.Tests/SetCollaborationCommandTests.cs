using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Invitations.Commands;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// collaboration: only the controller may toggle shared editing; it flips only the controlling side's
/// shared flag and never changes who controls. A shared-mode non-controller resolves to EditOnly.
/// </summary>
public class SetCollaborationCommandTests
{
    private const string GuardianSub = TestApp.OwnerAuthProviderId;
    private const string GuardianEmail = TestApp.OwnerEmail;
    private const string AthleteSub = "athlete-sub";
    private const string AthleteEmail = "athlete@test.local";

    private static readonly DateTime YoungMinorDob = DateTime.UtcNow.AddYears(-14);
    private static readonly DateTime AdultDob = new(1995, 1, 1);

    private static Task<ControlMode> ControlModeAsync(TestApp app, Guid profileId)
        => app.QueryAsync(c => c.AthleteProfiles.Where(p => p.Id == profileId).Select(p => p.ControlMode).SingleAsync());

    private static async Task<Guid> GuardianControlledWithOwnerLoginAsync(TestApp app)
    {
        var child = await app.Send(new GuardianRegisterAthleteCommand(
            GuardianSub, GuardianEmail, "Kid", "kid", YoungMinorDob, Locale.Da.Id));
        await app.Send(new InviteToProfileCommand(
            child.Id, GuardianSub, GuardianEmail, AthleteEmail, ProfileRole.AthleteOwner.Id));
        var inviteId = (await app.QueryAsync(c => c.Invitations.SingleAsync(i => i.Email == AthleteEmail))).Id;
        await app.Send(new AcceptInvitationCommand(inviteId, AthleteSub, AthleteEmail));
        return child.Id;
    }

    [Fact]
    public async Task Controller_can_enable_then_disable_shared_editing()
    {
        using var app = new TestApp();
        var profile = await app.Send(new SelfRegisterAthleteCommand(
            GuardianSub, GuardianEmail, "Anna", "anna", AdultDob, Locale.Da.Id)); // AthleteControlled

        await app.Send(new SetCollaborationCommand(profile.Id, GuardianSub, SharedEditing: true));
        Assert.Equal(ControlMode.AthleteControlledShared, await ControlModeAsync(app, profile.Id));

        await app.Send(new SetCollaborationCommand(profile.Id, GuardianSub, SharedEditing: false));
        Assert.Equal(ControlMode.AthleteControlled, await ControlModeAsync(app, profile.Id));
    }

    [Fact]
    public async Task Non_controller_cannot_toggle_collaboration()
    {
        using var app = new TestApp();
        var profileId = await GuardianControlledWithOwnerLoginAsync(app); // GuardianControlled; athlete is ReadOnly

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new SetCollaborationCommand(profileId, AthleteSub, SharedEditing: true)));
        Assert.Equal(ErrorCodes.NotAuthorized, ex.ErrorCode);
    }

    [Fact]
    public async Task Enabling_shared_gives_the_non_controller_edit_only()
    {
        using var app = new TestApp();
        var profileId = await GuardianControlledWithOwnerLoginAsync(app);

        // the guardian (controller) opens up editing
        await app.Send(new SetCollaborationCommand(profileId, GuardianSub, SharedEditing: true));
        Assert.Equal(ControlMode.GuardianControlledShared, await ControlModeAsync(app, profileId));

        // the athlete (non-controller) now resolves to EditOnly: edit draft + media, no publish/approve/transfer
        var profile = await app.QueryAsync(c => c.AthleteProfiles.SingleAsync(p => p.Id == profileId));
        var athleteLogin = await app.QueryAsync(c => c.ProfileLogins.SingleAsync(
            l => l.AthleteProfileId == profileId && l.RoleId == ProfileRole.AthleteOwner.Id));

        var perms = new PermissionResolver().Resolve(athleteLogin, profile);
        Assert.Equal(ProfilePermissions.EditOnly, perms);
        Assert.True(perms.CanEditContent);
        Assert.False(perms.CanPublish);
    }
}
