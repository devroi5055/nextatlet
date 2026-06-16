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
    Guid SiteId,
    string CallerAuthProviderId,
    bool SharedEditing) : IRequest<Result<Unit>>;

public class SetCollaborationCommandHandler : IRequestHandler<SetCollaborationCommand, Result<Unit>>
{
    private readonly ISiteRepository _sites;
    private readonly ISiteLoginRepository _logins;
    private readonly IAthleteProfileRepository _profiles;
    private readonly IUserRepository _users;
    private readonly PermissionResolver _permissions;
    private readonly IUnitOfWork _unitOfWork;

    public SetCollaborationCommandHandler(
        ISiteRepository sites,
        ISiteLoginRepository logins,
        IUserRepository users,
        PermissionResolver permissions,
        IUnitOfWork unitOfWork,
        IAthleteProfileRepository profiles)
    {
        _sites = sites;
        _logins = logins;
        _users = users;
        _permissions = permissions;
        _unitOfWork = unitOfWork;
        _profiles = profiles;
    }

    public async Task<Result<Unit>> Handle(SetCollaborationCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetBySiteIdAsync(request.SiteId, cancellationToken);
        if (profile == null) 
            return Error.FromCode(ErrorCodes.AthleteProfileNotFound);

        var caller = await _users.GetByAuthProviderIdAsync(request.CallerAuthProviderId, cancellationToken);
        if (caller is null)
            return Error.FromCode(ErrorCodes.NotAuthorized);

        var login = await _logins.GetActiveLoginAsync(caller.Id, request.SiteId, cancellationToken);
        if (login is null || !_permissions.IsController(login, profile))
            return Error.FromCode(ErrorCodes.NotAuthorized);

        // Flip only the shared flag of the currently-controlling side; already-in-state is a no-op.
        profile.ControlModeId = (profile.ControlModeId, request.SharedEditing) switch
        {
            (var id, true) when id == ControlModes.AthleteControlled.Id => ControlModes.AthleteControlledShared.Id,
            (var id, false) when id == ControlModes.AthleteControlledShared.Id => ControlModes.AthleteControlled.Id,
            (var id, true) when id == ControlModes.GuardianControlled.Id => ControlModes.GuardianControlledShared.Id,
            (var id, false) when id == ControlModes.GuardianControlledShared.Id => ControlModes.GuardianControlled.Id,
            _ => profile.ControlModeId
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
