using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Mapping;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Features.Athletes.Commands;

public record EditDraftSiteConfigCommand(
    Guid AthleteProfileId,
    SiteLayout Layout,
    GlobalSettings? GlobalSettings,
    int ExpectedVersion) : IRequest<SiteConfigDto>;

/// <summary>
/// Orchestrates a draft-config update: optimistic concurrency check, theme + section
/// validation, sanitization, version bump. Reads/writes via repositories, commits once.
/// </summary>
public class EditDraftSiteConfigCommandHandler : IRequestHandler<EditDraftSiteConfigCommand, SiteConfigDto>
{
    private readonly IAthleteProfileRepository _profiles;
    private readonly ISiteConfigRepository _siteConfigs;
    private readonly IThemeRepository _themes;
    private readonly ISectionTypeRegistry _sectionRegistry;
    private readonly ISanitizationService _sanitization;
    private readonly IUnitOfWork _unitOfWork;

    public EditDraftSiteConfigCommandHandler(
        IAthleteProfileRepository profiles,
        ISiteConfigRepository siteConfigs,
        IThemeRepository themes,
        ISectionTypeRegistry sectionRegistry,
        ISanitizationService sanitization,
        IUnitOfWork unitOfWork)
    {
        _profiles = profiles;
        _siteConfigs = siteConfigs;
        _themes = themes;
        _sectionRegistry = sectionRegistry;
        _sanitization = sanitization;
        _unitOfWork = unitOfWork;
    }

    public async Task<SiteConfigDto> Handle(EditDraftSiteConfigCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.AthleteProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.ProfileNotFound, request.AthleteProfileId);

        var siteConfig = await _siteConfigs.GetDraftByProfileIdAsync(request.AthleteProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.DraftConfigNotFound, request.AthleteProfileId);

        // Optimistic concurrency check
        if (siteConfig.Version != request.ExpectedVersion)
            throw new DomainException(ErrorCodes.DraftVersionConflict, request.ExpectedVersion, siteConfig.Version);

        // System/infra: the config references a theme that should exist — not a user error.
        var theme = await _themes.GetByIdAsync(siteConfig.ThemeId, cancellationToken)
            ?? throw new InvalidOperationException($"Theme {siteConfig.ThemeId} not found");

        // Validate against theme + per-type business rules (shape is guaranteed by the type)
        ValidateLayout(request.Layout, theme);

        // Sanitize free-text in place, then persist
        siteConfig.Layout = _sanitization.SanitizeLayout(request.Layout);
        if (request.GlobalSettings != null)
            siteConfig.GlobalSettings = request.GlobalSettings;

        siteConfig.Version++;
        siteConfig.SetUpdated();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SiteConfigMapper.ToDto(siteConfig);
    }

    private void ValidateLayout(SiteLayout layout, Theme theme)
    {
        for (var i = 0; i < layout.Sections.Count; i++)
        {
            var data = layout.Sections[i].Data;
            var typeKey = data.TypeKey;

            if (!theme.Manifest.SupportedSectionTypes.Contains(typeKey) || !_sectionRegistry.IsSupported(typeKey))
                throw new DomainException(ErrorCodes.SectionTypeNotSupported, i, typeKey);

            var validationResult = _sectionRegistry.Validate(data);
            if (!validationResult.IsValid)
                throw new DomainException(ErrorCodes.SectionValidationFailed, i, string.Join("; ", validationResult.Errors));
        }
    }
}
