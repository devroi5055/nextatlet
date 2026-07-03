using MediatR;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Mapping;
using NextAtlet.Application.Contracts.Sites.Response;
using NextAtlet.Application.Abstractions.Persistence;
using System.Reflection.Metadata.Ecma335;
using NextAtlet.Application.Common.Results;

namespace NextAtlet.Application.Features.Sites;

public record GetDraftAthleteSiteSnapshotQuery(Guid SiteId) : IRequest<Result<SiteSnapshotResponse>>;

public class GetDraftAthleteSiteSnapshotQueryHandler : IRequestHandler<GetDraftAthleteSiteSnapshotQuery, Result<SiteSnapshotResponse>>
{
    private readonly ISiteSnapshotRepository _siteSnapshots;

    public GetDraftAthleteSiteSnapshotQueryHandler(ISiteSnapshotRepository siteSnapshots) => _siteSnapshots = siteSnapshots;

    public async Task<Result<SiteSnapshotResponse>> Handle(GetDraftAthleteSiteSnapshotQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await _siteSnapshots.GetCurrentDraftBySiteIdAsync(request.SiteId, cancellationToken);
        if (snapshot is null)
            return Error.FromCode(ErrorCodes.DraftConfigNotFound);

        return SiteSnapshotMapper.ToDto(snapshot);
    }
}
