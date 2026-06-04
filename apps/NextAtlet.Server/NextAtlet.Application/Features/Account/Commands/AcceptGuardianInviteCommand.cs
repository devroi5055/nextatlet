using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;

namespace NextAtlet.Application.Features.Account.Commands;

/// <summary>
/// The invited guardian explicitly accepts guardianship. Claims their (unclaimed) user row by
/// matching the token's email, binds the IdP subject, and activates their Pending guardian logins
/// — the deliberate, auditable consent step that lets them publish/approve (`03` §1a).
/// Identity comes from the validated token (controller), never the body.
/// </summary>
public record AcceptGuardianInviteCommand(string AuthProviderId, string Email) : IRequest<GuardianshipAcceptedDto>;

public class AcceptGuardianInviteCommandHandler : IRequestHandler<AcceptGuardianInviteCommand, GuardianshipAcceptedDto>
{
    private readonly IUserRepository _users;
    private readonly IProfileLoginRepository _logins;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptGuardianInviteCommandHandler(
        IUserRepository users,
        IProfileLoginRepository logins,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _logins = logins;
        _unitOfWork = unitOfWork;
    }

    public async Task<GuardianshipAcceptedDto> Handle(AcceptGuardianInviteCommand request, CancellationToken cancellationToken)
    {
        // Match by subject (already linked) or by the invited email (not yet claimed).
        var user = await _users.GetByAuthProviderIdAsync(request.AuthProviderId, cancellationToken)
            ?? await _users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new DomainException(ErrorCodes.GuardianInviteNotFound);

        var pending = await _logins.GetPendingGuardianLoginsByUserIdAsync(user.Id, cancellationToken);
        if (pending.Count == 0)
            throw new DomainException(ErrorCodes.GuardianInviteNotFound);

        user.AuthProviderId ??= request.AuthProviderId; // bind the subject (claim the row)
        foreach (var login in pending)
            login.Accept();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new GuardianshipAcceptedDto(pending.Count);
    }
}
