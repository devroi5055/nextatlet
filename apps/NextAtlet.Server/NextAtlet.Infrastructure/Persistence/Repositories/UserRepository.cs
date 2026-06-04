using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NextAtletDbContext _context;

    public UserRepository(NextAtletDbContext context) => _context = context;

    public Task<User?> GetByAuthProviderIdAsync(string authProviderId, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(u => u.AuthProviderId == authProviderId, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public void Add(User user) => _context.Users.Add(user);
}
