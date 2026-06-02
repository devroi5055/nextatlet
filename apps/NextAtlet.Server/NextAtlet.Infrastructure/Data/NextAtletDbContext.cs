using Microsoft.EntityFrameworkCore;
using NextAtlet.Domain.Entities;

namespace NextAtlet.Infrastructure.Data;

public class NextAtletDbContext : DbContext
{
    public NextAtletDbContext(DbContextOptions<NextAtletDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<AthleteProfile> AthleteProfiles { get; set; }
    public DbSet<ProfileLogin> ProfileLogins { get; set; }
    public DbSet<Theme> Themes { get; set; }
    public DbSet<SiteConfig> SiteConfigs { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.AuthProviderId).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.AuthProviderId).IsUnique();
        });

        // AthleteProfile configuration
        modelBuilder.Entity<AthleteProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Sport).IsRequired().HasMaxLength(50).HasDefaultValue("judo");
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.DefaultLocale).IsRequired().HasMaxLength(2).HasDefaultValue("da");
            entity.Property(e => e.VisibilityState).IsRequired().HasMaxLength(20).HasDefaultValue("Public");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Sport);
            entity.HasIndex(e => e.CreatedUtc).IsDescending();
        });

        // ProfileLogin configuration
        modelBuilder.Entity<ProfileLogin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            entity.HasIndex(e => new { e.UserId, e.AthleteProfileId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AthleteProfileId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ProfileLogins)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AthleteProfile)
                .WithMany(ap => ap.ProfileLogins)
                .HasForeignKey(e => e.AthleteProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Theme configuration
        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Version).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.Manifest).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.MinimumCapability).HasColumnType("jsonb");
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        // SiteConfig configuration
        modelBuilder.Entity<SiteConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.State).IsRequired().HasMaxLength(20).HasDefaultValue("Draft");
            entity.Property(e => e.ThemeVersion).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.Layout).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.GlobalSettings).HasColumnType("jsonb");
            entity.Property(e => e.Version).IsRequired().HasDefaultValue(1);
            entity.HasIndex(e => new { e.AthleteProfileId, e.State }).IsUnique();
            entity.HasIndex(e => e.UpdatedUtc).IsDescending();

            entity.HasOne(e => e.AthleteProfile)
                .WithMany(ap => ap.SiteConfigs)
                .HasForeignKey(e => e.AthleteProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Theme)
                .WithMany(t => t.SiteConfigs)
                .HasForeignKey(e => e.ThemeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MediaAsset configuration
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Origin).IsRequired().HasMaxLength(50).HasDefaultValue("SelfUpload");
            entity.Property(e => e.IsClubBranding).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.StorageKey).IsRequired().HasMaxLength(512);
            entity.Property(e => e.AltText).HasMaxLength(512);
            entity.HasIndex(e => e.AthleteProfileId);

            entity.HasOne(e => e.AthleteProfile)
                .WithMany(ap => ap.MediaAssets)
                .HasForeignKey(e => e.AthleteProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedTheme(modelBuilder);
    }

    private void SeedTheme(ModelBuilder modelBuilder)
    {
        var classicThemeId = new Guid("11111111-1111-1111-1111-111111111111");

        var classicManifest = new Dictionary<string, object>
        {
            { "name", "Classic" },
            { "supportedSections", new[] { "hero", "bio" } },
            { "colorSlots", new[] { "primary", "secondary", "accent" } },
            { "fontSlots", new[] { "headingFont", "bodyFont" } }
        };

        modelBuilder.Entity<Theme>().HasData(new Theme
        {
            Id = classicThemeId,
            Name = "Classic",
            Version = 1,
            Manifest = classicManifest,
            MinimumCapability = new Dictionary<string, object>(),
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });
    }
}
