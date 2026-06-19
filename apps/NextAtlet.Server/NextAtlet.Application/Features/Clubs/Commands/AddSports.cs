using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Interfaces.Repositories;

namespace NextAtlet.Application.Features.Clubs.Commands;

public record AddSportsCommand(string id, List<string> sportIds) : IRequest<Unit>;

public class AddSportsCommandHandler : IRequestHandler<AddSportsCommand, Unit>
{
    private readonly IClubRepository _clubs;
    private readonly IUnitOfWork _unitOfWork;

    public AddSportsCommandHandler(
        IClubRepository clubs,
        IUnitOfWork unitOfWork)
    {
        _clubs = clubs;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddSportsCommand request, CancellationToken ct)
    {
        var club = await _clubs.GetByIdAsync(request.id, ct);
        if (club == null)
            throw new DomainException(ErrorCodes.ClubNotFound);

        club.AddSports(request.sportIds);

        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
