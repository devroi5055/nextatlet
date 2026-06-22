using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Media;

namespace NextAtlet.Infrastructure.Persistence.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.StorageKey).IsRequired().HasMaxLength(512);
        entity.Property(e => e.AltText).HasMaxLength(512);

        //SIMPLE SCALARS
        entity.Property(e => e.IsClubBranding).IsRequired().HasDefaultValue(false);
        entity.Property(e => e.Width);
        entity.Property(e => e.Height);

        //ENUM
        entity.Property(e => e.TypeId).IsRequired().HasMaxLength(20);
        entity.Property(e => e.OriginId).IsRequired().HasMaxLength(50).HasDefaultValue(MediaOrigin.SelfUpload.Id);

        //RELATIONS N:1 — AthleteSiteId is the Site FK (legacy name); the AthleteSite nav is stale.
        entity.Ignore(e => e.AthleteSite);
        entity.HasOne<Site>()
            .WithMany(s => s.MediaAssets)
            .HasForeignKey(e => e.AthleteSiteId)
            .OnDelete(DeleteBehavior.Cascade);

        //INDEXES
        entity.HasIndex(e => e.AthleteSiteId);
    }
}
