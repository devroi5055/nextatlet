using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;
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
        var athlete = TestIndividuals.AnAthlete();

        Assert.False(string.IsNullOrWhiteSpace(athlete.SportId));
        Assert.Equal(ControlModes.AthleteControlled.Id, athlete.ControlModeId);
    }

    [Theory]
    [InlineData(nameof(TestIndividuals.AnUnder13Athlete), "below_minimum", true)]
    [InlineData(nameof(TestIndividuals.AYoungMinorAthlete), "young_minor", true)]
    [InlineData(nameof(TestIndividuals.AnOlderMinorAthlete), "older_minor", true)]
    [InlineData(nameof(TestIndividuals.AnAdultAthlete), "adult", false)]
    public void AgeBandVariants_ProduceTheExpectedBand(string variant, string expectedBand, bool expectedMinor)
    {
        var now = DateTime.UtcNow;
        var athlete = variant switch
        {
            nameof(TestIndividuals.AnUnder13Athlete) => TestIndividuals.AnUnder13Athlete(now),
            nameof(TestIndividuals.AYoungMinorAthlete) => TestIndividuals.AYoungMinorAthlete(now),
            nameof(TestIndividuals.AnOlderMinorAthlete) => TestIndividuals.AnOlderMinorAthlete(now),
            _ => TestIndividuals.AnAdultAthlete(now)
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
            "athlete_controlled" => TestIndividuals.AnAthleteControlledProfile(),
            "guardian_controlled" => TestIndividuals.AGuardianControlledProfile(),
            "athlete_controlled_shared" => TestIndividuals.AnAthleteControlledSharedProfile(),
            _ => TestIndividuals.AGuardianControlledSharedProfile()
        };

        Assert.Equal(modeId, athlete.ControlModeId);
    }

    [Fact]
    public void OwnerAndGuardianLogins_AreActiveWithTheRightRole()
    {
        var owner = SiteLogins.AnOwnerLogin();
        var guardian = SiteLogins.AGuardianLogin();

        Assert.Equal(IndividualRole.Owner.Id, owner.SiteRoleId);
        Assert.Equal(ProfileLoginStatus.Active.Id, owner.StatusId);
        Assert.Equal(IndividualRole.Guardian.Id, guardian.SiteRoleId);
        Assert.Equal(ProfileLoginStatus.Active.Id, guardian.StatusId);
    }

    [Fact]
    public void RevokedLogin_IsRevoked()
    {
        Assert.Equal(ProfileLoginStatus.Revoked.Id, SiteLogins.ARevokedOwnerLogin().StatusId);
        Assert.Equal(ProfileLoginStatus.Revoked.Id, SiteLogins.ARevokedGuardianLogin().StatusId);
    }

    [Fact]
    public void APendingInviteToken_IsPendingGuardianInvite()
    {
        var token = ActionTokens.APendingInviteToken();
        var payload = Assert.IsType<InvitePayload>(token.Payload);

        Assert.Equal(ActionTokenType.Invitation.Id, token.TypeId);
        Assert.Equal(IndividualRole.Guardian.Id, payload.RoleId);
        Assert.False(string.IsNullOrWhiteSpace(payload.Email));
        Assert.True(token.IsPending);
        Assert.False(token.IsExpired);
    }

    [Fact]
    public void AnExpiredInviteToken_IsExpired()
        => Assert.True(ActionTokens.AnExpiredInviteToken().IsExpired);

    [Fact]
    public void AnAcceptedInviteToken_IsAcceptedWithTimestamp()
    {
        var token = ActionTokens.AnAcceptedInviteToken();

        Assert.False(token.IsPending);
        Assert.NotNull(token.AcceptedUtc);
    }

    [Fact]
    public void AConsentToken_CarriesConsentPayload()
    {
        var token = ActionTokens.AConsentToken();

        Assert.Equal(ActionTokenType.Consent.Id, token.TypeId);
        Assert.IsType<ConsentPayload>(token.Payload);
    }

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
        var config = SiteSnapshots.ADraftSiteSnapshot();

        Assert.Null(config.PublishedUtc);
        Assert.NotNull(config.Layout);
        Assert.Equal(2, config.Layout.Sections.Count);
        Assert.NotNull(config.GlobalSettings);
    }

    [Fact]
    public void APublishedSiteConfig_IsPublished()
    {
        var config = SiteSnapshots.APublishedSiteSnapshot();

        Assert.NotNull(config.PublishedUtc);
    }
}
