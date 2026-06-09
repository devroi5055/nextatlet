using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.Policies;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Sanity checks that double as documentation for the entity builders: each asserts the
/// variant-defining field, leaving the filler to AutoFixture.
/// </summary>
public class TestDataBuildersTests
{
    [Fact]
    public void AnAuthenticatedUser_IsClaimed()
    {
        var user = Users.AnAuthenticatedUser();

        Assert.False(string.IsNullOrWhiteSpace(user.AuthProviderId));
        Assert.True(user.IsClaimed);
    }

    [Fact]
    public void APendingUser_HasNoSubject_AndIsUnclaimed()
    {
        var user = Users.APendingUser();

        Assert.Null(user.AuthProviderId);
        Assert.False(user.IsClaimed);
    }

    [Fact]
    public void AnAthlete_DefaultIsCoherentAndAthleteControlled()
    {
        var athlete = TestAthletes.AnAthlete();

        Assert.False(string.IsNullOrWhiteSpace(athlete.Slug));
        Assert.Equal(ControlMode.AthleteControlled, athlete.ControlMode);
    }

    [Theory]
    [InlineData(nameof(TestAthletes.AnUnder13Athlete), AgeBand.BelowMinimum, true)]
    [InlineData(nameof(TestAthletes.AYoungMinorAthlete), AgeBand.YoungMinor, true)]
    [InlineData(nameof(TestAthletes.AnOlderMinorAthlete), AgeBand.OlderMinor, true)]
    [InlineData(nameof(TestAthletes.AnAdultAthlete), AgeBand.Adult, false)]
    public void AgeBandVariants_ProduceTheExpectedBand(string variant, AgeBand expectedBand, bool expectedMinor)
    {
        var now = DateTime.UtcNow;
        var athlete = variant switch
        {
            nameof(TestAthletes.AnUnder13Athlete) => TestAthletes.AnUnder13Athlete(now),
            nameof(TestAthletes.AYoungMinorAthlete) => TestAthletes.AYoungMinorAthlete(now),
            nameof(TestAthletes.AnOlderMinorAthlete) => TestAthletes.AnOlderMinorAthlete(now),
            _ => TestAthletes.AnAdultAthlete(now)
        };

        Assert.Equal(expectedBand, AgePolicy.BandToday(athlete.DateOfBirth, now));
        Assert.Equal(expectedMinor, athlete.IsMinor(now));
    }

    [Theory]
    [InlineData(ControlMode.AthleteControlled)]
    [InlineData(ControlMode.GuardianControlled)]
    [InlineData(ControlMode.AthleteControlledShared)]
    [InlineData(ControlMode.GuardianControlledShared)]
    public void ControlModeVariants_SetTheExpectedMode(ControlMode mode)
    {
        var athlete = mode switch
        {
            ControlMode.AthleteControlled => TestAthletes.AnAthleteControlledProfile(),
            ControlMode.GuardianControlled => TestAthletes.AGuardianControlledProfile(),
            ControlMode.AthleteControlledShared => TestAthletes.AnAthleteControlledSharedProfile(),
            _ => TestAthletes.AGuardianControlledSharedProfile()
        };

        Assert.Equal(mode, athlete.ControlMode);
    }

    [Fact]
    public void OwnerAndGuardianLogins_AreActiveWithTheRightRole()
    {
        var owner = ProfileLogins.AnOwnerLogin();
        var guardian = ProfileLogins.AGuardianLogin();

        Assert.Equal(ProfileRole.AthleteOwner.Id, owner.RoleId);
        Assert.Equal(ProfileLoginStatus.Active, owner.Status);
        Assert.Equal(ProfileRole.Guardian.Id, guardian.RoleId);
        Assert.Equal(ProfileLoginStatus.Active, guardian.Status);
    }

    [Fact]
    public void RevokedLogin_IsRevoked()
    {
        Assert.Equal(ProfileLoginStatus.Revoked, ProfileLogins.ARevokedOwnerLogin().Status);
        Assert.Equal(ProfileLoginStatus.Revoked, ProfileLogins.ARevokedGuardianLogin().Status);
    }

    [Fact]
    public void APendingInvitation_IsPendingGuardianInvite()
    {
        var invitation = Invitations.APendingInvitation();

        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Equal(ProfileRole.Guardian.Id, invitation.RoleId);
        Assert.False(string.IsNullOrWhiteSpace(invitation.Email));
        Assert.False(invitation.IsExpired);
    }

    [Fact]
    public void AnExpiredInvitation_IsExpired()
        => Assert.True(Invitations.AnExpiredInvitation().IsExpired);

    [Fact]
    public void AnAcceptedInvitation_IsAcceptedWithTimestamp()
    {
        var invitation = Invitations.AnAcceptedInvitation();

        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        Assert.NotNull(invitation.AcceptedUtc);
    }

    [Fact]
    public void ARevokedInvitation_IsRevoked()
        => Assert.Equal(InvitationStatus.Revoked, Invitations.ARevokedInvitation().Status);

    [Fact]
    public void AClubFundedAsset_StaysWithTheAthlete()
    {
        var asset = MediaAssets.AClubFundedAsset();

        Assert.Equal(MediaOrigin.ClubFundedShoot.Id, asset.OriginId);
        Assert.False(asset.IsClubBranding);
        Assert.NotNull(asset.AthleteProfileId);
        Assert.Null(asset.OrganizationId);
    }

    [Fact]
    public void AClubBrandingAsset_IsFlaggedForClubRetention()
        => Assert.True(MediaAssets.AClubBrandingAsset().IsClubBranding);

    [Fact]
    public void ADraftSiteConfig_IsDraftWithValidLayout()
    {
        var config = SiteConfigs.ADraftSiteConfig();

        Assert.True(config.IsDraft);
        Assert.Null(config.PublishedUtc);
        Assert.NotNull(config.Layout);
        Assert.Equal(2, config.Layout.Sections.Count);
        Assert.NotNull(config.GlobalSettings);
    }

    [Fact]
    public void APublishedSiteConfig_IsPublished()
    {
        var config = SiteConfigs.APublishedSiteConfig();

        Assert.False(config.IsDraft);
        Assert.NotNull(config.PublishedUtc);
    }
}
