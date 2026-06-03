using NextAtlet.Domain.Common;

public sealed class MediaOrigin : Enumeration
{
    public static readonly MediaOrigin SelfUpload = new()
    {
        Id = "self_upload",
        Title = "Uploaded by you",
        Description = "Media you uploaded directly to your profile"
    };

    public static readonly MediaOrigin AdminUpload = new()
    {
        Id = "admin_upload",
        Title = "Uploaded by NextAtlet",
        Description = "Media uploaded on your behalf by a NextAtlet admin"
    };

    public static readonly MediaOrigin ClubFundedShoot = new()
    {
        Id = "club_funded_shoot",
        Title = "Club funded shoot",
        Description = "Captured during a club-funded photoshoot. Photos stay with you if you leave the club"
    };

    public static readonly MediaOrigin OrganizationUpload = new()
    {
        Id = "organization_upload",
        Title = "Uploaded by organization",
        Description = "Media uploaded by your affiliated organization"
    };

    public static IReadOnlyCollection<MediaOrigin> All =>
        [SelfUpload, AdminUpload, ClubFundedShoot, OrganizationUpload];

    public static MediaOrigin FromId(string id) =>
        All.FirstOrDefault(o => o.Id == id)
        ?? throw new ArgumentException($"Unknown media origin: '{id}'");
}