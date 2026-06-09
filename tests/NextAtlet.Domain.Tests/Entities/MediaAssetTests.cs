namespace NextAtlet.Domain.Tests.Entities;

using FluentAssertions;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;

public class MediaAssetTests
{
    private static MediaAsset AClubFundedAsset(bool isClubBranding) => new()
    {
        AthleteProfileId = Guid.NewGuid(),
        Type = MediaAssetType.Image,
        OriginId = MediaOrigin.ClubFundedShoot.Id,
        IsClubBranding = isClubBranding,
        StorageKey = "abc123",
        Width = 800,
        Height = 600,
        AltText = "Maria competing"
    };

    [Fact]
    public void ClubFundedAsset_BelongsToTheAthlete_NotTheClub()
    {
        var asset = AClubFundedAsset(isClubBranding: false);

        asset.AthleteProfileId.Should().NotBe(Guid.Empty);
        asset.IsClubBranding.Should().BeFalse();
    }

    [Fact]
    public void ClubBrandingAsset_IsFlaggedForClubRetention()
    {
        var asset = AClubFundedAsset(isClubBranding: true);

        asset.IsClubBranding.Should().BeTrue();
    }

    //[Fact(Skip = "Confirm whether StaysWithAthleteOnClubExit is entity logic or resolved elsewhere.")]
    //TODO: SOMETHING HERE
    //public void NonBrandingAsset_StaysWithAthleteOnClubExit()
    //{
    //    var asset = AClubFundedAsset(isClubBranding: false);

    //    asset.StaysWithAthleteOnClubExit().Should().BeTrue();
    //}
}
