using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Features.Invitations.Commands;

/// <summary>
/// The invited person claims their login — this is purely about <i>joining</i> a profile. Role-agnostic:
/// the invitation Id lookup carries who was invited, to which profile, in what role. The ProfileLogin
/// (the materialized credential) is created here, at accept time, not at invite time — so a
/// revoked/expired invite never leaves a dangling login. Guardian consent is a separate flow (the
/// consent endpoint), never coupled to joining. Identity comes from the validated token, never the body.
/// </summary>
public record AcceptInvitationCommand(
    Guid InvitationId,
    string AuthProviderId,
    string Email) : IRequest<Result<InvitationAcceptedDto>>;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result<InvitationAcceptedDto>>
{
    private readonly IInvitationRepository _invitations;
    private readonly ISiteLoginRepository _logins;
    private readonly UserProvisioner _userProvisioner;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptInvitationCommandHandler(
        IInvitationRepository invitations,
        ISiteLoginRepository logins,
        UserProvisioner userProvisioner,
        IUnitOfWork unitOfWork)
    {
        _invitations = invitations;
        _logins = logins;
        _userProvisioner = userProvisioner;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InvitationAcceptedDto>> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invite = await _invitations.GetByIdAsync(request.InvitationId, cancellationToken);
        if (invite is null)
            return Error.FromCode(ErrorCodes.InvitationNotFound);

        // Expiry is checked on use (no background sweeper needed for MVP).
        if (invite.IsExpired)
            return Error.FromCode(ErrorCodes.InvitationExpired);

        if (invite.StatusId != InvitationStatus.Pending.Id)
            return Error.FromCode(ErrorCodes.InvitationAlreadyUsed);

        // Email match is the implicit proof of inbox access — the low-risk security gate.
        if (!string.Equals(invite.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            return Error.FromCode(ErrorCodes.InvitationEmailMismatch);

        // GetOrCreate the user — may already exist (returning user) or be new.
        var user = await _userProvisioner.GetOrCreateAsync(request.Email, request.AuthProviderId, cancellationToken);

        // Materialize the ProfileLogin (Active) with the role the invitation specified. Permissions
        // are derived from the profile's ControlMode at request time — none are stored here.
        _logins.Add(invite.RoleId == IndividualRole.Guardian.Id
            ? SiteLogin.CreateGuardian(user.Id, invite.TargetSiteId)
            : SiteLogin.CreateAthlete(user.Id, invite.TargetSiteId));

        invite.Accept();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new InvitationAcceptedDto(invite.TargetSiteId, invite.RoleId);
    }
}
