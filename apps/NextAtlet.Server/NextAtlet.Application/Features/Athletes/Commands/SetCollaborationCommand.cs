using MediatR;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Enumerations.AthleteProfile;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Toggles shared editing (collaboration). Does NOT change who controls — only whether the other
/// party may edit the draft (+ media). Flips only the shared flag of whichever side currently
/// controls. Only the current controller may toggle it. Identity comes from the token, never the body.
/// </summary>
public record SetCollaborationCommand(
    Guid ProfileId,
    string CallerAuthProviderId,
    bool SharedEditing) : IRequest<Result<Unit>>;

public class SetCollaborationCommandHandler : IRequestHandler<SetCollaborationCommand, Result<Unit>>
{
    private readonly IAthleteSiteRepository _sites;
    private readonly IProfileLoginRepository _logins;
    private readonly IUserRepository _users;
    private readonly PermissionResolver _permissions;
    private readonly IUnitOfWork _unitOfWork;

    public SetCollaborationCommandHandler(
        IAthleteSiteRepository sites,
        IProfileLoginRepository logins,
        IUserRepository users,
        PermissionResolver permissions,
        IUnitOfWork unitOfWork)
    {
        _sites = sites;
        _logins = logins;
        _users = users;
        _permissions = permissions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(SetCollaborationCommand request, CancellationToken cancellationToken)
    {
        var profile = await _sites.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
            return Error.FromCode(ErrorCodes.SiteNotFound);

        var caller = await _users.GetByAuthProviderIdAsync(request.CallerAuthProviderId, cancellationToken);
        if (caller is null)
            return Error.FromCode(ErrorCodes.NotAuthorized);

        var login = await _logins.GetActiveLoginAsync(caller.Id, request.ProfileId, cancellationToken);
        if (login is null || !_permissions.IsController(login, profile))
            return Error.FromCode(ErrorCodes.NotAuthorized);

        // Flip only the shared flag of the currently-controlling side; already-in-state is a no-op.
        profile.ControlModeId = (profile.ControlModeId, request.SharedEditing) switch
        {
            ("athlete_controlled",         true)  => ControlMode.AthleteControlledShared.Id,
            ("athlete_controlled_shared",  false) => ControlMode.AthleteControlled.Id,
            ("guardian_controlled",        true)  => ControlMode.GuardianControlledShared.Id,
            ("guardian_controlled_shared", false) => ControlMode.GuardianControlled.Id,
            _                                     => profile.ControlModeId
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
