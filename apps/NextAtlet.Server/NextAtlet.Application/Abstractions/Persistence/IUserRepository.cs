using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Identity;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByAuthProviderIdAsync(string authProviderId, CancellationToken cancellationToken = default);
    void Add(User user);
}
