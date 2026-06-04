using NextAtlet.Domain.Entities.Shared;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByAuthProviderIdAsync(string authProviderId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    void Add(User user);
}
