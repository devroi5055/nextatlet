using NextAtlet.Domain.Entities.Shared;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IThemeRepository
{
    Task<Theme?> GetActiveByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Theme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
