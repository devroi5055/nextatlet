using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Entities.ClubRegistry;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IClubRepository
{
    Task<Club?> GetClubByIdAsync(Guid id, CancellationToken ct);
    Task<Club> UpsertClubAsync(ScrapedClub club, CancellationToken ct);
    Task<ClubOfficial?> GetOfficialByIdAsync(Guid id, CancellationToken ct);
    Task DeactivateMissingClubAsync(string source, IEnumerable<string> presentKeys, CancellationToken ct);

}
