using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Entities.Verification;

namespace NextAtlet.Application.Interfaces.Repositories;

public interface IClubRepository
{
    Task<Club?> GetByIdAsync(string  id, CancellationToken ct);
    Task<Club> UpsertAsync(ScrapedClub club, CancellationToken ct);
    Task DeactivateMissingAsync(string source, IEnumerable<string> presentKeys, CancellationToken ct);

}
