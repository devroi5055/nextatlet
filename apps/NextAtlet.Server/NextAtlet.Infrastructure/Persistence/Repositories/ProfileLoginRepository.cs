using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class ProfileLoginRepository : IProfileLoginRepository
{
    private readonly NextAtletDbContext _context;

    public ProfileLoginRepository(NextAtletDbContext context) => _context = context;

    public void Add(ProfileLogin login) => _context.ProfileLogins.Add(login);
}
