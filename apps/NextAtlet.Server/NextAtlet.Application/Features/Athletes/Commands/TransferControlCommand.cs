using MediatR;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Policies;

namespace NextAtlet.Application.Features.Athletes.Commands;

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
    string TransferTo) : IRequest<Result<Unit>>; // "athlete" | "guardian"

public class TransferControlCommandHandler : IRequestHandler<TransferControlCommand, Result<Unit>>
{
    public const string ToAthlete = "athlete";
    public const string ToGuardian = "guardian";

    private readonly IAthleteSiteRepository _sites;
    private readonly IProfileLoginRepository _logins;
    private readonly IUserRepository _users;
    private readonly PermissionResolver _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public TransferControlCommandHandler(
        IAthleteSiteRepository sites,
        IProfileLoginRepository logins,
        IUserRepository users,
        PermissionResolver permissions,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _sites = sites;
        _logins = logins;
        _users = users;
        _permissions = permissions;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<Unit>> Handle(TransferControlCommand request, CancellationToken cancellationToken)
    {
        if (request.TransferTo is not (ToAthlete or ToGuardian))
            return Error.FromCode(ErrorCodes.TransferTargetInvalid);

        var profile = await _sites.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
            return Error.FromCode(ErrorCodes.SiteNotFound);

        var caller = await _users.GetByAuthProviderIdAsync(request.CallerAuthProviderId, cancellationToken);
        if (caller is null)
            return Error.FromCode(ErrorCodes.NotAuthorized);

        var login = await _logins.GetActiveLoginAsync(caller.Id, request.ProfileId, cancellationToken);
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

            profile.ControlModeId = ControlMode.AthleteControlled.Id;
        }
        else
        {
            if (!await _logins.HasActiveGuardianLoginAsync(request.ProfileId, cancellationToken))
                return Error.FromCode(ErrorCodes.NoGuardianLoginExists);

            profile.ControlModeId = ControlMode.GuardianControlled.Id;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
