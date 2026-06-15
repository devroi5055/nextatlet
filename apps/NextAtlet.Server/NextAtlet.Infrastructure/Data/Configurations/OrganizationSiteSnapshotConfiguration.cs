using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Organization;
using NextAtlet.Domain.Entities.Shared;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class OrganizationSiteSnapshotConfiguration : IEntityTypeConfiguration<OrganizationSiteSnapshot>
{
    public void Configure(EntityTypeBuilder<OrganizationSiteSnapshot> entity)
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
        entity.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.Theme)
            .WithMany()
            .HasForeignKey(e => e.ThemeId)
            .OnDelete(DeleteBehavior.Restrict);

        //INDEXES
        entity.HasIndex(e => e.OrganizationId);
        entity.HasIndex(e => e.CreatedUtc).IsDescending();
    }
}
