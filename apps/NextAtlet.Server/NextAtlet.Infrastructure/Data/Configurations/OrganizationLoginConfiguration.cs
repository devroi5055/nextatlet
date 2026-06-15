using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Organization;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations.Organization;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class OrganizationLoginConfiguration : IEntityTypeConfiguration<OrganizationLogin>
{
    public void Configure(EntityTypeBuilder<OrganizationLogin> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS

        //ENUM
        entity.Property(e => e.StatusId).IsRequired().HasMaxLength(20);
        entity.Property(e => e.OrganizationRoleId).IsRequired().HasMaxLength(50);

        //RELATIONS N:1
        entity.HasOne(e => e.Organization)
            .WithMany(o => o.Logins)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        //INDEXES
        entity.HasIndex(e => e.OrganizationId);
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
    }
}
