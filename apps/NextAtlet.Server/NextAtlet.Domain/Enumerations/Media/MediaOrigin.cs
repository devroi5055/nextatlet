using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Media;

public sealed class MediaOrigin : Enumeration
{
    public static readonly MediaOrigin SelfUpload = new()
    {
        Id = "self_upload",
        Title = new LocalizedText { Da = "Egne uploads", En = "Uploaded by you" },
        Description = new LocalizedText { Da = "Medie du selv uploadede til din profil", En = "Media you uploaded directly to your profile" }
    };

    public static readonly MediaOrigin AdminUpload = new()
    {
        Id = "admin_upload",
        Title = new LocalizedText { Da = "Uploadet af NextAtlet", En = "Uploaded by NextAtlet" },
        Description = new LocalizedText { Da = "Medie uploadet på dine vegne af en NextAtlet-administrator", En = "Media uploaded on your behalf by a NextAtlet admin" }
    };

    public static readonly MediaOrigin ClubFundedShoot = new()
    {
        Id = "club_funded_shoot",
        Title = new LocalizedText { Da = "Klubfinansieret fotosession", En = "Club funded shoot" },
        Description = new LocalizedText { Da = "Optaget under en klubfinansieret fotosession. Fotos forbliver hos dig, hvis du forlader klubben", En = "Captured during a club-funded photoshoot. Photos stay with you if you leave the club" }
    };

    public static readonly MediaOrigin OrganizationUpload = new()
    {
        Id = "organization_upload",
        Title = new LocalizedText { Da = "Uploadet af organisation", En = "Organization upload" },
        Description = new LocalizedText { Da = "Medie uploadet af din tilknyttede organisation", En = "Media uploaded by your affiliated organization" }
    };

    public static IReadOnlyCollection<MediaOrigin> All => [SelfUpload, AdminUpload, ClubFundedShoot, OrganizationUpload];

    public static MediaOrigin FromId(string id) =>
        All.FirstOrDefault(o => o.Id == id)
        ?? throw new ArgumentException($"Unknown media origin: '{id}'");
}
