using MediatR;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Mapping;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;

namespace NextAtlet.Application.Features.Athletes.Queries;

public record GetDraftAthleteSiteSnapshotQuery(Guid AthleteProfileId) : IRequest<AthleteSiteSnapshotDto>;

public class GetDraftAthleteSiteSnapshotQueryHandler : IRequestHandler<GetDraftAthleteSiteSnapshotQuery, AthleteSiteSnapshotDto>
{
    private readonly IAthleteSiteSnapshotRepository _siteSnapshots;

    public GetDraftAthleteSiteSnapshotQueryHandler(IAthleteSiteSnapshotRepository siteSnapshots) => _siteSnapshots = siteSnapshots;

    public async Task<AthleteSiteSnapshotDto> Handle(GetDraftAthleteSiteSnapshotQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await _siteSnapshots.GetDraftByProfileIdAsync(request.AthleteProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.DraftConfigNotFound, request.AthleteProfileId);

        return AthleteSiteSnapshotMapper.ToDto(snapshot);
    }
}
