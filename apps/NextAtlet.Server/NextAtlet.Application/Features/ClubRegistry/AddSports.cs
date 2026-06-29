using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;

namespace NextAtlet.Application.Features.ClubRegistry.Commands;

public record AddSportsCommand(Guid id, List<string> sportIds) : IRequest<Result<IEnumerable<string>>>;

public class AddSportsCommandHandler : IRequestHandler<AddSportsCommand, Result<IEnumerable<string>>>
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

    public async Task<Result<IEnumerable<string>>> Handle(AddSportsCommand request, CancellationToken ct)
    {
        var club = await _clubs.GetClubByIdAsync(request.id, ct);
        if (club == null)
            return Error.FromCode(ErrorCodes.ClubNotFound);

        var added = club.AddSports(request.sportIds);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<IEnumerable<string>>.Success(added);
    }
}
