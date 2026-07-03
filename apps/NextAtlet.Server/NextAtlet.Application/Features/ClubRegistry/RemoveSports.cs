using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using System.Collections;

namespace NextAtlet.Application.Features.ClubRegistry.Commands;

public record RemoveSportsCommand(Guid id, List<string> sportIds) : IRequest<Result<IEnumerable<string>>>;

public class RemoveSportsCommandHandler : IRequestHandler<RemoveSportsCommand, Result<IEnumerable<string>>>
{
    private readonly IClubRepository _clubs;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveSportsCommandHandler(
        IClubRepository clubs,
        IUnitOfWork unitOfWork)
    {
        _clubs = clubs;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<string>>> Handle(RemoveSportsCommand request, CancellationToken ct)
    {
        var club = await _clubs.GetClubByIdAsync(request.id, ct);
        if (club == null)
            return Error.FromCode(ErrorCodes.ClubNotFound);

        var removed = club.RemoveSports(request.sportIds);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<IEnumerable<string>>.Success(removed);
    }
}
