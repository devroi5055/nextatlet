using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class AthleteSiteSnapshotConfiguration : IEntityTypeConfiguration<AthleteSiteSnapshot>
{
    public void Configure(EntityTypeBuilder<AthleteSiteSnapshot> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //SIMPLE SCALARS
        entity.Property(e => e.ThemeVersion).IsRequired().HasDefaultValue(1);
        entity.Property(e => e.Version).IsRequired().HasDefaultValue(1);
        entity.Property(e => e.PublishedUtc);

        //JSONB
        entity.Property(e => e.Layout).HasJsonbConversion().IsRequired();
        entity.Property(e => e.GlobalSettings).HasJsonbConversion();

        //RELATIONS N:1
        entity.HasOne(e => e.AthleteSite)
            .WithMany()
            .HasForeignKey(e => e.AthleteProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.Theme)
            .WithMany()
            .HasForeignKey(e => e.ThemeId)
            .OnDelete(DeleteBehavior.Restrict);

        //INDEXES
        entity.HasIndex(e => e.AthleteProfileId);
        entity.HasIndex(e => e.CreatedUtc).IsDescending();
    }
}
