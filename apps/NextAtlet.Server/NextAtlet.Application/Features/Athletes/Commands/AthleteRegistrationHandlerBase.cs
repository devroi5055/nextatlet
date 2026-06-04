using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Extensions;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;
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

    protected readonly IUserRepository Users;
    protected readonly IAthleteProfileRepository Profiles;
    protected readonly IProfileLoginRepository Logins;
    protected readonly IThemeRepository Themes;
    protected readonly ISiteConfigRepository SiteConfigs;
    protected readonly IUnitOfWork UnitOfWork;

    protected AthleteRegistrationHandlerBase(
        IUserRepository users,
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IThemeRepository themes,
        ISiteConfigRepository siteConfigs,
        IUnitOfWork unitOfWork)
    {
        Users = users;
        Profiles = profiles;
        Logins = logins;
        Themes = themes;
        SiteConfigs = siteConfigs;
        UnitOfWork = unitOfWork;
    }

    // IsMinor is computed from DateOfBirth; never stored.
    protected static bool IsMinor(DateTime dateOfBirth) => dateOfBirth.AddYears(18) > DateTime.UtcNow;

    /// <summary>
    /// Slug validation + the AthleteProfile + its default draft SiteConfig. Returns the tracked
    /// profile with NO logins attached — the caller attaches owner/guardian logins per its flow.
    /// </summary>
    protected async Task<AthleteProfile> CreateAthleteProfileCoreAsync(
        string slug, string displayName, DateTime dateOfBirth, string defaultLocaleId, CancellationToken cancellationToken)
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
            VisibilityStateId = "public"
        };
        Profiles.Add(profile);

        await AttachDefaultDraftSiteConfigAsync(profile, cancellationToken);
        return profile;
    }

    /// <summary>
    /// Resolve the authenticated caller's domain user, provisioning just-in-time. If no row matches
    /// the subject yet, an invited user (e.g. a guardian) may already exist by email with no subject —
    /// link this login to it (Email is unique) instead of creating a duplicate. Binding the subject is
    /// identity reconciliation only; it does NOT grant guardianship (that stays an explicit accept).
    /// </summary>
    protected async Task<User> GetOrCreateUserAsync(string email, string authProviderId, CancellationToken cancellationToken)
    {
        var bySubject = await Users.GetByAuthProviderIdAsync(authProviderId, cancellationToken);
        if (bySubject is not null)
            return bySubject;

        var byEmail = await Users.GetByEmailAsync(email, cancellationToken);
        if (byEmail is not null)
        {
            byEmail.AuthProviderId ??= authProviderId; // claim the unclaimed (invited) row
            return byEmail;
        }

        var user = new User { Email = email, AuthProviderId = authProviderId };
        Users.Add(user);
        return user;
    }

    /// <summary>Invited (not-yet-authenticated) user: looked up by email, created unclaimed (no sub).</summary>
    protected async Task<User> GetOrCreatePendingUserAsync(string email, CancellationToken cancellationToken)
    {
        var user = await Users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            user = new User { Email = email }; // unclaimed: AuthProviderId == null
            Users.Add(user);
        }
        return user;
    }

    protected static AthleteProfileDto MapToDto(AthleteProfile profile) => new()
    {
        Id = profile.Id,
        Slug = profile.Slug,
        DisplayName = profile.DisplayName,
        DateOfBirth = profile.DateOfBirth,
        IsMinor = profile.IsMinor,
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
