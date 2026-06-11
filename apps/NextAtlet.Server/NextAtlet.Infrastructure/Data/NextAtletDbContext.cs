using Microsoft.EntityFrameworkCore;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NextAtlet.Domain.strings;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;
using NextAtlet.Domain.ValueObjects.Theme.Builders;
using NextAtlet.Domain.ValueObjects.ThemeStyle;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace NextAtlet.Infrastructure.Data;

public class NextAtletDbContext : DbContext
{
    public NextAtletDbContext(DbContextOptions<NextAtletDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<AthleteSite> AthleteSites { get; set; }
    public DbSet<ProfileLogin> ProfileLogins { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<GuardianConsent> GuardianConsents { get; set; }
    public DbSet<Theme> Themes { get; set; }
    public DbSet<AthleteSiteSnapshot> AthleteSiteSnapshots { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.AuthProviderId).HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.AuthProviderId).IsUnique();
        });

        // AthleteProfile configuration
        modelBuilder.Entity<AthleteSite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.SportId).IsRequired().HasMaxLength(50).HasDefaultValue("judo");
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.DefaultLocaleId).IsRequired().HasMaxLength(2).HasDefaultValue("da");
            entity.Property(e => e.VisibilityStateId).IsRequired().HasMaxLength(20).HasDefaultValue("Public");
            entity.Property(e => e.ControlMode)
                .HasConversion<string>().IsRequired().HasMaxLength(30)
                .HasDefaultValue(ControlMode.AthleteControlled);
            entity.Property(e => e.ConsentState)
                .HasConversion<string>().IsRequired().HasMaxLength(30)
                .HasDefaultValue(ConsentState.NotRequired);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.SportId);
            entity.HasIndex(e => e.CreatedUtc).IsDescending();
        });

        // ProfileLogin configuration
        modelBuilder.Entity<ProfileLogin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
            entity.Property(e => e.Permissions).HasJsonbConversion();
            entity.HasIndex(e => new { e.UserId, e.AthleteProfileId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AthleteProfileId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ProfileLogins)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AthleteSite)
                .WithMany(ap => ap.ProfileLogins)
                .HasForeignKey(e => e.AthleteProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Invitation configuration
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
            entity.Property(e => e.ExpiresUtc).IsRequired();
            entity.HasIndex(e => new { e.Email, e.Status });
            entity.HasIndex(e => e.TargetProfileId);

            entity.HasOne(e => e.TargetSite)
                .WithMany()
                .HasForeignKey(e => e.TargetProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InvitedBy)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GuardianConsent configuration (GDPR Art. 8 audit record)
        modelBuilder.Entity<GuardianConsent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Method).HasConversion<string>().IsRequired().HasMaxLength(30);
            entity.Property(e => e.TermsVersion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedUtc).IsRequired();
            entity.HasIndex(e => e.AthleteProfileId);

            entity.HasOne(e => e.AthleteSite)
                .WithMany()
                .HasForeignKey(e => e.AthleteProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Guardian)
                .WithMany()
                .HasForeignKey(e => e.GuardianUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Theme configuration
        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Version).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.Manifest).HasJsonbConversion().IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        // AthleteSiteSnapshot configuration
        modelBuilder.Entity<AthleteSiteSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ThemeVersion).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.Layout).HasJsonbConversion().IsRequired();
            entity.Property(e => e.GlobalSettings).HasJsonbConversion();
            entity.Property(e => e.Version).IsRequired().HasDefaultValue(1);
            entity.HasIndex(e => e.AthleteProfileId);
            entity.HasIndex(e => e.CreatedUtc).IsDescending();

            entity.HasOne(e => e.AthleteSite)
                .WithMany()
                .HasForeignKey(e => e.AthleteProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Theme)
                .WithMany()
                .HasForeignKey(e => e.ThemeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MediaAsset configuration
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type)
                .HasConversion(t => t.Id, id => MediaAssetType.FromId(id))
                .IsRequired().HasMaxLength(20);
            entity.Property(e => e.OriginId).IsRequired().HasMaxLength(50).HasDefaultValue("self_upload");
            entity.Property(e => e.IsClubBranding).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.StorageKey).IsRequired().HasMaxLength(512);
            entity.Property(e => e.AltText).HasMaxLength(512);
            entity.HasIndex(e => e.AthleteSiteId);

            entity.HasOne(e => e.AthleteSite)
                .WithMany(ap => ap.MediaAssets)
                .HasForeignKey(e => e.AthleteSiteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedTheme(modelBuilder);
    }

    private void SeedTheme(ModelBuilder modelBuilder)
    {
        var classicThemeId = new Guid("11111111-1111-1111-1111-111111111111");

        var classicManifest = ClassicTheme.Manifest();

        modelBuilder.Entity<Theme>().HasData(new Theme
        {
            Id = classicThemeId,
            Name = "Classic",
            Version = 1,
            Manifest = classicManifest,
            IsActive = true,
        });
    }
}