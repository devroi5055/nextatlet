using MediatR;
using NextAtlet.Application.Abstractions.Identity;
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
/// Registers a new athlete. The owner is the authenticated caller (identity from the token),
/// so the request carries profile data only — never the owner's identity.
/// </summary>
public record RegisterAthleteProfileCommand(
    string DisplayName,
    string Slug,
    DateTime DateOfBirth,
    string DefaultLocaleId,
    string? GuardianEmail = null) : IRequest<AthleteProfileDto>;

/// <summary>
/// Orchestrates athlete onboarding: resolve/provision the owner user, create the profile +
/// owner login, optionally invite a guardian (minors), and seed the default draft SiteConfig.
/// Coordinates repositories and commits once via the unit of work — it never touches DbContext.
/// </summary>
public class RegisterAthleteProfileCommandHandler : IRequestHandler<RegisterAthleteProfileCommand, AthleteProfileDto>
{
    private static readonly string[] ReservedSlugs =
        ["admin", "api", "about", "contact", "terms", "privacy", "login", "signup", "settings", "dashboard"];

    private readonly ICurrentUserContext _currentUser;
    private readonly IUserRepository _users;
    private readonly IAthleteProfileRepository _profiles;
    private readonly IProfileLoginRepository _logins;
    private readonly IThemeRepository _themes;
    private readonly ISiteConfigRepository _siteConfigs;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterAthleteProfileCommandHandler(
        ICurrentUserContext currentUser,
        IUserRepository users,
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IThemeRepository themes,
        ISiteConfigRepository siteConfigs,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _users = users;
        _profiles = profiles;
        _logins = logins;
        _themes = themes;
        _siteConfigs = siteConfigs;
        _unitOfWork = unitOfWork;
    }

    public async Task<AthleteProfileDto> Handle(RegisterAthleteProfileCommand request, CancellationToken cancellationToken)
    {
        var slug = NormalizeSlug(request.Slug);
        await EnsureSlugIsAvailableAsync(slug, cancellationToken);

        var isMinor = IsMinor(request.DateOfBirth);
        EnsureGuardianProvidedForMinor(isMinor, request.GuardianEmail);

        var owner = await ResolveOwnerUserAsync(cancellationToken);

        // One profile per owner — registration is not repeatable.
        if (await _profiles.GetOwnedByUserIdAsync(owner.Id, cancellationToken) is not null)
            throw new DomainException(ErrorCodes.ProfileAlreadyExists);

        var athleteProfile = AddProfile(request, slug);
        _logins.Add(ProfileLogin.CreateOwner(owner.Id, athleteProfile.Id));

        if (isMinor) await InviteGuardianAsync(request.GuardianEmail, athleteProfile, cancellationToken);

        await AddDefaultDraftSiteConfigAsync(athleteProfile, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(athleteProfile);
    }

    private static string NormalizeSlug(string slug) => slug.ToLowerInvariant();

    private async Task EnsureSlugIsAvailableAsync(string slug, CancellationToken cancellationToken)
    {
        if (await _profiles.SlugExistsAsync(slug, cancellationToken))
            throw new DomainException(ErrorCodes.SlugAlreadyTaken, slug);

        if (ReservedSlugs.Contains(slug))
            throw new DomainException(ErrorCodes.SlugReserved, slug);
    }

    // IsMinor is computed from DateOfBirth; never stored.
    private static bool IsMinor(DateTime dateOfBirth) => dateOfBirth.AddYears(18) > DateTime.UtcNow;

    private static void EnsureGuardianProvidedForMinor(bool isMinor, string? guardianEmail)
    {
        if (isMinor && string.IsNullOrWhiteSpace(guardianEmail))
            throw new DomainException(ErrorCodes.GuardianEmailRequired);
    }

    /// <summary>
    /// The owner is the authenticated caller — identity comes from the token, never the request body.
    /// Provisions the domain user just-in-time if this is their first action.
    /// </summary>
    private async Task<User> ResolveOwnerUserAsync(CancellationToken cancellationToken)
    {
        var existing = await _users.GetByAuthProviderIdAsync(_currentUser.AuthProviderId, cancellationToken);
        if (existing != null)
            return existing;

        var owner = new User { Email = _currentUser.Email, AuthProviderId = _currentUser.AuthProviderId };
        _users.Add(owner);
        return owner;
    }

    private AthleteProfile AddProfile(RegisterAthleteProfileCommand request, string slug)
    {
        var profile = new AthleteProfile
        {
            Slug = slug,
            DisplayName = request.DisplayName,
            SportId = "judo",
            DateOfBirth = DateOnly.FromDateTime(request.DateOfBirth),
            DefaultLocaleId = request.DefaultLocaleId,
            VisibilityStateId = "public"
        };
        _profiles.Add(profile);
        return profile;
    }

    /// <summary>
    /// Links a guardian. If they have no account yet, create an <em>unclaimed</em> user
    /// (no AuthProviderId) — claimed when they first sign in. The login starts Pending.
    /// </summary>
    private async Task InviteGuardianAsync(string? guardianEmail, AthleteProfile profile, CancellationToken cancellationToken)
    {
        if (guardianEmail == null) throw new ArgumentNullException("Guardian Email can not be empty");
        var guardian = await _users.GetByEmailAsync(guardianEmail, cancellationToken);
        if (guardian == null)
        {
            guardian = new User { Email = guardianEmail }; // unclaimed: AuthProviderId == null
            _users.Add(guardian);
        }

        _logins.Add(ProfileLogin.CreateGuardian(guardian.Id, profile));
    }

    private async Task AddDefaultDraftSiteConfigAsync(AthleteProfile profile, CancellationToken cancellationToken)
    {
        var theme = await _themes.GetActiveByNameAsync("Classic", cancellationToken)
            ?? throw new InvalidOperationException("Classic theme not found");

        _siteConfigs.Add(new SiteConfig
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

    private static AthleteProfileDto MapToDto(AthleteProfile profile) => new()
    {
        Id = profile.Id,
        Slug = profile.Slug,
        DisplayName = profile.DisplayName,
        DateOfBirth = profile.DateOfBirth,
        IsMinor = profile.IsMinor,
        DefaultLocale = Locale.FromId(profile.DefaultLocaleId).ToDto()
    };

    private static SiteLayout CreateDefaultLayout() => new()
    {
        Sections =
        [
            new SiteSection
            {
                Id = Guid.NewGuid().ToString(),
                Order = 0,
                Data = new HeroSectionData
                {
                    Headline = new LocalizedText(),
                    Subheading = new LocalizedText()
                }
            },
            new SiteSection
            {
                Id = Guid.NewGuid().ToString(),
                Order = 1,
                Data = new BioSectionData
                {
                    Bio = new LocalizedText()
                }
            }
        ]
    };
}
