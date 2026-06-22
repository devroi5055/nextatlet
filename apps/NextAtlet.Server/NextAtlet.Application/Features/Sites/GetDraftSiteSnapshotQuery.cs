using MediatR;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Mapping;
using NextAtlet.Application.Interfaces.Repositories;

namespace NextAtlet.Application.Features.IndividualSites.Queries;

public record GetDraftAthleteSiteSnapshotQuery(Guid SiteId) : IRequest<SiteSnapshotDto>;

public class GetDraftAthleteSiteSnapshotQueryHandler : IRequestHandler<GetDraftAthleteSiteSnapshotQuery, SiteSnapshotDto>
{
    private readonly ISiteSnapshotRepository _siteSnapshots;

    public GetDraftAthleteSiteSnapshotQueryHandler(ISiteSnapshotRepository siteSnapshots) => _siteSnapshots = siteSnapshots;

    public async Task<SiteSnapshotDto> Handle(GetDraftAthleteSiteSnapshotQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await _siteSnapshots.GetCurrentDraftBySiteIdAsync(request.SiteId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.DraftConfigNotFound, request.SiteId);

        return SiteSnapshotMapper.ToDto(snapshot);
    }
}
