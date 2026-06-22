using MediatR;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Consent;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Features.Individuals.Consent;

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
    string Email) : IRequest<Result<Guid?>>;

public class RecordGuardianConsentCommandHandler : IRequestHandler<RecordGuardianConsentCommand, Result<Guid?>>
{
    private readonly IIndividualProfileRepository _profiles;
    private readonly IGuardianConsentRepository _consents;
    private readonly UserProvisioner _userProvisioner;
    private readonly TermsOptions _terms;
    private readonly IUnitOfWork _unitOfWork;

    public RecordGuardianConsentCommandHandler(
        ISiteRepository sites,
        IGuardianConsentRepository consents,
        UserProvisioner userProvisioner,
        IOptions<TermsOptions> terms,
        IUnitOfWork unitOfWork,
        IIndividualProfileRepository profiles)
    {
        _consents = consents;
        _userProvisioner = userProvisioner;
        _terms = terms.Value;
        _unitOfWork = unitOfWork;
        _profiles = profiles;
    }

    public async Task<Result<Guid?>> Handle(RecordGuardianConsentCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
            return Error.FromCode(ErrorCodes.IndividualProfileNotFound);

        // Idempotent + scoped: only a profile awaiting consent transitions. Already-Consented or
        // NotRequired profiles need no consent — an empty success (nothing recorded).
        if (profile.ConsentStateId != ConsentStates.PendingGuardianConsent.Id)
            return Result<Guid?>.Success(null);

        // The authenticated guardian — resolved/provisioned from verified token claims.
        var guardian = await _userProvisioner.GetOrCreateAsync(request.Email, request.AuthProviderId, cancellationToken);
        var consent = new GuardianConsent
        {
            IndividualProfileId = profile.Id,
            GuardianUserId = guardian.Id,          // WHO //TODO: who needs to contain the guardians name
            MethodId = ConsentMethods.VerifiedEmail.Id,  // HOW
            TermsVersion = _terms.CurrentVersion,  // WHAT
        };

        _consents.Add(consent);
        
        profile.ConsentStateId = ConsentStates.Consented.Id; // lifts the publish gate

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return consent.Id; // created consent id → 200 with the id
    }
}
