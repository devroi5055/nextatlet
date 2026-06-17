using NextAtlet.Domain.Entities.Shared;

namespace NextAtlet.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByAuthProviderIdAsync(string authProviderId, CancellationToken cancellationToken = default);
    void Add(User user);
}
