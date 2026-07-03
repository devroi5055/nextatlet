using NextAtlet.Domain.Common;
using NextAtlet.Domain.Enumerations.Billing;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Identity;

/// <summary>
/// The kind of action an <see cref="Domain.Entities.Identity.ActionToken"/> authorizes when its
/// emailed link is followed. One value per link-bearing flow. Other verification methods (manual,
/// MitID) reach their terminal state without a token and so are NOT represented here — this is the
/// set of <i>clickable-link</i> actions only.
/// </summary>
/// 
public sealed class ActionTokenType : Enumeration
{
    public static readonly ActionTokenType Invitation = new()
    {
        Id = "invitation",
        Title = new LocalizedText { Da = "Profil Invitation", En = "Profile Invite" },
        Description = new LocalizedText { Da = "Invitation til at deltage i profil", En = "Invitation to join profile" }
    };
    public static readonly ActionTokenType Consent = new()
    {
        Id = "consent",
        Title = new LocalizedText { Da = "Forældre Sammentykke", En = "Parental Consent" },
        Description = new LocalizedText { Da = "Forældre Sammentykke til at aktivere en Profil", En = "Parental Consent for activating a Profile" }
    };
    public static readonly ActionTokenType OrgEmailVerification = new()
    {
        Id = "org_email_verification",
        Title = new LocalizedText { Da = "Organisatorisk Email Verificering", En = "Organization Email Verification" },
        Description = new LocalizedText { Da = "verificering af Organisation via Email til Klubmedlem", En = "Verification of the Organization via Email to a Club Member/Official" }
    };

    public static IReadOnlyCollection<ActionTokenType> All => [Invitation, Consent, OrgEmailVerification];

    public static ActionTokenType FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown Action Token type: '{id}'");
}

