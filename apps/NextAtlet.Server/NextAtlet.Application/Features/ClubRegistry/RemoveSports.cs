using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Interfaces.Repositories;

namespace NextAtlet.Application.Features.Clubs.Commands;

public record RemoveSportsCommand(string id, List<string> sportIds) : IRequest<Unit>;

public class RemoveSportsCommandHandler : IRequestHandler<RemoveSportsCommand, Unit>
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

    public async Task<Unit> Handle(RemoveSportsCommand request, CancellationToken ct)
    {
        var club = await _clubs.GetByIdAsync(request.id, ct);
        if (club == null)
            throw new DomainException(ErrorCodes.ClubNotFound);

        club.RemoveSports(request.sportIds);

        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
