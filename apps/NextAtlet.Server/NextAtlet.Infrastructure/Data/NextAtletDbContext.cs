using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Common.Time;
using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Verification;

namespace NextAtlet.Infrastructure.Data;

public class NextAtletDbContext : DbContext
{
    private readonly IClock _clock;

    public NextAtletDbContext(DbContextOptions<NextAtletDbContext> options, IClock clock) 
        : base(options) 
    {
        _clock = clock;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<IndividualProfile> IndividualProfiles { get; set; }
    public DbSet<OrganizationProfile> OrganizationProfiles { get; set; }
    public DbSet<Site> Sites { get; set; }
    public DbSet<Theme> Themes { get; set; }
    public DbSet<SiteLogin> SiteLogins { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<GuardianConsent> GuardianConsents { get; set; }
    public DbSet<SiteSnapshot> SiteSnapshots { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<ChangeRequest> ChangeRequests { get; set; }
    public DbSet<Membership> Memberships{ get; set; }

    //scraped data
    public DbSet<Club> Clubs { get; set; }
    public DbSet<ClubOfficial> ClubOfficials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NextAtletDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<CreatedOnlyEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedUtc).CurrentValue = _clock.UtcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedUtc).CurrentValue = _clock.UtcNow;
                entry.Property(e => e.UpdatedUtc).CurrentValue = _clock.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedUtc).CurrentValue = _clock.UtcNow;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

}