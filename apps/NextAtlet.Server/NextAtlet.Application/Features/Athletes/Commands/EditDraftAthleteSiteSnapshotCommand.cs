using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Mapping;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Features.Athletes.Commands;

public record EditDraftAthleteSiteSnapshotCommand(
    Guid AthleteProfileId,
    SiteLayout Layout,
    GlobalSettings? GlobalSettings,
    int ExpectedVersion) : IRequest<AthleteSiteSnapshotDto>;

/// <summary>
/// Replaces the draft snapshot: optimistic concurrency check, theme + section validation,
/// sanitization. Creates a new immutable snapshot and updates the profile's draft pointer.
/// Reads/writes via repositories, commits once.
/// </summary>
public class EditDraftAthleteSiteSnapshotCommandHandler : IRequestHandler<EditDraftAthleteSiteSnapshotCommand, AthleteSiteSnapshotDto>
{
    private readonly IAthleteSiteRepository _profiles;
    private readonly IAthleteSiteSnapshotRepository _siteSnapshots;
    private readonly IThemeRepository _themes;
    private readonly ISectionTypeRegistry _sectionRegistry;
    private readonly ISanitizationService _sanitization;
    //private readonly IPerkResolver _perkResolver; //generate perk resolver
    private readonly IUnitOfWork _unitOfWork;

    public EditDraftAthleteSiteSnapshotCommandHandler(
        IAthleteSiteRepository profiles,
        IAthleteSiteSnapshotRepository siteSnapshots,
        IThemeRepository themes,
        ISectionTypeRegistry sectionRegistry,
        ISanitizationService sanitization,
        IUnitOfWork unitOfWork)
    {
        _profiles = profiles;
        _siteSnapshots = siteSnapshots;
        _themes = themes;
        _sectionRegistry = sectionRegistry;
        _sanitization = sanitization;
        _unitOfWork = unitOfWork;
    }

    public async Task<AthleteSiteSnapshotDto> Handle(EditDraftAthleteSiteSnapshotCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.AthleteProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.ProfileNotFound, request.AthleteProfileId);

        var current = await _siteSnapshots.GetDraftByProfileIdAsync(request.AthleteProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.DraftConfigNotFound, request.AthleteProfileId);

        // Optimistic concurrency check
        if (current.Version != request.ExpectedVersion)
            throw new DomainException(ErrorCodes.DraftVersionConflict, request.ExpectedVersion, current.Version);

        // System/infra: the snapshot references a theme that should exist — not a user error.
        var theme = await _themes.GetByIdAsync(current.ThemeId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.ThemeNotFound, current.ThemeId);

        // Validate against theme + per-type business rules (shape is guaranteed by the type)
        //ValidateLayout(request.Layout, theme);

        // Snapshots are immutable — create a new one with the updated content, then move the draft pointer.
        var updated = new AthleteSiteSnapshot
        {
            AthleteProfileId = request.AthleteProfileId,
            ThemeId = current.ThemeId,
            ThemeVersion = current.ThemeVersion,
            Layout = _sanitization.SanitizeLayout(request.Layout),
            GlobalSettings = request.GlobalSettings ?? current.GlobalSettings,
            Version = current.Version + 1
        };
        _siteSnapshots.Add(updated);
        profile.CurrentDraftSnapshotId = updated.Id;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AthleteSiteSnapshotMapper.ToDto(updated);
    }

    //TODO : NEED PERKRESOLVER
    //private void ValidateLayout(
    //    SiteLayout layout,
    //    Theme theme,
    //    HashSet<string> effectiveCapabilities)  // resolved by PerkResolver, not raw selfTierId
    //{
    //    // 1. Can this athlete select this theme at all?
    //    if (!effectiveCapabilities.Contains($"themes.{theme.MinimumTierId}"))
    //        throw new DomainException(ErrorCodes.ThemeNotPermitted, theme.Name);

    //    for (var i = 0; i < layout.Sections.Count; i++)
    //    {
    //        var typeKey = layout.Sections[i].Data.TypeKey;

    //        // 2. Does the athlete's effective capability allow this section?
    //        if (!effectiveCapabilities.Contains($"sections.{typeKey}"))
    //            throw new DomainException(ErrorCodes.SectionNotPermitted, i, typeKey);

    //        // 3. Does the section type exist in the registry?
    //        if (!_sectionRegistry.IsSupported(typeKey))
    //            throw new DomainException(ErrorCodes.SectionTypeNotSupported, i, typeKey);

    //        // 4. Is the section data valid against its schema?
    //        var validationResult = _sectionRegistry.Validate(layout.Sections[i].Data);
    //        if (!validationResult.IsValid)
    //            throw new DomainException(ErrorCodes.SectionValidationFailed, i,
    //                string.Join("; ", validationResult.Errors));
    //    }
    //}
}
