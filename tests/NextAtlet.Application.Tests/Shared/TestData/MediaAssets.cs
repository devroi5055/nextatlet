using AutoFixture;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Media;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="MediaAsset"/>, athlete-owned (the owner is XOR — never also an org).
/// The club-funded variants cover the retention rule: non-branding stays with the athlete; branding
/// is flagged for club retention.
/// </summary>
public static class MediaAssets
{
    public static MediaAsset AMediaAsset(Action<MediaAsset>? customize = null)
    {
        var asset = TestFixture.Create().Build<MediaAsset>()
            .Without(m => m.AthleteSite)
            .Without(m => m.OrganizationId) // owner XOR → athlete-owned
            .With(m => m.AthleteSiteId, Guid.NewGuid())
            .With(m => m.TypeId, MediaAssetType.Image.Id)
            .With(m => m.OriginId, MediaOrigin.SelfUpload.Id)
            .With(m => m.IsClubBranding, false)
            .Create();
        customize?.Invoke(asset);
        return asset;
    }

    public static MediaAsset AClubFundedAsset(Action<MediaAsset>? customize = null)
        => AMediaAsset(m =>
        {
            m.OriginId = MediaOrigin.ClubFundedShoot.Id;
            m.IsClubBranding = false; // funded ≠ identity owned → stays with the athlete
            customize?.Invoke(m);
        });

    public static MediaAsset AClubBrandingAsset(Action<MediaAsset>? customize = null)
        => AMediaAsset(m =>
        {
            m.OriginId = MediaOrigin.ClubFundedShoot.Id;
            m.IsClubBranding = true; // club-retained on membership end
            customize?.Invoke(m);
        });
}
