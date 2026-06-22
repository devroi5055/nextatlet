using NextAtlet.Application.Features.OrganizationSites.Verification.Strategies;
using NextAtlet.Application.Interfaces.Services;
using NextAtlet.Domain.Enumerations.Organization;

public class CvrVerificationStrategy : IVerificationStrategy
{
    public string MethodId => VerificationMethod.CVR.Id;

    private readonly ICvrLookupService _cvrLookupService;

    public CvrVerificationStrategy(ICvrLookupService cvrLookupService)
    {
        _cvrLookupService = cvrLookupService;
    }

    public async Task<VerificationOutcome> InitiateAsync(Guid orgSiteId, string? providedCvr, CancellationToken ct)
    {
        var result = await _cvrLookupService.LookupAsync(providedCvr!, ct);
        return new VerificationOutcome(true, null);
    }
}
