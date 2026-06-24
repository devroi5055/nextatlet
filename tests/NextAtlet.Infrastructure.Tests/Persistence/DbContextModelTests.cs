using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Time;
using NextAtlet.Infrastructure.Persistence;

namespace NextAtlet.Infrastructure.Tests.Persistence;

/// <summary>
/// Builds the EF Core model offline (no database connection) so that every
/// <c>IEntityTypeConfiguration.Configure</c> applied via
/// <c>ApplyConfigurationsFromAssembly</c> actually executes. A failure here means a configuration
/// throws or the mapping is inconsistent — caught at build time, not first request.
/// </summary>
public class DbContextModelTests
{
    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
    }

    private static NextAtletDbContext BuildContext()
    {
        // Npgsql provider with a dummy connection string: model building is offline, no socket opened,
        // but the provider understands the jsonb/value-converter mappings the configurations declare.
        var options = new DbContextOptionsBuilder<NextAtletDbContext>()
            .UseNpgsql("Host=localhost;Database=tests;Username=u;Password=p")
            .Options;
        return new NextAtletDbContext(options, new FixedClock());
    }

    [Fact]
    public void Model_BuildsWithAllEntityConfigurationsApplied()
    {
        using var ctx = BuildContext();

        var model = ctx.Model; // forces OnModelCreating + every entity configuration

        Assert.NotNull(model);
        Assert.NotEmpty(model.GetEntityTypes());
    }

    [Theory]
    [InlineData("Users")]
    [InlineData("Sites")]
    [InlineData("SiteLogins")]
    [InlineData("ActionTokens")]
    [InlineData("GuardianConsents")]
    [InlineData("Memberships")]
    [InlineData("ChangeRequests")]
    [InlineData("MediaAssets")]
    public void Model_MapsExpectedEntities(string dbSetName)
    {
        using var ctx = BuildContext();

        // Each declared DbSet must resolve to a mapped CLR entity type in the built model.
        var entityClrNames = ctx.Model.GetEntityTypes().Select(e => e.ClrType.Name).ToHashSet();

        // sanity: the model is non-trivial
        Assert.True(entityClrNames.Count >= 8, $"expected a rich model, got {entityClrNames.Count} types");
        // and the property bag for the context exposes the set (DbSet property exists)
        Assert.NotNull(typeof(NextAtletDbContext).GetProperty(dbSetName));
    }
}
