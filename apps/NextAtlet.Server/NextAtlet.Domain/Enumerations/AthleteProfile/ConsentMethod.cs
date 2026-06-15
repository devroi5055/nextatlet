using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.AthleteProfile;

/// <summary>
/// HOW guardian consent was verified (a GDPR-required fact). MitID is the natural hard-assurance
/// upgrade for the Danish audience.
/// </summary>
public sealed class ConsentMethod : Enumeration
{
    public static readonly ConsentMethod VerifiedEmail = new()
    {
        Id = "verified_email",
        Title = new LocalizedText { Da = "Bekræftet email", En = "Verified email" },
        Description = new LocalizedText { Da = "Forælder/Værge godkendt via Auth0 og bekræftet", En = "Guardian authenticated via Auth0 and confirmed" }
    };

    // Future: MitId, SmsToken, ...

    public static IReadOnlyCollection<ConsentMethod> All => [VerifiedEmail];

    public static ConsentMethod FromId(string id) =>
        All.FirstOrDefault(m => m.Id == id)
        ?? throw new ArgumentException($"Unknown consent method: '{id}'");
}
