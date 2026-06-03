using Microsoft.EntityFrameworkCore;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.ValueObjects;
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

    public async Task<SiteConfig> ExecuteAsync(Guid athleteProfileId, SiteLayout layout, GlobalSettings? globalSettings, int expectedVersion)
    {
        var profile = await _context.AthleteProfiles.FindAsync(athleteProfileId);
        if (profile == null)
            throw new InvalidOperationException($"Profile {athleteProfileId} not found");

        var siteConfig = await _context.SiteConfigs
            .FirstOrDefaultAsync(sc => sc.AthleteProfileId == athleteProfileId && sc.IsDraft);

        if (siteConfig == null)
            throw new InvalidOperationException($"Draft config not found for profile {athleteProfileId}");

        // Optimistic concurrency check
        if (siteConfig.Version != expectedVersion)
            throw new InvalidOperationException($"Concurrency conflict: expected version {expectedVersion}, but found {siteConfig.Version}");

        // Theme constrains which section types are allowed
        var theme = await _context.Themes.FindAsync(siteConfig.ThemeId);
        if (theme == null)
            throw new InvalidOperationException($"Theme {siteConfig.ThemeId} not found");

        // Validate against theme + per-type business rules (shape is guaranteed by the type)
        ValidateLayout(layout, theme);

        // Sanitize free-text in place, then persist
        siteConfig.Layout = _sanitization.SanitizeLayout(layout);
        if (globalSettings != null)
            siteConfig.GlobalSettings = globalSettings;

        siteConfig.Version++;
        siteConfig.SetUpdated();

        // EF change tracking + the jsonb ValueComparer pick up the reassigned Layout/GlobalSettings.
        await _context.SaveChangesAsync();

        return siteConfig;
    }

    private void ValidateLayout(SiteLayout layout, Theme theme)
    {
        for (var i = 0; i < layout.Sections.Count; i++)
        {
            var data = layout.Sections[i].Data;
            var typeKey = data.TypeKey;

            if (!theme.Manifest.SupportedSectionTypes.Contains(typeKey))
                throw new InvalidOperationException($"Section [{i}] type '{typeKey}' is not supported by the theme");

            var validator = _sectionRegistry.GetValidator(typeKey);
            if (validator == null)
                throw new InvalidOperationException($"Section [{i}] type '{typeKey}' is not registered");

            var validationResult = validator.Validate(data);
            if (!validationResult.IsValid)
                throw new InvalidOperationException($"Section [{i}] validation failed: {string.Join("; ", validationResult.Errors)}");
        }
    }
}
