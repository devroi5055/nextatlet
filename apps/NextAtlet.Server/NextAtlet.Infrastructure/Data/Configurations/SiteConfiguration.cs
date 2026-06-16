using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Organization;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.Slug).IsRequired().HasMaxLength(256);
        entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);

        //ENUM
        entity.Property(e => e.VisibilityStateId).IsRequired().HasMaxLength(50).HasDefaultValue(VisibilityStates.Public.Id);
        entity.Property(e => e.VerificationStatusId).IsRequired().HasMaxLength(50).HasDefaultValue(VerificationStatus.Pending.Id);
        entity.Property(e => e.DefaultLocaleId).IsRequired().HasMaxLength(2).HasDefaultValue(Locale.En.Id);
        entity.Property(e => e.SiteProfileId).IsRequired().HasMaxLength(50).HasDefaultValue(SiteProfiles.Athlete.Id);

        //RELATIONS N:1 — draft/published pointers (SiteLogins + MediaAssets are configured from their own sides)
        entity.HasOne(e => e.CurrentDraftSnapshot)
            .WithMany()
            .HasForeignKey(e => e.CurrentDraftSnapshotId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(e => e.CurrentPublishedSnapshot)
            .WithMany()
            .HasForeignKey(e => e.CurrentPublishedSnapshotId)
            .OnDelete(DeleteBehavior.Restrict);

        //INDEXES
        entity.HasIndex(e => e.Slug).IsUnique();
    }
}
