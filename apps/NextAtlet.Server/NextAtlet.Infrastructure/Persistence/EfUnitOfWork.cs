using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>. Shares the scoped DbContext with the
/// repositories, so a single SaveChangesAsync commits everything they staged.
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    private readonly NextAtletDbContext _context;

    public EfUnitOfWork(NextAtletDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
