using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.ValueObjects.Sections;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Self-registration: the caller becomes the AthleteOwner.
/// </summary>
public class SelfRegisterAthleteCommandTests
{
    private static readonly DateTime AdultDob = new(1995, 1, 1);
    private static readonly DateTime MinorDob = DateTime.UtcNow.AddYears(-10);

    private static SelfRegisterAthleteCommand Own(string displayName, string slug, DateTime dob, string? guardianEmail = null)
        => new(TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, displayName, slug, dob, Locale.Da.Id, guardianEmail);

    [Fact]
    public async Task Adult_self_registration_creates_owner_login_and_draft()
    {
        using var app = new TestApp();

        var dto = await app.Send(Own("Anna", "Anna-Judo", AdultDob));

        Assert.Equal("anna-judo", dto.Slug);
        Assert.False(dto.IsMinor);

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
    public async Task Minor_self_registration_issues_a_guardian_invitation()
    {
        using var app = new TestApp();

        var dto = await app.Send(Own("Kid", "kid-judo", MinorDob, guardianEmail: "parent@example.com"));

        Assert.True(dto.IsMinor);

        // The guardian is invited via an Invitation row — not a pending ProfileLogin, not a pre-created user.
        var invitation = await app.QueryAsync(c => c.Invitations.SingleAsync(i => i.TargetProfileId == dto.Id));
        Assert.Equal("parent@example.com", invitation.Email);
        Assert.Equal(ProfileRole.Guardian.Id, invitation.RoleId);
        Assert.Equal(NextAtlet.Domain.Enumerations.Enums.AthleteProfile.InvitationStatus.Pending, invitation.Status);

        // Only the owner login exists; the guardian credential is materialized at accept time.
        var logins = await app.QueryAsync(c => c.ProfileLogins.Where(l => l.AthleteProfileId == dto.Id).ToListAsync());
        var only = Assert.Single(logins);
        Assert.Equal(ProfileRole.AthleteOwner.Id, only.RoleId);

        // No user row is pre-created for the invited guardian.
        var guardianUsers = await app.QueryAsync(c => c.Users.CountAsync(u => u.Email == "parent@example.com"));
        Assert.Equal(0, guardianUsers);
    }

    [Fact]
    public async Task Minor_without_guardian_is_rejected_and_writes_no_profile()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(Own("Kid", "kid", MinorDob)));
        Assert.Equal(ErrorCodes.GuardianEmailRequired, ex.ErrorCode);

        // atomicity: nothing was written
        var profileCount = await app.QueryAsync(c => c.AthleteProfiles.CountAsync());
        Assert.Equal(0, profileCount);
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

        // a DIFFERENT caller tries the same slug
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
