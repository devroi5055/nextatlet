using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NextAtlet.Infrastructure.Common.Time;

namespace NextAtlet.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so EF Core CLI tooling (migrations / scaffolding) can build a
/// <see cref="NextAtletDbContext"/> without booting the Api host. The connection string only needs to
/// be parseable — <c>migrations add</c> never connects. Runtime DI still wires the real context in Program.cs.
/// </summary>
public class NextAtletDbContextFactory : IDesignTimeDbContextFactory<NextAtletDbContext>
{
    public NextAtletDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NextAtletDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=nextatlet;Username=postgres;Password=postgres")
            .Options;

        return new NextAtletDbContext(options, new SystemClock());
    }
}
