using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Interfaces.Repositories;
using NextAtlet.Application.Interfaces.Services;
using NextAtlet.Application.Interfaces.Strategies;

namespace NextAtlet.Application.Features.Clubs.Commands;

public record ScrapeClubsCommand(string Sport, string Country) : IRequest<string>;

public class ScrapeClubsCommandHandler : IRequestHandler<ScrapeClubsCommand, string>
{
    private readonly IClubCanonicalizer _canonicalizer;
    private readonly IClubRepository _clubs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IClubSourceStrategy> _strategies;

    public ScrapeClubsCommandHandler(
        IEnumerable<IClubSourceStrategy> strategies,
        IClubCanonicalizer canonicalizer,
        IClubRepository clubs,
        IUnitOfWork unitOfWork)
    {
        _canonicalizer = canonicalizer;
        _clubs = clubs;
        _unitOfWork = unitOfWork;
        _strategies = strategies;
    }

    public async Task<string> Handle(ScrapeClubsCommand request, CancellationToken ct)
    {
        var strategies = _strategies.Where(s => s.Supports(request.Sport, request.Country)).ToList();

        var total = 0;
        foreach (var strategy in strategies)
        {
            var scraped = await strategy.FetchAsync(ct);

            foreach (var club in scraped)
                await _clubs.UpsertAsync(_canonicalizer.Canonicalize(club), ct);

            await _clubs.DeactivateMissingAsync(strategy.Source, scraped.Select(c => c.SourceKey), ct);
            total += scraped.Length;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return $"Imported {total} clubs from {strategies.Count} source(s).";
    }
}
