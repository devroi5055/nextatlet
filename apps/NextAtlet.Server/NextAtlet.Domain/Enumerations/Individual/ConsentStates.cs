using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.AthleteProfile;

/// <summary>
/// The guardian-consent gate on a profile (GDPR Art. 8). Orthogonal to VisibilityState — only
/// governs whether the profile may go public, never the public/private choice itself.
/// </summary>
public sealed class ConsentStates : Enumeration
{
    public static readonly ConsentStates NotRequired = new()
    {
        Id = "not_required",
        Title = new LocalizedText { Da = "Ikke påkrævet", En = "Not required" },
        Description = new LocalizedText { Da = "Selvsamtykkende alder, eller forældre-registreret (forældre er til stede)", En = "Self-consenting age, or guardian-registered (guardian present)" }
    };

    public static readonly ConsentStates PendingGuardianConsent = new()
    {
        Id = "pending_guardian_consent",
        Title = new LocalizedText { Da = "Afventer samtykke", En = "Pending guardian consent" },
        Description = new LocalizedText { Da = "Under selvsamtykkende alder, afventer forældreverifikation — kladde kan redigeres, kan ikke offentliggøres", En = "Under self-consent age, awaiting guardian verification — draft editable, cannot go public" }
    };

    public static readonly ConsentStates Consented = new()
    {
        Id = "consented",
        Title = new LocalizedText { Da = "Samtykke givet", En = "Consented" },
        Description = new LocalizedText { Da = "Forældre har verificeret — publiceringsporten er løftet", En = "Guardian verified — publish gate lifted" }
    };

    public static IReadOnlyCollection<ConsentStates> All => [NotRequired, PendingGuardianConsent, Consented];

    public static ConsentStates FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown consent state: '{id}'");
}
