using MediatR;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.OrganizationSites.Verification.Strategies;

public record VerifyOrgCommand(Guid OrgSiteId, string MethodId, string SportId, string? CvrNumber) : IRequest<VerificationOutcome>;

public class VerifyOrgCommandHandler : IRequestHandler<VerifyOrgCommand, VerificationOutcome>
{
    private readonly IEnumerable<IVerificationStrategy> _strategies;

    public VerifyOrgCommandHandler(IEnumerable<IVerificationStrategy> strategies)
        => _strategies = strategies;

    public async Task<VerificationOutcome> Handle(VerifyOrgCommand r, CancellationToken ct)
    {
        var strategy = _strategies.SingleOrDefault(s => s.MethodId == r.MethodId)
            ?? throw new DomainException(ErrorCodes.AthleteTooYoungForControl, r.MethodId);

        return await strategy.InitiateAsync(r.OrgSiteId, r.CvrNumber, ct);
    }
}