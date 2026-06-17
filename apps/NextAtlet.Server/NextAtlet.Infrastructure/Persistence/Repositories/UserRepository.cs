using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Interfaces.Repositories;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NextAtletDbContext _context;

    public UserRepository(NextAtletDbContext context) => _context = context;

    public Task<User?> GetByAuthProviderIdAsync(string authProviderId, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(u => u.AuthProviderId == authProviderId, cancellationToken);

    public void Add(User user) => _context.Users.Add(user);
}
