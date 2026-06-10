using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Infrastructure.Persistence.Repositories;

public class ThemeRepository : IThemeRepository
{
    private readonly NextAtletDbContext _context;

    public ThemeRepository(NextAtletDbContext context) => _context = context;

    public Task<Theme?> GetActiveByNameAsync(string name, CancellationToken cancellationToken = default)
        => _context.Themes.FirstOrDefaultAsync(t => t.Name == name && t.IsActive, cancellationToken);

    public async Task<Theme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Themes.FindAsync([id], cancellationToken);
}
