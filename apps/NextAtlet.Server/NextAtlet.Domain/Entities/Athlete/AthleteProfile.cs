using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Domain.Entities.Athlete;

public class AthleteProfile : AuditableEntity
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
    public ControlMode ControlMode { get; set; } = ControlMode.AthleteControlled;

    /// <summary>
    /// When parental consent was declared (13–15 self-registration). Null when consent is implicit
    /// (16+ self-register, or guardian-register where the guardian created the profile).
    /// </summary>
    public DateTime? ConsentCapturedUtc { get; set; }

    /// <summary>
    /// Denormalized read field — derived from the active Subscription.
    /// Not authoritative; never overwritten by club perks.
    /// Null until billing is wired (Step 6b).
    /// </summary>
    public string? SelfTierId { get; set; }

    // Navigation
    public ICollection<ProfileLogin> ProfileLogins { get; set; } = [];
    public ICollection<SiteConfig> SiteConfigs { get; set; } = [];
    public ICollection<MediaAsset> MediaAssets { get; set; } = [];

    /// <summary>
    /// Computed at request time from DateOfBirth. NOT stored — a stored flag
    /// goes stale the day the athlete turns 18.
    /// </summary>
    public bool IsMinor => DateTime.UtcNow < DateOfBirth.ToDateTime(TimeOnly.MinValue).AddYears(18);
}