using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Infrastructure.Persistence.Configurations;

public class SiteSnapshotConfiguration : IEntityTypeConfiguration<SiteSnapshot>
{
    public void Configure(EntityTypeBuilder<SiteSnapshot> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //SIMPLE SCALARS
        entity.Property(e => e.PublishedUtc);

        //JSONB
        entity.Property(e => e.Layout).HasJsonbConversion().IsRequired();
        entity.Property(e => e.GlobalSettings).HasJsonbConversion();

        //RELATIONS N:1
        entity.HasOne(e => e.Theme)
            .WithMany()
            .HasForeignKey(e => e.ThemeId)
            .OnDelete(DeleteBehavior.Restrict);

        //INDEXES
        entity.HasIndex(e => e.SiteId);
        entity.HasIndex(e => e.CreatedUtc).IsDescending();
    }
}
