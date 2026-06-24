using MediatR;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;

namespace NextAtlet.Application.Features.Organizations.Verification;

/// <summary>
/// Starts the email-to-official organization verification: issues an <see cref="ActionToken"/> of type
/// <see cref="ActionTokenType.OrgEmailVerification"/> and emails its accept link to the official's
/// address taken from the <b>trusted ClubRegistry</b> (never client-supplied — the registry is the
/// authority basis). Completion happens through the shared action-token accept, which flips the org to
/// Verified. Other methods (manual, MitID) reach Verified without a token and are not built here.
/// </summary>
public record SendOfficialEmailVerificationCommand(string AuthProviderId, string Email, Guid OrgSiteId, Guid ClubOfficialId) : IRequest<Result<Guid>>;

public class SendOfficialEmailVerificationCommandHandler : IRequestHandler<SendOfficialEmailVerificationCommand, Result<Guid>>
{
    private readonly IClubRepository _clubs;
    private readonly IActionTokenRepository _tokens;
    private readonly IEmailService _email;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly InvitationOptions _options;
    private readonly UserProvisioner _userProvisioner;

    public SendOfficialEmailVerificationCommandHandler(
        IClubRepository clubs,
        IActionTokenRepository tokens,
        IEmailService email,
        IUnitOfWork unitOfWork,
        IClock clock,
        IOptions<InvitationOptions> options)
    {
        _clubs = clubs;
        _tokens = tokens;
        _email = email;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<Result<Guid>> Handle(SendOfficialEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        // The official — and crucially their email — comes from the trusted registry, never the caller.
        var official = await _clubs.GetOfficialByIdAsync(request.ClubOfficialId, cancellationToken);
        if (official is null)
            return Error.FromCode(ErrorCodes.VerificationOfficialNotFound);
        if (string.IsNullOrWhiteSpace(official.Email))
            return Error.FromCode(ErrorCodes.VerificationOfficialEmailMissing);

        var user = await _userProvisioner.GetAsync(request.AuthProviderId, cancellationToken);
        var payload = new OrgEmailVerificationPayload
        {
            ClubOfficialId = official.Id,
            UserId = user?.Id,
            Email = request.Email,
        };

        var token = ActionToken.Issue(
            ActionTokenType.OrgEmailVerification.Id,
            request.OrgSiteId,
            payload,
            expiresUtc: _clock.UtcNow.AddDays(_options.ExpiryDays));
        _tokens.Add(token);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Sent after commit, to the registry-sourced address (never an address from the request body).
        await _email.SendOrgVerificationAsync(official.Email, token.Id, cancellationToken);

        return token.Id;
    }
}
