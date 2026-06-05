using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Invitations.Commands;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// POST /athletes/{id}/invite — invite a person to an existing profile. Role in body; only a caller
/// with an active login on the profile may invite; no double-pending.
/// </summary>
public class InviteToProfileCommandTests
{
    private static readonly DateTime ChildDob = DateTime.UtcNow.AddYears(-9);
    private static readonly DateTime AdultDob = new(1995, 1, 1);
    private const string SecondGuardianEmail = "second-guardian@test.local";

    /// <summary>Guardian registers a child and ends up with an active guardian login on the minor profile.</summary>
    private static Task<Common.DTOs.AthleteProfileDto> RegisterChildAsync(TestApp app) =>
        app.Send(new GuardianRegisterAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Kid", "kid", ChildDob, Locale.Da.Id));

    [Fact]
    public async Task Active_login_holder_can_invite_a_second_guardian()
    {
        using var app = new TestApp();
        var child = await RegisterChildAsync(app);

        var dto = await app.Send(new InviteToProfileCommand(
            child.Id, TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, SecondGuardianEmail, ProfileRole.Guardian.Id));

        Assert.Equal(SecondGuardianEmail, dto.Email);
        Assert.Equal(ProfileRole.Guardian.Id, dto.Role);

        var invite = await app.QueryAsync(c => c.Invitations.SingleAsync(i => i.Email == SecondGuardianEmail));
        Assert.Equal(child.Id, invite.TargetProfileId);
        Assert.Equal(InvitationStatus.Pending, invite.Status);
    }

    [Fact]
    public async Task Caller_without_a_login_on_the_profile_is_not_authorized()
    {
        using var app = new TestApp();
        var child = await RegisterChildAsync(app);

        // A caller with no presence/login on this profile cannot invite to it.
        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(new InviteToProfileCommand(
            child.Id, "stranger-sub", "stranger@test.local", SecondGuardianEmail, ProfileRole.Guardian.Id)));
        Assert.Equal(ErrorCodes.NotAuthorized, ex.ErrorCode);
    }

    [Fact]
    public async Task Double_invite_of_same_email_and_role_is_rejected()
    {
        using var app = new TestApp();
        var child = await RegisterChildAsync(app);
        await app.Send(new InviteToProfileCommand(
            child.Id, TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, SecondGuardianEmail, ProfileRole.Guardian.Id));

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(new InviteToProfileCommand(
            child.Id, TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, SecondGuardianEmail, ProfileRole.Guardian.Id)));
        Assert.Equal(ErrorCodes.InvitationAlreadyPending, ex.ErrorCode);
    }

    [Fact]
    public async Task Unknown_role_is_rejected()
    {
        using var app = new TestApp();
        var child = await RegisterChildAsync(app);

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(new InviteToProfileCommand(
            child.Id, TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, SecondGuardianEmail, "supervisor")));
        Assert.Equal(ErrorCodes.InvitationRoleInvalid, ex.ErrorCode);
    }

    [Fact]
    public async Task Inviting_a_guardian_onto_an_adult_profile_is_rejected()
    {
        using var app = new TestApp();
        // Adult self-registers — active owner login, but no guardian belongs on an adult profile.
        var adult = await app.Send(new SelfRegisterAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Anna", "anna", AdultDob, Locale.Da.Id));

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(new InviteToProfileCommand(
            adult.Id, TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, SecondGuardianEmail, ProfileRole.Guardian.Id)));
        Assert.Equal(ErrorCodes.GuardianCannotRegisterAdult, ex.ErrorCode);
    }
}
