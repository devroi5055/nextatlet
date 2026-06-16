using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Extensions;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Policies;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Shared mechanics for the two registration flows (self vs guardian-creates-child). Owns everything
/// identical between them — slug validation, profile + default draft AthleteSiteSnapshot creation, and user
/// get-or-create. Each concrete handler owns only its login-attachment + flow-specific rules.
/// Repositories are protected and shared; the concrete handler calls <c>SaveChangesAsync</c> once.
/// </summary>
public abstract class AthleteRegistrationHandlerBase
{
    protected static readonly string[] ReservedSlugs =
        ["admin", "api", "about", "contact", "terms", "privacy", "login", "signup", "settings", "dashboard"];

    protected readonly ISiteRepository _sites;
    protected readonly ISiteLoginRepository _logins;
    protected readonly IAthleteProfileRepository _athleteProfiles;
    protected readonly IThemeRepository _themes;
    protected readonly ISiteSnapshotRepository _siteSnapshots;
    protected readonly UserProvisioner _userProvisioner;
    protected readonly InvitationIssuer _inviter;
    protected readonly IClock _clock;
    protected readonly AgeThresholdOptions _threshold;
    protected readonly IUnitOfWork _unitOfWork;

    protected AthleteRegistrationHandlerBase(
        ISiteRepository sites,
        ISiteLoginRepository logins,
        IAthleteProfileRepository athleteProfiles,
        IThemeRepository themes,
        ISiteSnapshotRepository siteSnapshots,
        UserProvisioner userProvisioner,
        InvitationIssuer inviter,
        IClock clock,
        AgeThresholdOptions threshold,
        IUnitOfWork unitOfWork)
    {
        _sites = sites;
        _logins = logins;
        _athleteProfiles = athleteProfiles;
        _themes = themes;
        _siteSnapshots = siteSnapshots;
        _userProvisioner = userProvisioner;
        _inviter = inviter;
        _clock = clock;
        _threshold = threshold;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Slug validation + the AthleteProfile (with its explicit <paramref name="controlMode"/>) + its
    /// default draft AthleteSiteSnapshot. Returns the tracked profile with NO logins attached — the
    /// caller attaches owner/guardian logins per its flow.
    /// </summary>
    protected async Task<Result<SiteDto>> CreateAthleteProfileCoreAsync(
        string slug, string displayName, DateTime dateOfBirth, string defaultLocaleId, ControlModes controlMode, CancellationToken cancellationToken)
    {
        slug = slug.ToLowerInvariant();

        // Business rejections — recoverable, user-facing.
        if (await _sites.SlugExistsAsync(slug, cancellationToken))
            return Error.FromCode(ErrorCodes.SlugAlreadyTaken);
        if (ReservedSlugs.Contains(slug))
            return Error.FromCode(ErrorCodes.SlugReserved);

        var site = new Site
        {
            Slug = slug,
            DisplayName = displayName,
            VisibilityStateId = "public",
            DefaultLocaleId = defaultLocaleId,

        };
        _sites.Add(site);
        

        var consentIsRequired = AgePolicy.RequiresGuardianConsent(DateOnly.FromDateTime(dateOfBirth), _clock.UtcNow, _threshold.SelfConsentAge);
        var profile = new AthleteProfile
        {
            SiteId = site.Id,
            SportId = "judo",
            DateOfBirth = DateOnly.FromDateTime(dateOfBirth),
            ConsentStateId = consentIsRequired ? ConsentStates.PendingGuardianConsent.Id : ConsentStates.NotRequired.Id,
            ControlModeId = controlMode.Id

        };
        _athleteProfiles.Add(profile);

        await AttachDefaultDraftSnapshotAsync(site, cancellationToken);
        return MapToDto(site);
    }

    /// <summary>Resolve the authenticated caller's domain user, provisioning just-in-time (see <see cref="UserProvisioner"/>).</summary>
    protected Task<User> GetOrCreateUserAsync(string email, string authProviderId, CancellationToken cancellationToken)
        => _userProvisioner.GetOrCreateAsync(email, authProviderId, cancellationToken);

    protected SiteDto MapToDto(Site site) => new()
    {
        Id = site.Id,
        Slug = site.Slug,
        DisplayName = site.DisplayName,
        DefaultLocale = Locale.FromId(site.DefaultLocaleId).ToDto()
    };

    private async Task AttachDefaultDraftSnapshotAsync(Site site, CancellationToken cancellationToken)
    {
        var theme = await _themes.GetActiveByNameAsync("Classic", cancellationToken)
            ?? throw new InvalidOperationException("Classic theme not found"); // system/seed failure → 500

        var snapshot = new SiteSnapshot
        {
            SiteId = site.Id,
            ThemeId = theme.Id,
            Layout = CreateDefaultLayout(),
            GlobalSettings = new GlobalSettings { AccentColor = "#ffd700", FontFamily = "Inter" },
        };
        _siteSnapshots.Add(snapshot);
        site.CurrentDraftSnapshotId = snapshot.Id;
    }

    private static SiteLayout CreateDefaultLayout() => new()
    {
        Sections =
        [
            new SiteSection
            {
                Id = Guid.NewGuid().ToString(),
                Order = 0,
                Data = new HeroSectionData { Headline = new LocalizedText(), Subheading = new LocalizedText() }
            },
            new SiteSection
            {
                Id = Guid.NewGuid().ToString(),
                Order = 1,
                Data = new BioSectionData { Bio = new LocalizedText() }
            }
        ]
    };
}
