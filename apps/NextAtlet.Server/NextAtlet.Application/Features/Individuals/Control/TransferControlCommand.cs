using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Policies;

namespace NextAtlet.Application.Features.Individuals.Control;

/// <summary>
/// The only way <c>ControlMode</c> changes after creation. Only the <b>current controller</b> may
/// initiate: a ReadOnly party cannot grab control. Guardian→Athlete is age-gated (athlete must be ≥13)
/// and needs an athlete login to receive it; Athlete→Guardian is voluntary at any age but needs a
/// guardian login. Transfer resets the receiving side to its <b>non-shared</b> mode — handing over
/// control clears any prior collaboration; the new controller can re-enable it. No receiver
/// confirmation in v1 (initiator decides). Identity comes from the token, never the body.
/// </summary>
public record TransferControlCommand(
    Guid ProfileId,
    string CallerAuthProviderId,
    string TransferTo) : IRequest<Result>; // "athlete" | "guardian"

public class TransferControlCommandHandler : IRequestHandler<TransferControlCommand, Result>
{
    public const string ToAthlete = "athlete";
    public const string ToGuardian = "guardian";

    private readonly ISiteRepository _sites;
    private readonly IIndividualProfileRepository _profiles;
    private readonly ISiteLoginRepository _logins;
    private readonly IUserRepository _users;
    private readonly PermissionResolver _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public TransferControlCommandHandler(
        ISiteRepository sites,
        ISiteLoginRepository logins,
        IUserRepository users,
        PermissionResolver permissions,
        IUnitOfWork unitOfWork,
        IClock clock,
        IIndividualProfileRepository profiles)
    {
        _sites = sites;
        _logins = logins;
        _users = users;
        _permissions = permissions;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _profiles = profiles;
    }

    public async Task<Result> Handle(TransferControlCommand request, CancellationToken cancellationToken)
    {
        if (request.TransferTo is not (ToAthlete or ToGuardian))
            return Error.FromCode(ErrorCodes.TransferTargetInvalid);

        var profile = await _profiles.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
            return Error.FromCode(ErrorCodes.IndividualProfileNotFound);

        var caller = await _users.GetByAuthProviderIdAsync(request.CallerAuthProviderId, cancellationToken);
        if (caller is null)
            throw new InvalidOperationException("Authenticated user needs DB row");

        var login = await _logins.GetActiveLoginAsync(caller.Id, profile.SiteId, cancellationToken);
        // Only the current controller may initiate a transfer.
        if (login is null || !_permissions.IsController(login, profile))
            return Error.FromCode(ErrorCodes.NotAuthorized);

        if (request.TransferTo == ToAthlete)
        {
            // Age gate: control can only go to an athlete who is at least 13.
            if (AgePolicy.BandToday(profile.DateOfBirth, _clock.UtcNow) == AgeBand.BelowMinimum)
                return Error.FromCode(ErrorCodes.AthleteTooYoungForControl);

            // Can't hand control to a ghost — an athlete owner login must exist.
            if (!await _logins.HasActiveOwnerLoginAsync(request.ProfileId, cancellationToken))
                return Error.FromCode(ErrorCodes.NoAthleteLoginExists);

            profile.ControlModeId = ControlModes.AthleteControlled.Id;
        }
        else
        {
            if (!await _logins.HasActiveGuardianLoginAsync(request.ProfileId, cancellationToken))
                return Error.FromCode(ErrorCodes.NoGuardianLoginExists);

            profile.ControlModeId = ControlModes.GuardianControlled.Id;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
