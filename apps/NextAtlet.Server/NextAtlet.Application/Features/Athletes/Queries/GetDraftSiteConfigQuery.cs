using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Mapping;

namespace NextAtlet.Application.Features.Athletes.Queries;

public record GetDraftSiteConfigQuery(Guid AthleteProfileId) : IRequest<SiteConfigDto>;

public class GetDraftSiteConfigQueryHandler : IRequestHandler<GetDraftSiteConfigQuery, SiteConfigDto>
{
    private readonly ISiteConfigRepository _siteConfigs;

    public GetDraftSiteConfigQueryHandler(ISiteConfigRepository siteConfigs) => _siteConfigs = siteConfigs;

    public async Task<SiteConfigDto> Handle(GetDraftSiteConfigQuery request, CancellationToken cancellationToken)
    {
        var siteConfig = await _siteConfigs.GetDraftByProfileIdAsync(request.AthleteProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.DraftConfigNotFound, request.AthleteProfileId);

        return SiteConfigMapper.ToDto(siteConfig);
    }
}
