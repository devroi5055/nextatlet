using MediatR;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Interfaces.Services;
using NextAtlet.Domain.Enumerations.Organization;
using System.Text.Json;

public record RequestVerificationMethodsCommand(Guid OrgSiteId, string MethodId, string? CvrNumber) : IRequest<Result<List<VerificationMethod>>>;

public class RequestVerificationMethodsCommandHandler : IRequestHandler<RequestVerificationMethodsCommand, Result<List<VerificationMethod>>>
{
    private readonly ICvrLookupService _cvrLookupService;

    public RequestVerificationMethodsCommandHandler(ICvrLookupService cvrLookupService)
        => _cvrLookupService = cvrLookupService;

    public async Task<Result<List<VerificationMethod>>> Handle(RequestVerificationMethodsCommand r, CancellationToken ct)
    {
        var availableMethods = new List<VerificationMethod>() { VerificationMethod.Manual };
        var json = await _cvrLookupService.LookupAsync(r.CvrNumber!, ct);
        if (json == null) 
            return Result<List<VerificationMethod>>.Success(availableMethods);

        json.Value.TryGetProperty("email", out var email);
        json.Value.TryGetProperty("phone", out var phone);

        if (email.ValueKind != JsonValueKind.Null)
            availableMethods.Add(VerificationMethod.Email);

        if (phone.ValueKind != JsonValueKind.Null)
            availableMethods.Add(VerificationMethod.Phone);


        return Result<List<VerificationMethod>>.Success(availableMethods);
    }
}