using Microsoft.EntityFrameworkCore;
using NextAtlet.Domain.Entities;
using NextAtlet.Infrastructure.Data;
using NextAtlet.Infrastructure.Services;
using NextAtlet.Infrastructure.Services.SectionRegistry;

namespace NextAtlet.Application.Features.Athletes.Commands;

public class UpdateDraftConfigCommand
{
    private readonly NextAtletDbContext _context;
    private readonly SectionTypeRegistry _sectionRegistry;
    private readonly SanitizationService _sanitization;

    public UpdateDraftConfigCommand(NextAtletDbContext context, SectionTypeRegistry sectionRegistry, SanitizationService sanitization)
    {
        _context = context;
        _sectionRegistry = sectionRegistry;
        _sanitization = sanitization;
    }

    public async Task<SiteConfig> ExecuteAsync(Guid athleteProfileId, Dictionary<string, object> layout, Dictionary<string, object>? globalSettings, int expectedVersion)
    {
        var profile = await _context.AthleteProfiles.FindAsync(athleteProfileId);
        if (profile == null)
            throw new InvalidOperationException($"Profile {athleteProfileId} not found");

        var siteConfig = await _context.SiteConfigs
            .FirstOrDefaultAsync(sc => sc.AthleteProfileId == athleteProfileId && sc.State == "Draft");

        if (siteConfig == null)
            throw new InvalidOperationException($"Draft config not found for profile {athleteProfileId}");

        // Optimistic concurrency check
        if (siteConfig.Version != expectedVersion)
            throw new InvalidOperationException($"Concurrency conflict: expected version {expectedVersion}, but found {siteConfig.Version}");

        // Get theme to validate section types
        var theme = await _context.Themes.FindAsync(siteConfig.ThemeId);
        if (theme == null)
            throw new InvalidOperationException($"Theme {siteConfig.ThemeId} not found");

        // Validate the layout
        ValidateLayout(layout, theme);

        // Sanitize all text fields
        var sanitizedLayout = _sanitization.SanitizeLayout(layout);

        // Update config
        siteConfig.Layout = sanitizedLayout;
        if (globalSettings != null)
            siteConfig.GlobalSettings = globalSettings;

        siteConfig.Version++;
        siteConfig.UpdatedUtc = DateTime.UtcNow;

        _context.SiteConfigs.Update(siteConfig);
        await _context.SaveChangesAsync();

        return siteConfig;
    }

    private void ValidateLayout(Dictionary<string, object> layout, Theme theme)
    {
        if (!layout.TryGetValue("sections", out var sectionsObj))
            throw new InvalidOperationException("Layout must contain 'sections' key");

        if (sectionsObj is not System.Collections.IEnumerable sections)
            throw new InvalidOperationException("Layout.sections must be an array");

        // Get supported section types from theme manifest
        var supportedTypes = ExtractSupportedSectionTypes(theme.Manifest);

        int sectionIndex = 0;
        foreach (var section in sections)
        {
            if (section is not Dictionary<string, object> sectionDict)
                throw new InvalidOperationException($"Section [{sectionIndex}] must be an object");

            if (!sectionDict.TryGetValue("type", out var typeObj) || typeObj is not string sectionType)
                throw new InvalidOperationException($"Section [{sectionIndex}] must have a 'type' string");

            // Check if section type is supported by theme
            if (!supportedTypes.Contains(sectionType))
                throw new InvalidOperationException($"Section type '{sectionType}' is not supported by the theme");

            // Check if section type is registered
            if (!_sectionRegistry.IsSupported(sectionType))
                throw new InvalidOperationException($"Section type '{sectionType}' is not registered");

            // Validate section using the registry
            var sectionToValidate = new Section
            {
                Type = sectionType,
                Data = sectionDict.TryGetValue("data", out var dataObj) && dataObj is Dictionary<string, object> dict ? dict : new Dictionary<string, object>()
            };

            var validator = _sectionRegistry.GetValidator(sectionType);
            if (validator != null)
            {
                var validationResult = validator.Validate(sectionToValidate);
                if (!validationResult.IsValid)
                    throw new InvalidOperationException($"Section [{sectionIndex}] validation failed: {string.Join("; ", validationResult.Errors)}");
            }

            sectionIndex++;
        }
    }

    private List<string> ExtractSupportedSectionTypes(Dictionary<string, object> manifest)
    {
        if (manifest.TryGetValue("supportedSections", out var supported) && supported is System.Collections.IEnumerable sections)
        {
            return sections.Cast<object>().OfType<string>().ToList();
        }

        return [];
    }
}
