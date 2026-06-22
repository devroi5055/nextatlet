using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Entities.ClubRegistry;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IClubRepository
{
    Task<Club?> GetClubByIdAsync(string id, CancellationToken ct);
    Task<Club> UpsertClubAsync(ScrapedClub club, CancellationToken ct);
    Task<ClubOfficial?> GetOfficialByIdAsync(string id, CancellationToken ct);
    Task DeactivateMissingClubAsync(string source, IEnumerable<string> presentKeys, CancellationToken ct);

}
