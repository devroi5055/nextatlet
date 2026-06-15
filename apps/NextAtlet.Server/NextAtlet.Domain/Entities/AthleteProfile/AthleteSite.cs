using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Domain.Entities.AthleteProfile;

public class AthleteSite : AuditableEntity
{
    public required string Slug { get; set; }
    public required string DisplayName { get; set; }

    /// <summary>
    /// Sport this athlete competes in. Defaults to judo at launch.
    /// Typed as string — extend Sport enumeration and update this default as new sports are added, but avoid a hard dependency on the enumeration to allow future flexibility.
    /// </summary>
    public string SportId { get; set; } = Sport.Judo.Id;

    /// <summary>
    /// Source of truth for minor/adult status. Never store IsMinor — it goes stale.
    /// </summary>
    public required DateOnly DateOfBirth { get; set; }

    public string DefaultLocaleId { get; set; } = Locale.Da.Id;
    public string VisibilityStateId { get; set; } = VisibilityState.Public.Id;

    /// <summary>
    /// Who controls this profile — a stored, explicit fact set at registration and changed only via the
    /// transfer-control / collaboration endpoints. Never derived from age, never auto-mutated.
    /// </summary>
    public string ControlModeId { get; set; } = ControlMode.AthleteControlled.Id;

    /// <summary>
    /// Guardian-consent gate (GDPR Art. 8). Stored, explicit. A profile may go public only when this
    /// is not <see cref="ConsentState.PendingGuardianConsent"/>. Orthogonal to VisibilityState.
    /// </summary>
    public string ConsentStateId { get; set; } = ConsentState.NotRequired.Id;

    /// <summary>
    /// Denormalized read field — derived from the active Subscription.
    /// Not authoritative; never overwritten by club perks.
    /// Null until billing is wired (Step 6b).
    /// </summary>
    /// TODO: might remove
    public string? SelfTierId { get; set; }

    // Navigation
    public Guid? CurrentDraftSnapshotId { get; set; }
    public Guid? CurrentPublishedSnapshotId { get; set; }
    public ICollection<ProfileLogin> ProfileLogins { get; set; } = [];
    public ICollection<MediaAsset> MediaAssets { get; set; } = [];

    /// <summary>
    /// Computed at request time from DateOfBirth. NOT stored — a stored flag
    /// goes stale the day the athlete turns 18.
    /// </summary>
    public bool IsMinor(DateTime utcNow) => utcNow < DateOfBirth.ToDateTime(TimeOnly.MinValue).AddYears(18);

    /// <summary>
    /// True while awaiting guardian consent — the publish path must refuse to make the profile public
    /// in this state (no public processing pre-consent). The draft may still be edited.
    /// </summary>
    public bool AwaitsGuardianConsent => ConsentStateId == ConsentState.PendingGuardianConsent.Id;

}