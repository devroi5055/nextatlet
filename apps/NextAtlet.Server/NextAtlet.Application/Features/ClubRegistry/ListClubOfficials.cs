using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Domain.Entities.ClubRegistry;

namespace NextAtlet.Application.Features.ClubRegistry.Commands;

public record ListClubOfficialsCommand(Guid ClubId) : IRequest<Result<List<ClubOfficial>>>;

public class ListClubOfficialsCommandHandler : IRequestHandler<ListClubOfficialsCommand, Result<List<ClubOfficial>>>
{
    private readonly IClubRepository _clubs;
    public ListClubOfficialsCommandHandler(
        IClubRepository clubs)
    {
        _clubs = clubs;
    }

    public async Task<Result<List<ClubOfficial>>> Handle(ListClubOfficialsCommand request, CancellationToken ct)
    {
        var club = await _clubs.GetClubByIdAsync(request.ClubId, ct);
        if (club == null)
            return Error.FromCode(ErrorCodes.ClubNotFound);

        return Result<List<ClubOfficial>>.Success(club.Officials.ToList());
    }
}
