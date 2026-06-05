using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.ValueObjects.Sections;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Self-registration: the caller becomes the AthleteOwner and the profile is always AthleteControlled.
/// Under-13 is rejected; 13–17 needs a guardian invite; 13–15 also needs parental consent.
/// </summary>
public class SelfRegisterAthleteCommandTests
{
    private static readonly DateTime AdultDob = new(1995, 1, 1);
    private static readonly DateTime OlderMinorDob = DateTime.UtcNow.AddYears(-17); // 16–17
    private static readonly DateTime YoungMinorDob = DateTime.UtcNow.AddYears(-14); // 13–15
    private static readonly DateTime BelowMinDob = DateTime.UtcNow.AddYears(-10);   // < 13

    private static SelfRegisterAthleteCommand Own(string displayName, string slug, DateTime dob, string? guardianEmail = null, bool consent = false)
        => new(TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, displayName, slug, dob, Locale.Da.Id, guardianEmail, consent);

    [Fact]
    public async Task Adult_self_registration_creates_owner_login_draft_and_athlete_control()
    {
        using var app = new TestApp();

        var dto = await app.Send(Own("Anna", "Anna-Judo", AdultDob));

        Assert.Equal("anna-judo", dto.Slug);
        Assert.False(dto.IsMinor);
        Assert.Equal(ControlMode.AthleteControlled, dto.ControlMode);

        var logins = await app.QueryAsync(c => c.ProfileLogins.Where(l => l.AthleteProfileId == dto.Id).ToListAsync());
        Assert.Single(logins);
        Assert.Equal(ProfileRole.AthleteOwner.Id, logins[0].RoleId);

        var owner = await app.QueryAsync(c => c.Users.SingleAsync(u => u.AuthProviderId == TestApp.OwnerAuthProviderId));
        Assert.True(owner.IsClaimed);

        var draft = await app.QueryAsync(c => c.SiteConfigs.SingleAsync(sc => sc.AthleteProfileId == dto.Id && sc.IsDraft));
        Assert.Collection(draft.Layout.Sections,
            s => Assert.IsType<HeroSectionData>(s.Data),
            s => Assert.IsType<BioSectionData>(s.Data));
    }

    [Fact]
    public async Task Young_minor_self_registration_issues_guardian_invitation_and_stays_athlete_controlled()
    {
        using var app = new TestApp();

        var dto = await app.Send(Own("Kid", "kid-judo", YoungMinorDob, guardianEmail: "parent@example.com", consent: true));

        Assert.True(dto.IsMinor);
        // Self-registered minor controls their own profile; the guardian is not (yet) the controller.
        Assert.Equal(ControlMode.AthleteControlled, dto.ControlMode);

        // The guardian is invited via an Invitation row — not a pending ProfileLogin, not a pre-created user.
        var invitation = await app.QueryAsync(c => c.Invitations.SingleAsync(i => i.TargetProfileId == dto.Id));
        Assert.Equal("parent@example.com", invitation.Email);
        Assert.Equal(ProfileRole.Guardian.Id, invitation.RoleId);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);

        // Only the owner login exists; the guardian credential is materialized at accept time.
        var logins = await app.QueryAsync(c => c.ProfileLogins.Where(l => l.AthleteProfileId == dto.Id).ToListAsync());
        var only = Assert.Single(logins);
        Assert.Equal(ProfileRole.AthleteOwner.Id, only.RoleId);

        // Parental consent declaration is stamped for the 13–15 band.
        var profile = await app.QueryAsync(c => c.AthleteProfiles.SingleAsync(p => p.Id == dto.Id));
        Assert.NotNull(profile.ConsentCapturedUtc);
    }

    [Fact]
    public async Task Older_minor_self_registration_needs_guardian_but_no_consent_stamp()
    {
        using var app = new TestApp();

        var dto = await app.Send(Own("Teen", "teen-judo", OlderMinorDob, guardianEmail: "parent@example.com"));

        Assert.Equal(ControlMode.AthleteControlled, dto.ControlMode);
        var invitation = await app.QueryAsync(c => c.Invitations.SingleAsync(i => i.TargetProfileId == dto.Id));
        Assert.Equal(ProfileRole.Guardian.Id, invitation.RoleId);

        var profile = await app.QueryAsync(c => c.AthleteProfiles.SingleAsync(p => p.Id == dto.Id));
        Assert.Null(profile.ConsentCapturedUtc); // consent implicit for 16+
    }

    [Fact]
    public async Task Under_13_self_registration_is_rejected_and_writes_no_profile()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(Own("Tiny", "tiny", BelowMinDob)));
        Assert.Equal(ErrorCodes.BelowMinimumAge, ex.ErrorCode);

        var profileCount = await app.QueryAsync(c => c.AthleteProfiles.CountAsync());
        Assert.Equal(0, profileCount);
    }

    [Fact]
    public async Task Young_minor_without_guardian_is_rejected()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(Own("Kid", "kid", YoungMinorDob, consent: true)));
        Assert.Equal(ErrorCodes.GuardianEmailRequired, ex.ErrorCode);

        var profileCount = await app.QueryAsync(c => c.AthleteProfiles.CountAsync());
        Assert.Equal(0, profileCount);
    }

    [Fact]
    public async Task Young_minor_without_parental_consent_is_rejected()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(Own("Kid", "kid", YoungMinorDob, guardianEmail: "parent@example.com", consent: false)));
        Assert.Equal(ErrorCodes.ParentalConsentRequired, ex.ErrorCode);
    }

    [Fact]
    public async Task Reserved_slug_is_rejected()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(Own("Admin", "admin", AdultDob)));
        Assert.Equal(ErrorCodes.SlugReserved, ex.ErrorCode);
    }

    [Fact]
    public async Task Duplicate_slug_across_two_callers_is_rejected()
    {
        using var app = new TestApp();
        await app.Send(Own("Anna", "anna", AdultDob));

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(new SelfRegisterAthleteCommand(
            "other-sub", "other@test.local", "Bjorn", "anna", AdultDob, Locale.Da.Id)));
        Assert.Equal(ErrorCodes.SlugAlreadyTaken, ex.ErrorCode);
    }

    [Fact]
    public async Task Same_caller_cannot_self_register_twice()
    {
        using var app = new TestApp();
        await app.Send(Own("Anna", "anna", AdultDob));

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(Own("Anna Again", "anna-2", AdultDob)));
        Assert.Equal(ErrorCodes.ProfileAlreadyExists, ex.ErrorCode);
    }
}
