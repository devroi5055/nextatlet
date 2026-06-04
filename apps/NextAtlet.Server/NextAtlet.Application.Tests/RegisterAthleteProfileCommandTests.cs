using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.ValueObjects.Sections;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Characterization tests for athlete registration, dispatched through the MediatR pipeline.
/// Owner identity is supplied by the fake ICurrentUserContext, not the command.
/// </summary>
public class RegisterAthleteProfileCommandTests
{
    private static readonly DateTime AdultDob = new(1995, 1, 1);
    private static readonly DateTime MinorDob = DateTime.UtcNow.AddYears(-10);

    [Fact]
    public async Task Creates_profile_owner_login_and_draft_config_for_adult()
    {
        using var app = new TestApp();

        var dto = await app.Send(new RegisterAthleteProfileCommand(
            "Anna", "Anna-Judo", AdultDob, Locale.Da.Id));

        Assert.Equal("anna-judo", dto.Slug); // slug is lowercased
        Assert.False(dto.IsMinor);

        // exactly one AthleteOwner login, no guardian
        var logins = await app.QueryAsync(c =>
            c.ProfileLogins.Where(l => l.AthleteProfileId == dto.Id).ToListAsync());
        Assert.Single(logins);
        Assert.Equal(ProfileRole.AthleteOwner.Id, logins[0].RoleId);

        // owner user is claimed (identity came from the authenticated caller)
        var owner = await app.QueryAsync(c => c.Users.SingleAsync(u => u.AuthProviderId == TestApp.OwnerAuthProviderId));
        Assert.True(owner.IsClaimed);
        Assert.Equal(TestApp.OwnerEmail, owner.Email);

        // a draft config with the default hero + bio sections
        var draft = await app.QueryAsync(c =>
            c.SiteConfigs.SingleAsync(sc => sc.AthleteProfileId == dto.Id && sc.IsDraft));
        Assert.Equal(1, draft.Version);
        Assert.Collection(draft.Layout.Sections,
            s => Assert.IsType<HeroSectionData>(s.Data),
            s => Assert.IsType<BioSectionData>(s.Data));
    }

    [Fact]
    public async Task Creates_pending_unclaimed_guardian_for_minor()
    {
        using var app = new TestApp();

        var dto = await app.Send(new RegisterAthleteProfileCommand(
            "Kid", "kid-judo", MinorDob, Locale.Da.Id, GuardianEmail: "parent@example.com"));

        Assert.True(dto.IsMinor);

        var guardianLogin = await app.QueryAsync(c => c.ProfileLogins.SingleAsync(l =>
            l.AthleteProfileId == dto.Id && l.RoleId == ProfileRole.Guardian.Id));
        Assert.Equal(NextAtlet.Domain.Enumerations.Enums.AthleteProfile.ProfileLoginStatus.Pending, guardianLogin.Status);

        // invited guardian exists as an UNCLAIMED user (no AuthProviderId yet)
        var guardian = await app.QueryAsync(c => c.Users.SingleAsync(u => u.Email == "parent@example.com"));
        Assert.Null(guardian.AuthProviderId);
        Assert.False(guardian.IsClaimed);
    }

    [Fact]
    public async Task Duplicate_slug_throws()
    {
        using var app = new TestApp();
        await app.Send(new RegisterAthleteProfileCommand("Anna", "anna", AdultDob, Locale.Da.Id));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new RegisterAthleteProfileCommand("Bjorn", "anna", AdultDob, Locale.Da.Id)));
        Assert.Equal(ErrorCodes.SlugAlreadyTaken, ex.ErrorCode);
        Assert.Contains("anna", ex.Parameters);
    }

    [Fact]
    public async Task Reserved_slug_throws()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new RegisterAthleteProfileCommand("Admin", "admin", AdultDob, Locale.Da.Id)));
        Assert.Equal(ErrorCodes.SlugReserved, ex.ErrorCode);
    }

    [Fact]
    public async Task Minor_without_guardian_throws()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new RegisterAthleteProfileCommand("Kid", "kid", MinorDob, Locale.Da.Id)));
        Assert.Equal(ErrorCodes.GuardianEmailRequired, ex.ErrorCode);
    }

    [Fact]
    public async Task Second_registration_for_same_user_throws()
    {
        using var app = new TestApp();
        await app.Send(new RegisterAthleteProfileCommand("Anna", "anna", AdultDob, Locale.Da.Id));

        // same authenticated caller (fake identity), different slug → one-profile-per-owner guard
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new RegisterAthleteProfileCommand("Anna Again", "anna-2", AdultDob, Locale.Da.Id)));
        Assert.Equal(ErrorCodes.ProfileAlreadyExists, ex.ErrorCode);
    }
}
