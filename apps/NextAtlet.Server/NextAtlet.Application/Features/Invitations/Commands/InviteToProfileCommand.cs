using MediatR;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Contracts.Invitations.Response;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Features.Invitations.Commands;

/// <summary>
/// Invite a person (by email) to an existing site in a given role. Authorization is natural: only
/// someone holding an Active login on the site may invite to it. The credential is materialized at
/// accept time (the shared action-token accept), not here — so a revoked/expired invite never leaves a
/// dangling login. Caller identity comes from the validated token (controller), never the body.
/// </summary>
public record InviteToProfileCommand(
    Guid SiteId,
    string CallerAuthProviderId,
    string CallerEmail,
    string Email,
    string RoleId) : IRequest<Result<InvitationResponse>>;

public class InviteToProfileCommandHandler : IRequestHandler<InviteToProfileCommand, Result<InvitationResponse>>
{
    private readonly IUserRepository _users;
    private readonly ISiteLoginRepository _logins;
    private readonly IIndividualProfileRepository _profiles;
    private readonly IActionTokenRepository _tokens;
    private readonly IEmailService _email;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly InvitationOptions _options;

    public InviteToProfileCommandHandler(
        IUserRepository users,
        ISiteLoginRepository logins,
        IIndividualProfileRepository profiles,
        IActionTokenRepository tokens,
        IEmailService email,
        IUnitOfWork unitOfWork,
        IClock clock,
        IOptions<InvitationOptions> options)
    {
        _users = users;
        _logins = logins;
        _profiles = profiles;
        _tokens = tokens;
        _email = email;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<Result<InvitationResponse>> Handle(InviteToProfileCommand request, CancellationToken cancellationToken)
    {
        // Role must be a known IndividualRole — reject early rather than create an unusable invite.
        if (request.RoleId != IndividualRole.Owner.Id && request.RoleId != IndividualRole.Guardian.Id)
            return Error.FromCode(ErrorCodes.InvitationRoleInvalid);

        // The caller must already be a known user; an unknown subject holds no rights anywhere.
        var caller = await _users.GetByAuthProviderIdAsync(request.CallerAuthProviderId, cancellationToken);
        if (caller is null)
            throw new InvalidOperationException("Authenticated user must have DB row");

        var site = await _profiles.GetBySiteIdAsync(request.SiteId, cancellationToken);
        if (site is null)
            return Error.FromCode(ErrorCodes.SiteNotFound);

        // Authorization: only someone with an Active login on this site may invite to it.
        if (await _logins.GetActiveLoginAsync(caller.Id, request.SiteId, cancellationToken) is null)
            return Error.FromCode(ErrorCodes.NotAuthorized);

        // A guardian only makes sense for a minor — refuse to invite one onto an adult site.
        if (request.RoleId == IndividualRole.Guardian.Id && !site.IsMinor(_clock.UtcNow))
            return Error.FromCode(ErrorCodes.GuardianCannotRegisterAdult);

        // Don't double-invite the same email+role on the same site.
        if (await _tokens.HasPendingInviteAsync(request.SiteId, request.Email, request.RoleId, cancellationToken))
            return Error.FromCode(ErrorCodes.InvitationAlreadyPending);

        var token = ActionToken.Issue(
            ActionTokenType.Invitation.Id,
            request.SiteId,
            new InvitePayload { Email = request.Email, RoleId = request.RoleId },
            expiresUtc: _clock.UtcNow.AddDays(_options.ExpiryDays));
        _tokens.Add(token);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Sent after the token is durably committed (the token is the source of truth, send is best-effort).
        await _email.SendInviteAsync(request.Email, token.Id, cancellationToken);

        return new InvitationResponse(token.Id, token.TargetSiteId, request.Email, request.RoleId, token.ExpiresUtc);
    }
}
