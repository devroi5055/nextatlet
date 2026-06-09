using MediatR;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Features.Account;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// A guardian gives consent (GDPR Art. 8) for a minor's profile by following the emailed link and
/// authenticating. This single step records the <see cref="GuardianConsent"/> audit row (who / how /
/// what-version / when) and lifts the publish gate — it does NOT make the guardian a member of the
/// profile (that's a separate, owner-initiated invitation). The guardian authenticating + confirming
/// IS the consent act; identity comes from the validated token, never the body.
/// </summary>
public record RecordGuardianConsentCommand(
    Guid ProfileId,
    string AuthProviderId,
    string Email) : IRequest;

public class RecordGuardianConsentCommandHandler : IRequestHandler<RecordGuardianConsentCommand>
{
    private readonly IAthleteProfileRepository _profiles;
    private readonly IGuardianConsentRepository _consents;
    private readonly UserProvisioner _userProvisioner;
    private readonly TermsOptions _terms;
    private readonly IUnitOfWork _unitOfWork;

    public RecordGuardianConsentCommandHandler(
        IAthleteProfileRepository profiles,
        IGuardianConsentRepository consents,
        UserProvisioner userProvisioner,
        IOptions<TermsOptions> terms,
        IUnitOfWork unitOfWork)
    {
        _profiles = profiles;
        _consents = consents;
        _userProvisioner = userProvisioner;
        _terms = terms.Value;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RecordGuardianConsentCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new DomainException(ErrorCodes.ProfileNotFound);

        // Idempotent + scoped: only a profile awaiting consent transitions. Already-Consented or
        // NotRequired profiles are a no-op (no duplicate audit row).
        if (profile.ConsentState != ConsentState.PendingGuardianConsent)
            return;

        // The authenticated guardian — resolved/provisioned from verified token claims.
        var guardian = await _userProvisioner.GetOrCreateAsync(request.Email, request.AuthProviderId, cancellationToken);

        _consents.Add(new GuardianConsent
        {
            AthleteProfileId = profile.Id,
            GuardianUserId = guardian.Id,          // WHO //TODO: who needs to contain the guardians name
            Method = ConsentMethod.VerifiedEmail,  // HOW
            TermsVersion = _terms.CurrentVersion,  // WHAT
            ConsentedUtc = DateTime.UtcNow         // WHEN
        });

        profile.ConsentState = ConsentState.Consented; // lifts the publish gate

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
