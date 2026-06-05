using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Domain.Authorization;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Toggles shared editing (collaboration). Does NOT change who controls — only whether the other
/// party may edit the draft (+ media). Flips only the shared flag of whichever side currently
/// controls. Only the current controller may toggle it. Identity comes from the token, never the body.
/// </summary>
public record SetCollaborationCommand(
    Guid ProfileId,
    string CallerAuthProviderId,
    bool SharedEditing) : IRequest;

public class SetCollaborationCommandHandler : IRequestHandler<SetCollaborationCommand>
{
    private readonly IAthleteProfileRepository _profiles;
    private readonly IProfileLoginRepository _logins;
    private readonly IUserRepository _users;
    private readonly PermissionResolver _permissions;
    private readonly IUnitOfWork _unitOfWork;

    public SetCollaborationCommandHandler(
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IUserRepository users,
        PermissionResolver permissions,
        IUnitOfWork unitOfWork)
    {
        _profiles = profiles;
        _logins = logins;
        _users = users;
        _permissions = permissions;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetCollaborationCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.ProfileNotFound);

        var caller = await _users.GetByAuthProviderIdAsync(request.CallerAuthProviderId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.NotAuthorized);

        var login = await _logins.GetActiveLoginAsync(caller.Id, request.ProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.NotAuthorized);

        if (!_permissions.IsController(login, profile))
            throw new DomainException(ErrorCodes.NotAuthorized);

        // Flip only the shared flag of the currently-controlling side; already-in-state is a no-op.
        profile.ControlMode = (profile.ControlMode, request.SharedEditing) switch
        {
            (ControlMode.AthleteControlled, true) => ControlMode.AthleteControlledShared,
            (ControlMode.AthleteControlledShared, false) => ControlMode.AthleteControlled,
            (ControlMode.GuardianControlled, true) => ControlMode.GuardianControlledShared,
            (ControlMode.GuardianControlledShared, false) => ControlMode.GuardianControlled,
            var (current, _) => current
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
