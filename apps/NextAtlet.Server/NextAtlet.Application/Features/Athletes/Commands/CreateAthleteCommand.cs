using Microsoft.EntityFrameworkCore;
using NextAtlet.Domain.Entities;
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

    public async Task<AthleteProfile> ExecuteAsync(string email, string authProviderId, string displayName, string slug, DateTime dateOfBirth, string defaultLocale, string? guardianEmail = null)
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
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            _context.Users.Add(user);
        }

        // Create profile
        var profile = new AthleteProfile
        {
            Id = Guid.NewGuid(),
            Slug = slug.ToLower(),
            DisplayName = displayName,
            Sport = "judo",
            DateOfBirth = dateOfBirth,
            DefaultLocale = defaultLocale,
            VisibilityState = "Public",
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
        _context.AthleteProfiles.Add(profile);

        // Create AthleteOwner profile login
        var ownerLogin = new ProfileLogin
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AthleteProfileId = profile.Id,
            Role = "AthleteOwner",
            Status = "Active",
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
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
                    Email = guardianEmail,
                    AuthProviderId = $"pending-{Guid.NewGuid()}", // Placeholder; will be updated when guardian signs up
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow
                };
                _context.Users.Add(guardianUser);
            }

            var guardianLogin = new ProfileLogin
            {
                Id = Guid.NewGuid(),
                UserId = guardianUser.Id,
                AthleteProfileId = profile.Id,
                Role = "Guardian",
                Permissions = new Dictionary<string, object>
                {
                    { "canEditContent", true },
                    { "canPublish", true },
                    { "canApproveChanges", true },
                    { "canManageMedia", true },
                    { "canManageMemberships", false }
                },
                Status = "Pending", // Pending until guardian accepts
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            _context.ProfileLogins.Add(guardianLogin);
        }

        // Get the Classic theme
        var theme = await _context.Themes.FirstOrDefaultAsync(t => t.Name == "Classic" && t.IsActive);
        if (theme == null)
            throw new InvalidOperationException("Classic theme not found");

        // Create draft SiteConfig with hero + bio sections
        var layout = CreateDefaultLayout();
        var siteConfig = new SiteConfig
        {
            Id = Guid.NewGuid(),
            AthleteProfileId = profile.Id,
            State = "Draft",
            ThemeId = theme.Id,
            ThemeVersion = theme.Version,
            Layout = layout,
            GlobalSettings = new Dictionary<string, object>
            {
                { "colors", new Dictionary<string, object>
                    {
                        { "primary", "#000000" },
                        { "secondary", "#ffffff" },
                        { "accent", "#ffd700" }
                    }
                },
                { "fonts", new Dictionary<string, object>
                    {
                        { "headingFont", "Inter" },
                        { "bodyFont", "Inter" }
                    }
                }
            },
            Version = 1,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
        _context.SiteConfigs.Add(siteConfig);

        await _context.SaveChangesAsync();

        return profile;
    }

    private Dictionary<string, object> CreateDefaultLayout()
    {
        var sections = new List<object>
        {
            new
            {
                id = Guid.NewGuid().ToString(),
                type = "hero",
                order = 0,
                data = new Dictionary<string, object>
                {
                    { "headline", new Dictionary<string, string> { { "da", "" }, { "en", "" } } },
                    { "subheading", new Dictionary<string, string> { { "da", "" }, { "en", "" } } },
                    { "backgroundImageAssetId", (string?)null }
                }
            },
            new
            {
                id = Guid.NewGuid().ToString(),
                type = "bio",
                order = 1,
                data = new Dictionary<string, object>
                {
                    { "bio", new Dictionary<string, string> { { "da", "" }, { "en", "" } } },
                    { "highlightItems", new List<object>() }
                }
            }
        };

        return new Dictionary<string, object> { { "sections", sections } };
    }
}
