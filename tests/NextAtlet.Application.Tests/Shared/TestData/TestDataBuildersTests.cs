using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Media;
using NextAtlet.Domain.Enumerations.Shared;
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
        Assert.Equal(ControlMode.AthleteControlled.Id, athlete.ControlModeId);
    }

    [Theory]
    [InlineData(nameof(TestAthletes.AnUnder13Athlete), "below_minimum", true)]
    [InlineData(nameof(TestAthletes.AYoungMinorAthlete), "young_minor", true)]
    [InlineData(nameof(TestAthletes.AnOlderMinorAthlete), "older_minor", true)]
    [InlineData(nameof(TestAthletes.AnAdultAthlete), "adult", false)]
    public void AgeBandVariants_ProduceTheExpectedBand(string variant, string expectedBand, bool expectedMinor)
    {
        var now = DateTime.UtcNow;
        var athlete = variant switch
        {
            nameof(TestAthletes.AnUnder13Athlete) => TestAthletes.AnUnder13Athlete(now),
            nameof(TestAthletes.AYoungMinorAthlete) => TestAthletes.AYoungMinorAthlete(now),
            nameof(TestAthletes.AnOlderMinorAthlete) => TestAthletes.AnOlderMinorAthlete(now),
            _ => TestAthletes.AnAdultAthlete(now)
        };

        Assert.Equal(AgeBand.FromId(expectedBand), AgePolicy.BandToday(athlete.DateOfBirth, now));
        Assert.Equal(expectedMinor, athlete.IsMinor(now));
    }

    [Theory]
    [InlineData("athlete_controlled")]
    [InlineData("guardian_controlled")]
    [InlineData("athlete_controlled_shared")]
    [InlineData("guardian_controlled_shared")]
    public void ControlModeVariants_SetTheExpectedMode(string modeId)
    {
        var athlete = modeId switch
        {
            "athlete_controlled" => TestAthletes.AnAthleteControlledProfile(),
            "guardian_controlled" => TestAthletes.AGuardianControlledProfile(),
            "athlete_controlled_shared" => TestAthletes.AnAthleteControlledSharedProfile(),
            _ => TestAthletes.AGuardianControlledSharedProfile()
        };

        Assert.Equal(modeId, athlete.ControlModeId);
    }

    [Fact]
    public void OwnerAndGuardianLogins_AreActiveWithTheRightRole()
    {
        var owner = ProfileLogins.AnOwnerLogin();
        var guardian = ProfileLogins.AGuardianLogin();

        Assert.Equal(ProfileRole.AthleteOwner.Id, owner.RoleId);
        Assert.Equal(ProfileLoginStatus.Active.Id, owner.StatusId);
        Assert.Equal(ProfileRole.Guardian.Id, guardian.RoleId);
        Assert.Equal(ProfileLoginStatus.Active.Id, guardian.StatusId);
    }

    [Fact]
    public void RevokedLogin_IsRevoked()
    {
        Assert.Equal(ProfileLoginStatus.Revoked.Id, ProfileLogins.ARevokedOwnerLogin().StatusId);
        Assert.Equal(ProfileLoginStatus.Revoked.Id, ProfileLogins.ARevokedGuardianLogin().StatusId);
    }

    [Fact]
    public void APendingInvitation_IsPendingGuardianInvite()
    {
        var invitation = Invitations.APendingInvitation();

        Assert.Equal(InvitationStatus.Pending.Id, invitation.StatusId);
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

        Assert.Equal(InvitationStatus.Accepted.Id, invitation.StatusId);
        Assert.NotNull(invitation.AcceptedUtc);
    }

    [Fact]
    public void ARevokedInvitation_IsRevoked()
        => Assert.Equal(InvitationStatus.Revoked.Id, Invitations.ARevokedInvitation().StatusId);

    [Fact]
    public void AClubFundedAsset_StaysWithTheAthlete()
    {
        var asset = MediaAssets.AClubFundedAsset();

        Assert.Equal(MediaOrigin.ClubFundedShoot.Id, asset.OriginId);
        Assert.False(asset.IsClubBranding);
        Assert.NotNull(asset.AthleteSiteId);
        Assert.Null(asset.OrganizationId);
    }

    [Fact]
    public void AClubBrandingAsset_IsFlaggedForClubRetention()
        => Assert.True(MediaAssets.AClubBrandingAsset().IsClubBranding);

    [Fact]
    public void ADraftSiteConfig_IsDraftWithValidLayout()
    {
        var config = AthleteSiteSnapshots.ADraftSiteSnapshot();

        Assert.Null(config.PublishedUtc);
        Assert.NotNull(config.Layout);
        Assert.Equal(2, config.Layout.Sections.Count);
        Assert.NotNull(config.GlobalSettings);
    }

    [Fact]
    public void APublishedSiteConfig_IsPublished()
    {
        var config = AthleteSiteSnapshots.APublishedSiteSnapshot();

        Assert.NotNull(config.PublishedUtc);
    }
}
