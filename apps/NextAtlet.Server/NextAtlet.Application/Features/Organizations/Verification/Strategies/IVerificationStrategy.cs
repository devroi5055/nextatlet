using NextAtlet.Domain.Enumerations.Organization;
using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Application.Features.OrganizationSites.Verification.Strategies
{
    public interface IVerificationStrategy
    {
        public string MethodId { get; }
        Task<VerificationOutcome> InitiateAsync (Guid orgSiteId, string? providedCvr, CancellationToken ct);
    }
    public record VerificationOutcome(
        bool CompletedImmediatly,
        string? VerificationToken
        );
}
