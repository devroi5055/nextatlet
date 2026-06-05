using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.Policies;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Control is a stored, explicit fact — decoupled from age. A birthday (age-band transition) never
/// mutates ControlMode, and the resolver never reads age.
/// </summary>
public class ControlModeStabilityTests
{
    [Fact]
    public async Task ControlMode_is_stored_not_derived_from_age_band()
    {
        using var app = new TestApp();

        // A 17-year-old, guardian-registered → GuardianControlled. Age band is OlderMinor, but control
        // is the stored value, not derived from the band — they are independent.
        var dob = DateTime.UtcNow.AddYears(-17);
        var child = await app.Send(new GuardianRegisterAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Almost Adult", "almost-adult", dob, Locale.Da.Id));

        var profile = await app.QueryAsync(c => c.AthleteProfiles.SingleAsync(p => p.Id == child.Id));

        Assert.Equal(AgeBand.OlderMinor, AgePolicy.BandToday(profile.DateOfBirth));
        Assert.Equal(ControlMode.GuardianControlled, profile.ControlMode); // stored, not recomputed from age
    }

    [Fact]
    public void Resolution_is_independent_of_age()
    {
        var resolver = new PermissionResolver();
        var guardian = new ProfileLogin
        {
            UserId = Guid.NewGuid(),
            AthleteProfileId = Guid.NewGuid(),
            RoleId = ProfileRole.Guardian.Id,
            Status = ProfileLoginStatus.Active
        };

        AthleteProfile Profile(DateOnly dob) => new()
        {
            Slug = "x",
            DisplayName = "X",
            DateOfBirth = dob,
            ControlMode = ControlMode.GuardianControlled
        };

        // A newborn and a near-adult in the same ControlMode resolve identically — age is not an input.
        var newborn = resolver.Resolve(guardian, Profile(DateOnly.FromDateTime(DateTime.UtcNow)));
        var nearAdult = resolver.Resolve(guardian, Profile(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-17))));

        Assert.Equal(ProfilePermissions.FullControl, newborn);
        Assert.Equal(ProfilePermissions.FullControl, nearAdult);
    }
}
