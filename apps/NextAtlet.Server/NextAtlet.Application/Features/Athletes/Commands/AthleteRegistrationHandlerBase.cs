using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Extensions;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Shared mechanics for the two registration flows (self vs guardian-creates-child). Owns everything
/// identical between them — slug validation, profile + default draft SiteConfig creation, and user
/// get-or-create. Each concrete handler owns only its login-attachment + flow-specific rules.
/// Repositories are protected and shared; the concrete handler calls <c>SaveChangesAsync</c> once.
/// </summary>
public abstract class AthleteRegistrationHandlerBase
{
    protected static readonly string[] ReservedSlugs =
        ["admin", "api", "about", "contact", "terms", "privacy", "login", "signup", "settings", "dashboard"];

    protected readonly IAthleteProfileRepository Profiles;
    protected readonly IProfileLoginRepository Logins;
    protected readonly IThemeRepository Themes;
    protected readonly ISiteConfigRepository SiteConfigs;
    protected readonly UserProvisioner UserProvisioner;
    protected readonly InvitationIssuer Inviter;
    protected readonly IUnitOfWork UnitOfWork;

    protected AthleteRegistrationHandlerBase(
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IThemeRepository themes,
        ISiteConfigRepository siteConfigs,
        UserProvisioner userProvisioner,
        InvitationIssuer inviter,
        IUnitOfWork unitOfWork)
    {
        Profiles = profiles;
        Logins = logins;
        Themes = themes;
        SiteConfigs = siteConfigs;
        UserProvisioner = userProvisioner;
        Inviter = inviter;
        UnitOfWork = unitOfWork;
    }

    /// <summary>
    /// Slug validation + the AthleteProfile (with its explicit <paramref name="controlMode"/>) + its
    /// default draft SiteConfig. Returns the tracked profile with NO logins attached — the caller
    /// attaches owner/guardian logins per its flow.
    /// </summary>
    protected async Task<AthleteProfile> CreateAthleteProfileCoreAsync(
        string slug, string displayName, DateTime dateOfBirth, string defaultLocaleId, ControlMode controlMode, CancellationToken cancellationToken)
    {
        slug = slug.ToLowerInvariant();

        if (await Profiles.SlugExistsAsync(slug, cancellationToken))
            throw new DomainException(ErrorCodes.SlugAlreadyTaken, slug);
        if (ReservedSlugs.Contains(slug))
            throw new DomainException(ErrorCodes.SlugReserved, slug);

        var profile = new AthleteProfile
        {
            Slug = slug,
            DisplayName = displayName,
            SportId = "judo",
            DateOfBirth = DateOnly.FromDateTime(dateOfBirth),
            DefaultLocaleId = defaultLocaleId,
            VisibilityStateId = "public",
            ControlMode = controlMode
        };
        Profiles.Add(profile);

        await AttachDefaultDraftSiteConfigAsync(profile, cancellationToken);
        return profile;
    }

    /// <summary>Resolve the authenticated caller's domain user, provisioning just-in-time (see <see cref="UserProvisioner"/>).</summary>
    protected Task<User> GetOrCreateUserAsync(string email, string authProviderId, CancellationToken cancellationToken)
        => UserProvisioner.GetOrCreateAsync(email, authProviderId, cancellationToken);

    protected static AthleteProfileDto MapToDto(AthleteProfile profile) => new()
    {
        Id = profile.Id,
        Slug = profile.Slug,
        DisplayName = profile.DisplayName,
        DateOfBirth = profile.DateOfBirth,
        IsMinor = profile.IsMinor,
        ControlMode = profile.ControlMode,
        DefaultLocale = Locale.FromId(profile.DefaultLocaleId).ToDto()
    };

    private async Task AttachDefaultDraftSiteConfigAsync(AthleteProfile profile, CancellationToken cancellationToken)
    {
        var theme = await Themes.GetActiveByNameAsync("Classic", cancellationToken)
            ?? throw new InvalidOperationException("Classic theme not found"); // system/seed failure → 500

        SiteConfigs.Add(new SiteConfig
        {
            AthleteProfileId = profile.Id,
            IsDraft = true,
            ThemeId = theme.Id,
            ThemeVersion = theme.Version,
            Layout = CreateDefaultLayout(),
            GlobalSettings = new GlobalSettings { AccentColor = "#ffd700", FontFamily = "Inter" },
            Version = 1
        });
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
