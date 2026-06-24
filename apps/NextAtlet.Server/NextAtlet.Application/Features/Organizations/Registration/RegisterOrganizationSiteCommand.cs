using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Extensions;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.strings;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Application.Features.Organizations.Registration
{
    public record RegisterOrganizationSiteCommand
    (
        string AuthProviderId,
        string Email,
        string Slug,
        string DisplayName,
        string PlanTierId,
        string DefaultLocaleId,
        string OrganizationTypeId
    ) : IRequest<Result<SiteDto>>;

    public class RegisterOrganizationSiteCommandHandler : IRequestHandler<RegisterOrganizationSiteCommand, Result<SiteDto>>
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IOrganizationProfileRepository _profiles;
        private readonly IThemeRepository _themes;
        private readonly ISiteSnapshotRepository _siteSnapshots;
        private readonly UserProvisioner _userProvisioner;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterOrganizationSiteCommandHandler(ISiteRepository siteRepository, IOrganizationProfileRepository profiles, IThemeRepository themes, ISiteSnapshotRepository siteSnapshots, UserProvisioner userProvisioner, IUnitOfWork unitOfWork)
        {
            _siteRepository = siteRepository;
            _profiles = profiles;
            _themes = themes;
            _siteSnapshots = siteSnapshots;
            _userProvisioner = userProvisioner;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<SiteDto>> Handle(RegisterOrganizationSiteCommand request, CancellationToken cancellationToken)
        {
            var user = await _userProvisioner.GetOrCreateAsync(request.Email, request.AuthProviderId, cancellationToken);
            var slugExists = await _siteRepository.SlugExistsAsync(request.Slug, cancellationToken);
            if (slugExists)
                return Error.FromCode(ErrorCodes.SlugAlreadyTaken);
            
            var slugReserved = Strings.ReservedSlugs.Contains(request.Slug, StringComparer.OrdinalIgnoreCase);
            if (slugReserved)
                return Error.FromCode(ErrorCodes.SlugReserved);

            var site = new Site
            {
                Slug = request.Slug,
                DisplayName = request.DisplayName,
                DefaultLocaleId = request.DefaultLocaleId,
                SiteTypeId = SiteType.Organization.Id,
            };
            _siteRepository.Add(site);

            var profile = new OrganizationProfile
            {
                SiteId = site.Id,
                OrganizationTypeId = request.OrganizationTypeId,
                OrganizationTierId = request.PlanTierId,
            };
            _profiles.Add(profile);

            await AttachDefaultDraftSnapshotAsync(site, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDto(site);
        }

        protected SiteDto MapToDto(Site site) => new()
        {
            Id = site.Id,
            Slug = site.Slug,
            DisplayName = site.DisplayName,
            DefaultLocale = Locale.FromId(site.DefaultLocaleId).ToDto(),
            VisibilityState = VisibilityStates.FromId(site.VisibilityStateId).ToDto()

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
                Id = Guid.NewGuid(),
                Order = 0,
                Data = new HeroSectionData { Headline = new LocalizedText(), Subheading = new LocalizedText() }
            },
            new SiteSection
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Data = new BioSectionData { Bio = new LocalizedText() }
            }
            ]
        };
    }
}


