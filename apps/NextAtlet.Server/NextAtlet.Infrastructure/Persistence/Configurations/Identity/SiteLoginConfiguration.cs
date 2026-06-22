using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class SiteLoginConfiguration : IEntityTypeConfiguration<SiteLogin>
{
    public void Configure(EntityTypeBuilder<SiteLogin> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS

        //ENUM
        entity.Property(e => e.StatusId).IsRequired().HasMaxLength(20);
        entity.Property(e => e.SiteRoleId).IsRequired().HasMaxLength(50);

        //JSONB
        entity.Property(e => e.Permissions).HasJsonbConversion();

        //RELATIONS N:1
        entity.HasOne(e => e.User)
            .WithMany(u => u.SiteLogins)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.Site)
            .WithMany(s => s.SiteLogins)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        //INDEXES
        entity.HasIndex(e => new { e.UserId, e.SiteId }).IsUnique();
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => e.SiteId);
    }
}
