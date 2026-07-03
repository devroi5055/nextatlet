using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Infrastructure.Persistence;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class ThemeRepository : IThemeRepository
{
    private readonly NextAtletDbContext _context;

    public ThemeRepository(NextAtletDbContext context) => _context = context;

    public Task<Theme?> GetActiveByNameAsync(string name, CancellationToken cancellationToken = default)
        => _context.Themes.FirstOrDefaultAsync(t => t.Name == name && t.RetiredUtc == null, cancellationToken);

    public async Task<Theme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Themes.FindAsync([id], cancellationToken);
}
