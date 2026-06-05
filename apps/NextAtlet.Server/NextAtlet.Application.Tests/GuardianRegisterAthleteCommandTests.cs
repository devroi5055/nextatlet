using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Domain.Enumerations;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Guardian-creates-profile-for-child: the caller becomes the Guardian; no AthleteOwner login (v1).
/// </summary>
public class GuardianRegisterAthleteCommandTests
{
    private static readonly DateTime AdultDob = new(1995, 1, 1);
    private static readonly DateTime ChildDob = DateTime.UtcNow.AddYears(-9);

    private static GuardianRegisterAthleteCommand Child(string childName, string slug, DateTime dob)
        => new(TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, childName, slug, dob, Locale.Da.Id);

    [Fact]
    public async Task Guardian_registers_child_creates_guardian_login_and_no_owner_login()
    {
        using var app = new TestApp();

        var dto = await app.Send(Child("Little Judoka", "little-judoka", ChildDob));

        Assert.True(dto.IsMinor);

        var logins = await app.QueryAsync(c => c.ProfileLogins.Where(l => l.AthleteProfileId == dto.Id).ToListAsync());
        var single = Assert.Single(logins);
        Assert.Equal(ProfileRole.Guardian.Id, single.RoleId); // guardian only, no owner

        // the guardian is the (claimed) caller
        var guardian = await app.QueryAsync(c => c.Users.SingleAsync(u => u.AuthProviderId == TestApp.OwnerAuthProviderId));
        Assert.Equal(single.UserId, guardian.Id);
        Assert.True(guardian.IsClaimed);
    }

    [Fact]
    public async Task Guardian_registering_an_adult_is_rejected()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() => app.Send(Child("Grown Up", "grown-up", AdultDob)));
        Assert.Equal(ErrorCodes.GuardianCannotRegisterAdult, ex.ErrorCode);

        var profileCount = await app.QueryAsync(c => c.AthleteProfiles.CountAsync());
        Assert.Equal(0, profileCount);
    }

    [Fact]
    public async Task Guardian_can_register_multiple_children()
    {
        using var app = new TestApp();

        var first = await app.Send(Child("Kid One", "kid-one", ChildDob));
        var second = await app.Send(Child("Kid Two", "kid-two", ChildDob));

        Assert.NotEqual(first.Id, second.Id);

        var profiles = await app.QueryAsync(c => c.AthleteProfiles.CountAsync());
        Assert.Equal(2, profiles);

        // one guardian user, two guardian logins
        var guardianLogins = await app.QueryAsync(c =>
            c.ProfileLogins.CountAsync(l => l.RoleId == ProfileRole.Guardian.Id));
        Assert.Equal(2, guardianLogins);
        var users = await app.QueryAsync(c => c.Users.CountAsync());
        Assert.Equal(1, users);
    }
}
