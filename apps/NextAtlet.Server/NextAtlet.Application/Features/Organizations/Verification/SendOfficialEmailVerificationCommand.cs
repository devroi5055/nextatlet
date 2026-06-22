using MediatR;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Enumerations.Organization;
using System.Text.Json;
using NextAtlet.Application.Abstractions.Persistence;

public record SendOfficialEmailVerificationCommand(string clubOfficialId) : IRequest<Result<List<VerificationMethod>>>;

//public class SendOfficialEmailVerificationCommandHandler : IRequestHandler<SendOfficialEmailVerificationCommand, Result<List<VerificationMethod>>>
//{
//    private readonly IClubRepository _clubRepository;
//    private readonly IEmailService _emailService;


//    public SendOfficialEmailVerificationCommandHandler(IClubRepository clubRepository)
//    {
//        _clubRepository = clubRepository;
//    }

//    public async Task<Result<List<VerificationMethod>>> Handle(SendOfficialEmailVerificationCommand request, CancellationToken ct)
//    {
//        var clubOfficial = _clubRepository.GetOfficialByIdAsync(request.clubOfficialId, ct);
//        _emailService.SendOrgVerificationAsync();



//    }
//}