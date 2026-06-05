using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Account;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Application.Features.Invitations.Commands;

/// <summary>
/// The invited person claims their login. Role-agnostic — the invitation Id lookup carries everything:
/// who was invited, to which profile, in what role. The ProfileLogin (the materialized credential) is
/// created here, at accept time, not at invite time — so a revoked/expired invite never leaves a
/// dangling login. Identity comes from the validated token (controller), never the body.
/// </summary>
public record AcceptInvitationCommand(
    Guid InvitationId,
    string AuthProviderId,
    string Email) : IRequest<InvitationAcceptedDto>;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, InvitationAcceptedDto>
{
    private readonly IInvitationRepository _invitations;
    private readonly IProfileLoginRepository _logins;
    private readonly UserProvisioner _userProvisioner;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptInvitationCommandHandler(
        IInvitationRepository invitations,
        IProfileLoginRepository logins,
        UserProvisioner userProvisioner,
        IUnitOfWork unitOfWork)
    {
        _invitations = invitations;
        _logins = logins;
        _userProvisioner = userProvisioner;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvitationAcceptedDto> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invite = await _invitations.GetByIdAsync(request.InvitationId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.InvitationNotFound);

        // Expiry is checked on use (no background sweeper needed for MVP).
        if (invite.IsExpired)
            throw new DomainException(ErrorCodes.InvitationExpired);

        if (invite.Status != InvitationStatus.Pending)
            throw new DomainException(ErrorCodes.InvitationAlreadyUsed);

        // Email match is the implicit proof of inbox access — the low-risk security gate.
        if (!string.Equals(invite.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            throw new DomainException(ErrorCodes.InvitationEmailMismatch);

        // GetOrCreate the user — may already exist (returning user) or be new.
        var user = await _userProvisioner.GetOrCreateAsync(request.Email, request.AuthProviderId, cancellationToken);

        // Materialize the ProfileLogin (Active) with the role the invitation specified. Permissions
        // are derived from the profile's ControlMode at request time — none are stored here.
        _logins.Add(invite.RoleId == ProfileRole.Guardian.Id
            ? ProfileLogin.CreateGuardian(user.Id, invite.TargetProfileId)
            : ProfileLogin.CreateOwner(user.Id, invite.TargetProfileId));

        invite.Accept();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new InvitationAcceptedDto(invite.TargetProfileId, invite.RoleId);
    }
}
