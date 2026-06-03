using Microsoft.EntityFrameworkCore;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;
using NextAtlet.Infrastructure.Data;
using NextAtlet.Infrastructure.Services;
using NextAtlet.Infrastructure.Services.SectionRegistry;

namespace NextAtlet.Application.Features.Athletes.Commands;

public class CreateAthleteCommand
{
    private readonly NextAtletDbContext _context;
    private readonly SectionTypeRegistry _sectionRegistry;
    private readonly SanitizationService _sanitization;

    public CreateAthleteCommand(NextAtletDbContext context, SectionTypeRegistry sectionRegistry, SanitizationService sanitization)
    {
        _context = context;
        _sectionRegistry = sectionRegistry;
        _sanitization = sanitization;
    }

    public async Task<AthleteProfile> ExecuteAsync(string email, string authProviderId, string displayName, string slug, DateTime dateOfBirth, string defaultLocaleId, string? guardianEmail = null)
    {
        // Check if profile already exists
        var existingProfile = await _context.AthleteProfiles.FirstOrDefaultAsync(ap => ap.Slug == slug);
        if (existingProfile != null)
            throw new InvalidOperationException($"Slug '{slug}' is already taken");

        // Reserved slug words
        var reservedSlugs = new[] { "admin", "api", "about", "contact", "terms", "privacy", "login", "signup", "settings", "dashboard" };
        if (reservedSlugs.Contains(slug.ToLower()))
            throw new InvalidOperationException($"Slug '{slug}' is reserved");

        // Determine if minor: IsMinor = DateOfBirth + 18 years is in the future
        var isMinor = dateOfBirth.AddYears(18) > DateTime.UtcNow;

        // If minor, guardian email is required
        if (isMinor && string.IsNullOrWhiteSpace(guardianEmail))
            throw new InvalidOperationException("Guardian email is required for minors");

        // Create or get user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.AuthProviderId == authProviderId);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                AuthProviderId = authProviderId,
            };
            _context.Users.Add(user);
        }

        // Create profile
        var profile = new AthleteProfile
        {
            Slug = slug.ToLower(),
            DisplayName = displayName,
            SportId = "judo",
            DateOfBirth = DateOnly.FromDateTime(dateOfBirth),
            DefaultLocaleId = defaultLocaleId,
            VisibilityStateId = "public"
        };
        _context.AthleteProfiles.Add(profile);

        // Create AthleteOwner profile login
        var ownerLogin = ProfileLogin.CreateOwner(user.Id, profile.Id);
        _context.ProfileLogins.Add(ownerLogin);

        // If minor, create pending guardian link
        if (isMinor)
        {
            var guardianUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == guardianEmail);
            if (guardianUser == null)
            {
                // Create a placeholder user for the invited guardian (Status = Pending)
                guardianUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = guardianEmail, // is never null or empty due to validation above
                    AuthProviderId = $"pending-{Guid.NewGuid()}", // Placeholder; will be updated when guardian signs up
                };
                _context.Users.Add(guardianUser);
            }

            // Create a pending ProfileLogin for the guardian
            var guardianLogin = ProfileLogin.CreateGuardian(guardianUser.Id, profile);
            _context.ProfileLogins.Add(guardianLogin);
        }

        // Get the Classic theme
        var theme = await _context.Themes.FirstOrDefaultAsync(t => t.Name == "Classic" && t.IsActive);
        if (theme == null)
            throw new InvalidOperationException("Classic theme not found");

        // Create draft SiteConfig with hero + bio sections
        var siteConfig = new SiteConfig
        {
            AthleteProfileId = profile.Id,
            IsDraft = true,
            ThemeId = theme.Id,
            ThemeVersion = theme.Version,
            Layout = CreateDefaultLayout(),
            GlobalSettings = new GlobalSettings
            {
                AccentColor = "#ffd700",
                FontFamily = "Inter"
            },
            Version = 1
        };
        _context.SiteConfigs.Add(siteConfig);

        await _context.SaveChangesAsync();

        return profile;
    }

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
